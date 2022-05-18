using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Gaming.Input;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Start_Robot_2._1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public event EventHandler<ControllersEventArgs> OnControllersChanged;
        public RobotsManager RobotsManager;
        public ObservableCollection<Robot> ObservableRobots { get; set; }
        public Logger Logger;
        public Frame PageContentFrame;
        public ObservableCollection<VirtualGamepad> Gamepads { get; private set; }
        private Robot SelectedRobot;

        public MainPage()
        {
            this.InitializeComponent();

            Gamepads = new ObservableCollection<VirtualGamepad>();
            PageContentFrame = PageContent;
            NavigationMain.SelectedItem = NavigationItemWelcome;
            ProgressRobotConnection.IsActive = false;
            NavigationItemGamepad.IsEnabled = false;

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(AppTitleBar);

            Logger = new Logger();
            Logger.OnUnreadMessage += async (sender, e) =>
            {
                await BadgeTerminal.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    BadgeTerminal.Value = e.UnreadMessagesCount;
                    BadgeTerminalOpacityUp.Begin();
                });
            };
            Logger.OnMessagesReaded += async (sender, e) =>
            {
                await BadgeTerminal.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    BadgeTerminalOpacityDown.Begin();
                });
            };

            RobotsManager = new RobotsManager(Logger);
            RobotsManager.OnRobotsUpdated += async (sender, e) =>
            {
                await ComboRobot.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ObservableRobots = new ObservableCollection<Robot>(RobotsManager.Robots);
                    ComboRobot.ItemsSource = null;
                    ComboRobot.ItemsSource = ObservableRobots;
                });
            };
            RobotsManager.OnRobotConnectionInProgress += async (sender, e) =>
            {
                await ProgressRobotConnection.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ProgressRobotConnection.IsActive = true;
                });
            };
            RobotsManager.OnRobotConnected += async (sender, e) =>
            {
                await ProgressRobotConnection.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ProgressRobotConnection.IsActive = false;
                });
            };
            RobotsManager.OnRobotStop += async (sender, e) =>
            {
                await ProgressRobotConnection.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ProgressRobotConnection.IsActive = false;
                    ComboRobot.IsEnabled = true;
                    NavigationItemGamepad.IsEnabled = false;
                    NavigationItemPrograms.IsEnabled = false;
                });
            };
            RobotsManager.OnRobotRun += async (sender, e) =>
            {
                await ComboRobot.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ComboRobot.IsEnabled = false;
                    NavigationItemGamepad.IsEnabled = true;
                    NavigationItemPrograms.IsEnabled = true;
                });
            };

            RawGameController.RawGameControllerAdded += RawGameController_RawGameControllerAdded;
            RawGameController.RawGameControllerRemoved += RawGameController_RawGameControllerRemoved;

            RobotsManager.LoadRobots();
            RobotsManager.UpdateRobots();
            FrameNavigationOptions navOptions = new FrameNavigationOptions();
            PageContent.NavigateToType(typeof(WelcomePage), this, navOptions);
        }

        private async void ShowInfo(string title, string message)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                BarMain.Title = title;
                BarMain.Message = message;
                BarMain.IsOpen = true;
                await Task.Delay(5000);
                BarMain.IsOpen = false;
            });
        }

        private void RawGameController_RawGameControllerRemoved(object sender, RawGameController e)
        {
            Gamepads.Remove(Gamepads.First(x => x.Controller == e));
            OnControllersChanged?.Invoke(this, new ControllersEventArgs
            {
                Gamepads = Gamepads
            });
            ShowInfo("Controller disconnected", $"Controller {e.DisplayName} has just disconnected. If you think that it is abnormal, try to reboot controller.");
        }

        private void RawGameController_RawGameControllerAdded(object sender, RawGameController e)
        {
            Gamepads.Add(new VirtualGamepad(e, e.DisplayName));
            OnControllersChanged?.Invoke(this, new ControllersEventArgs
            {
                Gamepads = Gamepads
            });
            ShowInfo("Controller connected", $"Controller {e.DisplayName} has just connected. Open \"Controllers\" page to configure it.");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ComboRobot.SelectedValue = SelectedRobot;
        }

        private void NavigationView_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            FrameNavigationOptions navOptions = new FrameNavigationOptions
            {
                TransitionInfoOverride = args.RecommendedNavigationTransitionInfo
            };
            Type pageType = typeof(WelcomePage);
            switch (args.InvokedItemContainer.Tag)
            {
                case "NavigationItemRun":
                    pageType = typeof(RunPage);
                    break;
                case "Settings":
                    pageType = typeof(OverviewPage);
                    break;
                case "NavigationItemWelcome":
                    pageType = typeof(WelcomePage);
                    break;
                case "NavigationItemGamepad":
                    pageType = typeof(GamepadPage);
                    break;
                case "NavigationItemPrograms":
                    pageType = typeof(ProgramsPage);
                    break;
                case "NavigationItemControllers":
                    pageType = typeof(ControllerPage);
                    break;
            }
            PageContent.NavigateToType(pageType, this, navOptions);
        }

        private void ToggleConnect_Checked(object sender, RoutedEventArgs e)
        {
            NavigationItemRun.IsEnabled = true;
            if(ComboRobot.SelectedItem == null)
            {
                ToggleConnect.IsChecked = false;
            }
            else
            {
                RobotsManager.RunRobot(ComboRobot.SelectedItem as Robot);
            }
        }

        private void ToggleConnect_Unchecked(object sender, RoutedEventArgs e)
        {
            if (RobotsManager.IsRobotRunning)
            {
                RobotsManager.StopRobot();
            }
        }

        private void ComboRobot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedRobot = ComboRobot.SelectedValue as Robot;
        }
    }
}
