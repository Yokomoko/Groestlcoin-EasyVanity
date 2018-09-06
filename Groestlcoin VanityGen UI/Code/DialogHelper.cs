using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace Groestlcoin_VanityGen_UI.Code {
    public class DialogHelper {

        public enum DialogButtons {
            YesNo,
            OK
        }

        public static void ShowOKDialog(DialogHost host, string message) {
            var card = new MaterialDesignThemes.Wpf.Card { Padding = new System.Windows.Thickness(32), Margin = new System.Windows.Thickness(16), Width = 350 };
            var stackPanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            var textBlock = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap };
            var okButton = new Button { Content = "OK", Width = 100 };

            Style btnstyle = Application.Current.FindResource("MaterialDesignFlatButton") as Style;
            if (btnstyle != null) {
                okButton.Style = btnstyle;
            }
            okButton.Click += (sender, args) => host.IsOpen = false;

            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(okButton);
            card.Content = stackPanel;

            host?.ShowDialog(card);
        }

        //public static void ShowYesNoDialog(DialogHost host, string message) {
        //    var card = new MaterialDesignThemes.Wpf.Card { Padding = new System.Windows.Thickness(32), Margin = new System.Windows.Thickness(16), Width = 350 };
        //    var stackPanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
        //    var textBlock = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap };

        //    var buttonStack = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
        //    var yesButton = new Button { Content = "YES", Width = 100 };
        //    var noButton = new Button { Content = "NO", Width = 100 };

        //    Style btnstyle = Application.Current.FindResource("MaterialDesignFlatButton") as Style;
        //    if (btnstyle != null) {
        //        yesButton.Style = btnstyle;
        //        noButton.Style = btnstyle;
        //    }
        //    stackPanel.Children.Add(textBlock);
        //    buttonStack.Children.Add(yesButton);
        //    buttonStack.Children.Add(noButton);
        //    stackPanel.Children.Add(buttonStack);
        //    card.Content = stackPanel;

        //    host?.ShowDialog(card);
        //}
    }
}
