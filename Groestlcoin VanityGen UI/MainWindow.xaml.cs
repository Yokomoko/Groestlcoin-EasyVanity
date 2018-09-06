using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using Groestlcoin_VanityGen_UI.Code;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;

namespace Groestlcoin_VanityGen_UI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public ICommand DialogYesPrompt { get; }
        public ICommand DialogNoPrompt { get; }

        private readonly BackgroundWorker Worker = new BackgroundWorker();
        private readonly DispatcherTimer CpuTimer = new DispatcherTimer();
        public string RunSettings;
        public string CMDFile;
        public string Home = AppDomain.CurrentDomain.BaseDirectory;
        public string OutputText = string.Empty;

        private readonly PerformanceCounter CpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        public string SaveFileDirectory = "";

        public MainWindow() {
            InitializeComponent();
            Titlebar.IsMainWindow = true;
            Worker.DoWork += WorkerOnDoWork;
            Worker.WorkerSupportsCancellation = true;

            DialogYesPrompt = new CommandImplementation(OnYesDialogClick);
            DialogNoPrompt = new CommandImplementation(OnNoDialogClick);

            //Check if the PC is connected to the internet, and show a warning if it is.
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) {
                ShowStartDialog("This machine is currently connected to the internet! We would not advise creating or storing Groestlcoin wallets on a machine which is connected to the internet as it poses a risk\n\nAlthough we advise users to use the Generator offline we do allow the application to be run whilst connected to the internet.\n\nWould you like to continue?");
            }
            else {
                DialogHelper.ShowOKDialog(DH, "Online Status: Offline");
            }

            CpuTimer.Interval = new TimeSpan(0, 0, 0, 0, 1024);
            CpuTimer.IsEnabled = true;

            CpuTimer.Tick += (sender, args) => {
                var counterPercent = Convert.ToInt32(CpuCounter.NextValue());
                uxCPULbl.Text = counterPercent + "%";
            };

            Titlebar.Clicked += (sender, args) => ThemeSelector.ThemeSelector.SetCurrentThemeDictionary(this, new Uri((Titlebar.uxThemeSelector.SelectedItem as ComboBoxItem).Tag.ToString(), UriKind.Relative));
        }

        private void OnYesDialogClick(object obj){
            DH.IsOpen = false;
        }

        private void OnNoDialogClick(object obj) {
            Application.Current.Shutdown();
        }

        public void ShowStartDialog(string message) {
            var card = new MaterialDesignThemes.Wpf.Card { Padding = new System.Windows.Thickness(32), Margin = new System.Windows.Thickness(16), Width = 350 };
            var stackPanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            var textBlock = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap };

            var buttonStack = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            var yesButton = new Button { Content = "YES", Width = 100 };
            var noButton = new Button { Content = "NO", Width = 100 };

            Style btnstyle = Application.Current.FindResource("MaterialDesignFlatButton") as Style;
            if (btnstyle != null) {
                yesButton.Style = btnstyle;
                noButton.Style = btnstyle;
            }
            yesButton.Command = DialogYesPrompt;
            noButton.Command = DialogNoPrompt;

            stackPanel.Children.Add(textBlock);
            buttonStack.Children.Add(yesButton);
            buttonStack.Children.Add(noButton);
            stackPanel.Children.Add(buttonStack);
            card.Content = stackPanel;

            DH?.ShowDialog(card);
        }

        private void WorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs) {
            using (Process process = new Process()) {
                process.StartInfo.FileName = "cmd.exe";
                //process.StartInfo.Arguments = "/c";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false)) {
                    process.OutputDataReceived += (s, e) => {
                        if (e.Data == null) {
                            if (outputWaitHandle != null) outputWaitHandle.Set();
                        }
                        else {
                            SetText(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (s, e) => {
                        if (e.Data == null) {
                            if (errorWaitHandle != null) errorWaitHandle.Set();
                        }
                        else {
                            SetText(e.Data);
                        }
                    };

                    string phrase = string.Empty;

                    Dispatcher.Invoke(() => {
                        phrase = uxPhraseTxt.Text;
                        if (uxHwSelect.IsPressed == false) phrase = "F" + phrase;
                    });

                    process.Start();

                    process.StandardInput.WriteLine($"cd {Home}{@"\ProgFiles\"}");
                    process.StandardInput.WriteLine($"{CMDFile} {RunSettings} {phrase}");
                    process.StandardInput.Close();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (process.WaitForExit(3600000) &&
                        outputWaitHandle.WaitOne(3600000) &&
                        errorWaitHandle.WaitOne(3600000)) {
                    }
                    else {
                        KillProcesses();
                    }
                    Dispatcher.Invoke(() => {
                        uxStartBtn.IsEnabled = true;
                        uxStopBtn.IsEnabled = false;
                        uxPhraseTxt.IsEnabled = true;
                    });
                }
            }
        }

        delegate void SetTextCallback(string OutputLine);



        private void SetText(string outputLine) {
            string PublicKey;
            string PrivateKey;

            if (!string.IsNullOrEmpty(OutputText)) {
                OutputText += OutputText + Environment.NewLine;
            }

            var outputLineSplit = outputLine.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);

            if (outputLineSplit.Length > 4) {
                Dispatcher.Invoke(() => {
                    uxKps.Text = outputLineSplit[0];
                    //  uxTlKeysLbl.Text = outputLineSplit[1].Replace("total", "");
                    uxProbLbl.Text = outputLineSplit[3];
                });
            }

            if (outputLine.Contains("Address: ")) {
                PublicKey = outputLine.Replace("Address: ", "");
                Dispatcher.Invoke(() => uxPubKeyTxt.Text = PublicKey);
            }
            else if (outputLine.Contains("Privkey: ")) {
                PrivateKey = outputLine.Replace("Privkey: ", "");
                Dispatcher.Invoke(() => {
                    uxPrivKeyTxt.Text = PrivateKey;
                    uxSecretPrivKey.Password = PrivateKey;
                });
            }
            Dispatcher.Invoke(() => uxOutputTxt.Text += outputLine + Environment.NewLine);
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            try {
                this?.DragMove();
            }
            catch {
                //Ignored
            }

        }

        private void UxStopStartBtn_OnClick(object sender, RoutedEventArgs e) {
            if (!string.IsNullOrEmpty(uxPhraseTxt.Text)) {
                var illegalChar = string.Empty;
                var legalReplacement = string.Empty;

                var validChars = new[] { "W", "X", "Y", "Z" };
                var firstCharacter = uxPhraseTxt.Text[0];

                if (uxPhraseTxt.Text.Contains("0")) {
                    illegalChar = "0";
                    legalReplacement = "o";
                }
                else if (uxPhraseTxt.Text.Contains("O")) {
                    illegalChar = "O";
                    legalReplacement = "o";
                }
                else if (uxPhraseTxt.Text.Contains("I")) {
                    illegalChar = "I";
                    legalReplacement = "i";
                }
                else if (uxPhraseTxt.Text.Contains("l")) {
                    illegalChar = "l";
                    legalReplacement = "L";
                }
                //If the case sensitive option is checked, check to see if the first character is a valid uppercase character (WXYZ)

                else if (uxCaseOptChk.IsChecked == true || char.IsNumber(firstCharacter)) {
                    if (!char.IsLower(firstCharacter)) {
                        if (!App.StringExtensions.ContainsAny(firstCharacter.ToString(), validChars, StringComparison.Ordinal)) {
                            illegalChar = firstCharacter.ToString();
                            DialogHelper.ShowOKDialog(DH, $"Whoops! You have entered an illegal character \"{illegalChar}\". When using Case Sensitive characters, only lowercase letters, or uppercase 'W', 'X', 'Y' and 'Z' characters are valid for the first character.");
                        }
                    }
                }

                if (!string.IsNullOrEmpty(illegalChar)) {
                    DialogHelper.ShowOKDialog(DH, $"Whoops! You have entered an illegal Base58 character \"{illegalChar}\". You could always try \"{legalReplacement}\" instead.");
                    return;
                }
            }
            if (Worker.IsBusy) {
                KillProcesses();
            }
            else {
                if (uxHwSelect.IsPressed) {
                    CMDFile = "oclvanitygen.exe";
                }
                else if (!uxHwSelect.IsPressed) {
                    CMDFile = "vanitygen.exe";
                }
            }
            SetSettings();
        }

        private void SetSettings() {
            var sb = new StringBuilder();

            sb.Append("-v");

            if (uxCaseOptChk.IsChecked == false) sb.Append(" -i");
            if (uxKeepFindingOptChk.IsChecked == true) sb.Append(" -k");
            if (uxOutputKeysChk.IsChecked == true) {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.DefaultExt = ".txt";
                dlg.Filter = "Text documents (.txt)|*.txt";
                if (dlg.ShowDialog() == true) {
                    SaveFileDirectory = dlg.FileName;
                    sb.Append($" -o \"{SaveFileDirectory}\"");
                    //Set UI up for a successful run
                    uxViewFileBtn.IsEnabled = uxOutputKeysChk.IsChecked == true;
                }
                else {
                    return;
                }
            }
            RunSettings = sb.ToString();

            uxPhraseTxt.IsEnabled = false;
            uxPubKeyTxt.Text = string.Empty;
            uxPrivKeyTxt.Text = string.Empty;
            uxSecretPrivKey.Password = string.Empty;
            uxViewOutputBtn.IsEnabled = true;
            uxOutputTxt.Text = string.Empty;
            uxStartBtn.IsEnabled = false;
            uxStopBtn.IsEnabled = true;

            Worker.WorkerReportsProgress = true;
            Worker.RunWorkerAsync();
        }

        private void KillProcesses() {
            try {
                if (Worker.IsBusy) {
                    Worker.CancelAsync();
                }
                foreach (Process prog in Process.GetProcesses()) {
                    if (prog.ProcessName == CMDFile.Replace(".exe", "")) {
                        prog.Kill();
                    }
                }
            }
            catch (Exception e) {
                //Ignored

                //MessageBox.Show(e.Message);
            }
            uxPhraseTxt.IsEnabled = true;
        }

        private void UxStopBtn_OnClick(object sender, RoutedEventArgs e) {
            KillProcesses();
            uxPhraseTxt.IsEnabled = true;
            uxStartBtn.IsEnabled = true;
            uxStopBtn.IsEnabled = false;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e) {
            KillProcesses();
        }

        private void UxViewFileBtn_OnClick(object sender, RoutedEventArgs e) {
            if (!string.IsNullOrEmpty(SaveFileDirectory)) {
                Process.Start("explorer.exe", $"/select,\"{SaveFileDirectory}\"");
            }
        }

        private void UxPhraseTxt_OnPreviewTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = !TextAllowed(e.Text);
        }

        private void UxPhraseTxt_OnPasting(object sender, DataObjectPastingEventArgs e) {
            string s = (string)e.DataObject.GetData(typeof(string));
            if (!TextAllowed(s)) e.CancelCommand();
        }

        private Boolean TextAllowed(String s) {
            foreach (Char c in s.ToCharArray()) {
                if (Char.IsLetterOrDigit(c) || Char.IsControl(c)) continue;
                else return false;
            }
            return true;
        }


        private void UxPwdToggleBtn_OnClick(object sender, RoutedEventArgs e) {
            //uxSecretPrivKey.Password = uxPrivKeyTxt.Text;
            if (((ToggleButton)sender).IsChecked == true) {
                uxPrivKeyTxt.Visibility = Visibility.Visible;
                uxSecretPrivKey.Visibility = Visibility.Collapsed;
            }
            else {
                uxPrivKeyTxt.Visibility = Visibility.Collapsed;
                uxSecretPrivKey.Visibility = Visibility.Visible;
            }
        }

        private void UxSecretPrivKey_OnPreviewTextInput(object sender, TextCompositionEventArgs e) {
            //Make read-only
            e.Handled = true;
        }

        private void UxSecretPrivKey_OnPreviewKeyDown(object sender, KeyEventArgs e) {
            //Make read-only
            e.Handled = true;
        }

        private void UxCopyBtn_OnClick(object sender, RoutedEventArgs e) {
            var text = uxPrivKeyTxt.Text;

            Thread thread = new Thread(() => Clipboard.SetText(text));
            thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
            thread.Start();
            thread.Join();


            DialogHelper.ShowOKDialog(DH, "Private Key Copied to Clipboard");
        }
    }
}

