using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace Start_Robot_2._1
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class ControllerPage : Page
    {
        public ObservableCollection<VirtualGamepad> Gamepads { get; private set; }

        private CancellationTokenSource _cancellationTokenSource;

        public ControllerPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var page = e.Parameter as MainPage;
            page.OnControllersChanged += Page_OnControllersChanged;
            Gamepads = page.Gamepads;
            RefreshGamepads();
            _cancellationTokenSource = new CancellationTokenSource();
            new Task(async () =>
            {
                while (true)
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        break;
                    }
                    foreach(VirtualGamepad gamepad in Gamepads)
                    {
                        gamepad.UpdateReading();
                    }
                    
                    await Task.Delay(50);
                }
            }, _cancellationTokenSource.Token).Start();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            var page = e.Parameter as MainPage;
            page.OnControllersChanged -= Page_OnControllersChanged;
            _cancellationTokenSource.Cancel();
        }

        private async void RefreshGamepads()
        {
            
        }

        private void Page_OnControllersChanged(object sender, ControllersEventArgs e)
        {
            Gamepads = e.Gamepads;
            RefreshGamepads();
        }
    }
}
