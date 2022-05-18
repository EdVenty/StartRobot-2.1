using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Gaming.Input;

namespace Start_Robot_2._1
{
    public class VirtualGamepad
    {
        public event EventHandler<OnGamepadEventArgs> OnGamepadEvent;

        public readonly RawGameController Controller;
        public ObservableCollection<VirtualChannel> Channels;
        public string Name;
        public double[] AxisArray;
        public ObservableCollection<VirtualAxisValue> AxisValues;
        public bool IsReading { get; private set; }

        private CancellationTokenSource _cancellationTokenSource;
        public int AxisCount { 
            get
            {
                return Controller.AxisCount;
            } 
        }
        public VirtualGamepad(RawGameController controller, string name)
        {
            Controller = controller;
            Name = name;
            AxisValues = new ObservableCollection<VirtualAxisValue>();
            UpdateReading();
        }
        public void UpdateReading()
        {
            double[] axisArray = new double[Controller.AxisCount];
            Controller.GetCurrentReading(null, null, axisArray);
            if (Channels != null)
            {
                foreach (var channel in Channels)
                {
                    channel.UpdateFromArray(axisArray);
                }
            }
            AxisValues.Clear();
            if (axisArray != null)
            {
                for (int i = 0; i < axisArray.Length; i++)
                {
                    AxisValues.Add(new VirtualAxisValue
                    {
                        Name = $"Axis #{i}",
                        Value = axisArray[i]
                    });
                }
                AxisArray = axisArray;
            }
        }
        public void StartReading(int delay = 10)
        {
            if(_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
            _cancellationTokenSource = new CancellationTokenSource();
            new Task(() =>
            {
                while (true)
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        break;
                    }
                    UpdateReading();
                    OnGamepadEvent?.Invoke(this, new OnGamepadEventArgs
                    {
                        Gamepad = this
                    });
                    Task.Delay(delay);
                }
            }, _cancellationTokenSource.Token).Start();
            IsReading = true;
        }
        public void StopReading()
        {
            if(_cancellationTokenSource == null)
            {
                throw new InvalidOperationException("Reading was not started yet.");
            }
            IsReading = false;
            _cancellationTokenSource.Cancel();
        }
    }
}
