using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Gaming.Input;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Start_Robot_2._1
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class OverviewPage : Page
	{
		public RobotsManager _rm;
		private Robot _editingRobot;

		public OverviewPage()
		{
			this.InitializeComponent();
		}

		protected override void OnNavigatedTo(NavigationEventArgs args)
		{
			base.OnNavigatedTo(args);
			_rm = (args.Parameter as MainPage).RobotsManager;
			_rm.OnRobotsLoaded += (sender, e) =>
			{
				_editingRobot = null;
			};
			_rm.OnRobotsUpdated += (sender, e) =>
			{
				_editingRobot = null;
				ListRobots.ItemsSource = null;
				ListRobots.ItemsSource = _rm.Robots;
			};
			_rm.OnRobotRun += (sender, e) => SetEnabled(false);
			_rm.OnRobotStop += (sender, e) => SetEnabled(true);
			_rm.LoadRobots();
			_rm.UpdateRobots();
		}

		private void SetEnabled(bool enabled)
        {
			ListRobots.IsEnabled = enabled;
			InputRobotName.IsEnabled = enabled;
			InputRobotIp.IsEnabled = enabled;
			InputRobotLogin.IsEnabled = enabled;
			InputRobotPassword.IsEnabled = enabled;
			ButtonCreateRobot.IsEnabled = enabled;
		}

		private void ButtonCreateRobot_Click(object sender, RoutedEventArgs e)
		{
			if (_editingRobot != null)
			{
				_rm.RemoveRobot(_editingRobot);
			}
			_rm.AddRobot(new Robot
			{
				Name = InputRobotName.Text,
				Ip = InputRobotIp.Text,
				Login = InputRobotLogin.Text,
				Password = InputRobotPassword.Text
			});
			_rm.SaveRobots();
			InputRobotName.Text = String.Empty;
			InputRobotIp.Text = String.Empty;
			InputRobotLogin.Text = String.Empty;
			InputRobotPassword.Text = String.Empty;
			_rm.UpdateRobots();
		}

		private async void ButtonRobotRemove_Click(object sender, RoutedEventArgs e)
		{
			MessageDialog removeDialog = new MessageDialog($"Do you want to remove the robot?");
			var commandYes = new UICommand("Yes", null);
			removeDialog.Commands.Add(commandYes);
			removeDialog.Commands.Add(new UICommand("Cancel", null));
			removeDialog.DefaultCommandIndex = 0;
			removeDialog.CancelCommandIndex = 1;
			var removeResult = await removeDialog.ShowAsync();
			if(removeResult == commandYes)
			{
				var foundRobot = _rm.Robots.Find(x => x == (sender as Button).DataContext);
				if (foundRobot != null)
				{
					_rm.RemoveRobot(foundRobot);
					_rm.UpdateRobots();
					_rm.SaveRobots();
				}
			}
		}

		private void ButtonRobotEdit_Click(object sender, RoutedEventArgs e)
		{
			_editingRobot = (sender as Button).DataContext as Robot;
			InputRobotName.Text = _editingRobot.Name;
			InputRobotIp.Text = _editingRobot.Ip;
			InputRobotLogin.Text = _editingRobot.Login;
			InputRobotPassword.Text = _editingRobot.Password;
		}
	}
}
