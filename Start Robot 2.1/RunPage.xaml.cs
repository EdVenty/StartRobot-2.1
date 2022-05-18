using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Start_Robot_2._1
{
    public sealed partial class RunPage : Page
    {
        private Logger Logger;
        public RunPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            Logger.OnLog -= Logger_OnLog;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Logger = (e.Parameter as MainPage).Logger;
            Logger.OnLog += Logger_OnLog;
            Logger.SetLogsChecked();
        }

        private void Logger_OnLog(object sender, LoggerOnLogEventArgs e)
        {
            Logger.SetLogsChecked();
            BindData();
        }

        private async void BindData()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ListTerminal.ItemsSource = null;
                ListTerminal.ItemsSource = Logger.Logs;
                if(ToggleAutoscroll.IsChecked ?? false)
                {
                    ListTerminal.ScrollIntoView(Logger.Logs.Last());
                }
            });
        }

        private void SendMessage()
        {
            Logger.Write(InputText.Text, Symbol.Camera);
            InputText.Text = String.Empty;
        }

        private void InputText_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key == Windows.System.VirtualKey.Enter)
            {
                SendMessage();
            }
        }

        private void ButtonSendInput_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            Logger.Clear();
        }
    }
}
