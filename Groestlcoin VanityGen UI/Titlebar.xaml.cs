using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Groestlcoin_VanityGen_UI {
    /// <summary>
    /// Interaction logic for Titlebar.xaml
    /// </summary>
    public partial class Titlebar : UserControl {

        public Titlebar() {
            InitializeComponent();
            ThemeSelector.ThemeSelector.SetCurrentThemeDictionary(this, new Uri((uxThemeSelector.SelectedItem as ComboBoxItem).Tag.ToString(), UriKind.Relative));
        }
        private bool _isMainWindow = false;

        public bool IsMainWindow {
            get {
                return _isMainWindow;
            }
            set {
                if (value == false) {
                    uxMinimiseBtn.Visibility = Visibility.Hidden;
                    uxMinimiseBtn.IsEnabled = false;
                }
                _isMainWindow = value;
            }
        }
        private bool _isAbout = false;

        public bool IsAbout {
            get {
                return _isAbout;
            }
            set {
                if (value) {
                    uxMinimiseBtn.Visibility = Visibility.Hidden;
                    uxMinimiseBtn.IsEnabled = false;
                    uxAboutBtn.Visibility = Visibility.Hidden;
                    uxAboutBtn.IsEnabled = false;
                }
                _isAbout = value;
            }
        }





        private void UxCloseBtn_OnClick(object sender, RoutedEventArgs e) {
            if (IsMainWindow) {
                Application.Current.Shutdown();
            }
            else {
                Window.GetWindow(this)?.Hide();
            }
        }

        private void TitleBar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            Window.GetWindow(this)?.DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            var win = Window.GetWindow(this);
            if (win != null) {
                win.WindowState = WindowState.Minimized;
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
            //var win = Window.GetWindow(this);
            //if (win != null) {
            //    var about = new About {
            //        Owner = win,
            //        WindowStartupLocation = WindowStartupLocation.CenterOwner,
            //    };
            //    about.ShowDialog();

            //}
        }

        public bool isDark { get; set; } = true;

        public class ThemeChangeClickEventArgs : EventArgs {
            public string Tag { get; set; }
        }
        public event EventHandler<ThemeChangeClickEventArgs> Clicked;
        private void UxThemeSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            isDark = !isDark;

            ((App)Application.Current).ApplyBase(isDark);

            ((App) Application.Current).ApplyPrimary(isDark ? ((App) Application.Current).DarkPrimary : ((App) Application.Current).LightPrimary);
        }

        private void LnkGit_OnRequestNavigate(object sender, RequestNavigateEventArgs e){
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
