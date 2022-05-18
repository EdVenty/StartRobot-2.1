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
using Windows.Storage.Pickers;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Start_Robot_2._1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProgramsPage : Page
    {
        public IEnumerable<ProgramFile> Files;
        public RobotsManager RobotManager;

        public ProgramsPage()
        {
            Files = new List<ProgramFile>
            {
                new ProgramFile { FileName = "Amogus" },
                new ProgramFile { FileName = "Sus" }
            };
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);
            RobotManager = (args.Parameter as MainPage).RobotsManager;
            RobotManager.RunningRobot.OnFilesChanged += async (sender, e) =>
            {
                if (!Dispatcher.HasThreadAccess)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Files = e.Files;
                        DataFiles.ItemsSource = Files;
                        DataFiles.UpdateLayout();
                    });
                }
            };

            DataFiles.ItemsSource = null;
            Files = RobotManager.RunningRobot.ProgramFiles;
            DataFiles.ItemsSource = Files;

            RobotManager.RunningRobot?.SendGetFiles();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs args)
        {
            base.OnNavigatedFrom(args);
            RobotManager.RunningRobot.OnFilesChanged -= async (sender, e) =>
            {
                if (!Dispatcher.HasThreadAccess)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Files = e.Files;
                        DataFiles.ItemsSource = Files;
                        DataFiles.UpdateLayout();
                    });
                }
            };
        }

        private async void ButtonUpload_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.FileTypeFilter.Add(".py");
            var file = await picker.PickSingleFileAsync();
            if(file != null)
            {
                RobotManager.RunningRobot?.SendAddFile(file.Name.Split('\\').Last(), Convert.ToBase64String((await Windows.Storage.FileIO.ReadBufferAsync(file)).ToArray()));
            }
        }

        private void DataFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ButtonRemove.IsEnabled = DataFiles.SelectedItem != null;
        }

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            if(DataFiles.SelectedItem != null)
            {
                RobotManager.RunningRobot?.SendRemoveFile((DataFiles.SelectedItem as ProgramFile).FileName);
            }
        }

        private void DataFiles_CellEditEnding(object sender, Microsoft.Toolkit.Uwp.UI.Controls.DataGridCellEditEndingEventArgs e)
        {
            
        }

        private void DataFiles_CellEditEnded(object sender, Microsoft.Toolkit.Uwp.UI.Controls.DataGridCellEditEndedEventArgs e)
        {
            var file = e.Row.DataContext as ProgramFile;
            if (file != null)
            {
                RobotManager.RunningRobot?.SendMarkFile(file.FileName, FilePropertyType.RunOnBoot, file.RunOnBoot);
                if (file.IsRunning)
                {
                    RobotManager.RunningRobot?.RunFile(file.FileName);
                }
                else
                {
                    RobotManager.RunningRobot?.StopFile(file.FileName);
                }
            }
        }
    }
}
