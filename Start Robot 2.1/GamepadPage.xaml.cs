using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Gaming.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Start_Robot_2._1
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class GamepadPage : Page
	{
		private Gamepad _Gamepad = null;
		private RobotsManager rm;
		private Logger Logger;
		private Frame PageContentFrame;
		private CancellationTokenSource CancelGamepadListening;
		public GamepadPage()
		{
			this.InitializeComponent();
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;
		}

        private void CoreWindow_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
			if (ToggleKeyboard.IsChecked ?? false)
			{
				rm.RunningRobot?.SendKey(args.VirtualKey.ToString(), KeyState.Unpressed);
			}
		}

        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
			if (ToggleKeyboard.IsChecked ?? false)
			{
				rm.RunningRobot?.SendKey(args.VirtualKey.ToString(), KeyState.Pressed);
			}
		}

        private bool IsGamepadButtonPressed(GamepadButtons state, GamepadButtons button)
		{
			return (state & button) == button;
		}

        private void RunningRobot_GamepadVibrationChanged(object sender, VibrationEventArgs e)
        {
			if (_Gamepad != null) {
				_Gamepad.Vibration = new Windows.Gaming.Input.GamepadVibration
				{
					LeftMotor = e.Vibration.LeftMotor,
					LeftTrigger = e.Vibration.LeftTrigger,
					RightMotor = e.Vibration.RightMotor,
					RightTrigger = e.Vibration.RightTrigger
				};
			}
        }

        private async void RunningRobot_VideoMessageReceived(object sender, VideoMessageEventArgs e)
        {
			try
			{
				await ImageCamera.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
				{
					var bitmap = await ImageConverter.GetBitmapAsync(e.Frame);
					ImageCamera.Source = bitmap;
				});
			}
			catch(Exception ex)
            {
				Logger.Write($"Error: {ex.Message}");
            }
		}

        private async void Gamepad_GamepadRemoved(object sender,
												  Gamepad e)
		{
			CancelGamepadListening?.Cancel();
			_Gamepad = null;

			await Dispatcher.RunAsync(
				CoreDispatcherPriority.Normal, () =>
				{
					tbConnected.Text = "Controller removed";
				});
		}

		private async void Gamepad_GamepadAdded(object sender,
												Gamepad e)
		{
			_Gamepad = e;
			CancelGamepadListening = RunGamepadListener();
			await StartGamepad();
		}

		private async Task StartGamepad()
        {
			var batteryLevel = _Gamepad.TryGetBatteryReport();
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				tbConnected.Text = "Controller added";
				TextBattery.Text = $"Battery: {batteryLevel.RemainingCapacityInMilliwattHours} of {batteryLevel.FullChargeCapacityInMilliwattHours} mWh";
			});
			_Gamepad.Vibration = new Windows.Gaming.Input.GamepadVibration
			{
				RightMotor = 1,
				LeftTrigger = 1
			};
			await Task.Delay(1000);
			_Gamepad.Vibration = new Windows.Gaming.Input.GamepadVibration();
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			rm = (e.Parameter as MainPage).RobotsManager;
			Logger = (e.Parameter as MainPage).Logger;
			PageContentFrame = (e.Parameter as MainPage).PageContentFrame;
            rm.OnRobotStop += Rm_OnRobotStop;
			rm.RunningRobot.VideoMessageReceived += RunningRobot_VideoMessageReceived;
			rm.RunningRobot.GamepadVibrationChanged += RunningRobot_GamepadVibrationChanged;
			Gamepad.GamepadAdded += Gamepad_GamepadAdded;
			Gamepad.GamepadRemoved += Gamepad_GamepadRemoved;
			/*if (Gamepad.Gamepads.Count > 0)
			{
				_Gamepad = Gamepad.Gamepads[0];
				CancelGamepadListening = RunGamepadListener();
				StartGamepad().Start();
			}*/
		}

		private CancellationTokenSource RunGamepadListener()
        {
			var tokenSource = new CancellationTokenSource();
			new Task(async () =>
			{
				while (true)
				{
					if (tokenSource.IsCancellationRequested)
					{
						break;
					}
					await Dispatcher.RunAsync(
						CoreDispatcherPriority.Normal, () =>
						{
							if (_Gamepad == null)
							{
								return;
							}
							// Get the current state
							if (_Gamepad != null)
							{
								var reading = _Gamepad.GetCurrentReading();
								if (rm.RunningRobot != null)
								{
									rm.RunningRobot.SendGamepadState(reading);
								}
							}
						}
					);
					await Task.Delay(TimeSpan.FromMilliseconds(100));
				}
			}, tokenSource.Token).Start();
			return tokenSource;
		}

        private async void Rm_OnRobotStop(object sender, EventArgs e)
        {
			var navOptions = new FrameNavigationOptions();
			await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
			{
				PageContentFrame?.NavigateToType(typeof(WelcomePage), null, navOptions);
			});
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
			rm.OnRobotStop -= Rm_OnRobotStop;
			rm.RunningRobot.VideoMessageReceived -= RunningRobot_VideoMessageReceived;
			rm.RunningRobot.GamepadVibrationChanged -= RunningRobot_GamepadVibrationChanged;
			CancelGamepadListening?.Cancel();
			Gamepad.GamepadAdded -= Gamepad_GamepadAdded;
			Gamepad.GamepadRemoved -= Gamepad_GamepadRemoved;
		}

        private void ToggleKeyboard_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void Grid_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (ToggleKeyboard.IsChecked ?? false)
            {
				rm.RunningRobot?.SendKey(e.Key.ToString(), KeyState.Pressed);
            }
        }

        private void Grid_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
			if (ToggleKeyboard.IsChecked ?? false)
			{
				rm.RunningRobot?.SendKey(e.Key.ToString(), KeyState.Unpressed);
			}
		}
    }
}
