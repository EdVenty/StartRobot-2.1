using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatsonWebsocket;
using System.Text.Json;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using Windows.Gaming.Input;
using System.Diagnostics;
using Windows.Networking.Sockets; 

namespace Start_Robot_2._1
{
	public enum KeyState
	{
		Pressed, Unpressed
	}
	public class GamepadControls
    {
		public bool ButtonNone { get; set; }
		public bool ButtonMenu { get; set; }
		public bool ButtonA { get; set; }
		public bool ButtonB { get; set; }
		public bool ButtonX { get; set; }
		public bool ButtonY { get; set; }
		public bool ButtonDPadUp { get; set; }
		public bool ButtonDPadDown { get; set; }
		public bool ButtonDPadLeft { get; set; }
		public bool ButtonDPadRight { get; set; }
		public bool ButtonLeftShoulder { get; set; }
		public bool ButtonRightShoulder { get; set; }
		public bool ButtonLeftThumbstick { get; set; }
		public bool ButtonRightThumbstick { get; set; }
		public bool ButtonPaddle1 { get; set; }
		public bool ButtonPaddle2 { get; set; }
		public bool ButtonPaddle3 { get; set; }
		public bool ButtonPaddle4 { get; set; }
		public double LeftThumbstickX { get; set; }
		public double LeftThumbstickY { get; set; }
		public double RightThumbstickX { get; set; }
		public double RightThumbstickY { get; set; }
		public double LeftTrigger { get; set; }
		public double RightTrigger { get; set; }
	}
	public class GamepadVibration
    {
		public double LeftTrigger { get; set; }
		public double RightTrigger { get; set; }
		public double LeftMotor { get; set; }
		public double RightMotor { get; set; }
    }
	public class ControlMessage
	{
		public string Key { get; set; }
		public KeyState KeyState { get; set; }
		public GamepadControls Gamepad { get; set; }
		public GamepadVibration Vibration { get; set; }
	}
	public class TerminalMessage
	{
		public string StartFile { get; set; } = null;
		public bool GetFile { get; set; } = false;
		public bool ProgramActive { get; set; } = false;
		public string Message { get; set; } = null;
		public bool StopProgram { get; set; } = false;
	}
	public enum FilePropertyType
    {
		Program, RunOnBoot, User
    }
	public enum FileMessageType
    {
		AddFile, RemoveFile, Mark, GetFiles, RunFile, StopFile
    }
	public interface IFileToRobotMessage
    {
		FileMessageType Event { get; }
    }
	public class AddFileMessage : IFileToRobotMessage
    {
		public FileMessageType Event { get; } = FileMessageType.AddFile;
		public string FileContent { get; set; }
		public string FileName { get; set; }
	}
	public class RemoveFileMessage : IFileToRobotMessage
    {
		public FileMessageType Event { get; } = FileMessageType.RemoveFile;
		public string FileName { get; set; }
    }
	public class MarkFileMessage : IFileToRobotMessage
    {
		public FileMessageType Event { get; } = FileMessageType.Mark;
		public string FileName { get; set; }
		public FilePropertyType FilePropertyType { get; set; }
		public object FilePropertyValue { get; set; }
    }
	public class GetFilesMessage : IFileToRobotMessage
	{
		public FileMessageType Event { get; } = FileMessageType.GetFiles;
	}
	public class RunFileMessage : IFileToRobotMessage
    {
		public FileMessageType Event { get; } = FileMessageType.RunFile;
		public string FileName { get; set; }
    }
	public class StopFileMessage : IFileToRobotMessage
	{
		public FileMessageType Event { get; } = FileMessageType.StopFile;
		public string FileName { get; set; }
	}
	public class FilesMessage
    {
		public string Log { get; set; }
		public IEnumerable<ProgramFile> Files { get; set; }
    }
	public class InfoMessage
	{
		public bool Request { get; set; } = true;
		public string Server { get; set; }
		public string Id { get; set; }
		public string Ip { get; set; }
		public string Firmware { get; set; }
		public string Os { get; set; }
		public string Hostname { get; set; }
	}
	public class RobotActiveStateChangedEventArgs : EventArgs
	{
		public int ActiveChannels;
		public int ChannelsAmount;
		public bool AllChannelsActive;
		public Dictionary<string, bool> ChannelsState;
	}
	public class ProgramChangedEventArgs : EventArgs
	{
		public bool ProgramActive;
		public string CurrentProgram;
	}
	public class TerminalMessageEventArgs : EventArgs
	{
		public string Message;
	}
	public class VideoMessageEventArgs : EventArgs
	{
		public dynamic Frame;
	}
	public class InfoMessageEventArgs : EventArgs
	{
		public InfoMessage Info;
	}
	public class VibrationEventArgs : EventArgs
    {
		public GamepadVibration Vibration;
    }
	public class FilesMessageEventArgs : EventArgs
    {
		public string Log;
		public IEnumerable<ProgramFile> Files;
    }
	public class Robot
	{
		[XmlIgnore] private WatsonWsClient ControlSocket;
		[XmlIgnore] private WatsonWsClient FileSocket;
		[XmlIgnore] private WatsonWsClient TerminalSocket;
		[XmlIgnore] private WatsonWsClient VideoSocket;
		[XmlIgnore] private WatsonWsClient InfoSocket;
		[XmlIgnore] public Logger Logger { private get; set; }
		[XmlIgnore] public InfoMessage Information;
		public event EventHandler<RobotActiveStateChangedEventArgs> RobotActiveStateChanged;
		public event EventHandler<ProgramChangedEventArgs> ProgramChanged;
		public event EventHandler<TerminalMessageEventArgs> TerminalMessageReceived;
		public event EventHandler<VideoMessageEventArgs> VideoMessageReceived;
		public event EventHandler<InfoMessageEventArgs> InfoMessageReceived;
		public event EventHandler<FilesMessageEventArgs> OnFilesChanged;
		public event EventHandler<VibrationEventArgs> GamepadVibrationChanged;
		public string Name { get; set; }
		public string Ip { get; set; }
		public string Login { get; set; }
		public string Password { get; set; }
		[XmlIgnore] public IEnumerable<ProgramFile> ProgramFiles { get; private set; }

		public void Initialize(Logger logger)
		{
			Logger = logger;
			ControlSocket = new WatsonWsClient(Ip, 8020, false);
			ControlSocket.ServerConnected += (sender, e) =>
			{
				//Logger.Write("Control socket connected.");
				InvokeChannelsActiveEvent();
			};
			ControlSocket.MessageReceived += (sender, e) =>
			{
				ControlMessage data = JsonSerializer.Deserialize<ControlMessage>(Encoding.UTF8.GetString(e.Data));
				if (data.Vibration != null)
				{
					GamepadVibrationChanged?.Invoke(this, new VibrationEventArgs
					{
						Vibration = data.Vibration
					});
                }
			};
			ControlSocket.ServerDisconnected += (sender, e) =>
			{
				Logger.Write($"Control socket disconnected. Reason: {e}");
				InvokeChannelsActiveEvent();
			};
			FileSocket = new WatsonWsClient(Ip, 8030, false);
			FileSocket.ServerConnected += (sender, e) =>
			{
				//Logger.Write("File socket connected.");
				SendGetFiles();
				InvokeChannelsActiveEvent();
			};
			FileSocket.ServerDisconnected += (sender, e) =>
			{
				Logger.Write($"File socket disconnected. Reason: {e}");
				InvokeChannelsActiveEvent();
			};
			FileSocket.MessageReceived += (sender, e) =>
			{
				var decoded = JsonSerializer.Deserialize<FilesMessage>(e.Data);
				if (decoded.Files != null)
				{
					OnFilesChanged?.Invoke(this, new FilesMessageEventArgs
					{
						Files = decoded.Files
					});
				}
				if(decoded.Log != null)
                {
					Logger.Write(decoded.Log.Trim(), Windows.UI.Xaml.Controls.Symbol.Message);
                }
			};
			TerminalSocket = new WatsonWsClient(Ip, 8040, false);
			TerminalSocket.ServerConnected += (sender, e) =>
			{
				//Logger.Write("Terminal socket connected.");
				SendGetFileRequest();
				InvokeChannelsActiveEvent();
			};
			TerminalSocket.ServerDisconnected += (sender, e) =>
			{
				Logger.Write($"Terminal socket disconnected. Reason: {e}");
				InvokeChannelsActiveEvent();
			};
			VideoSocket = new WatsonWsClient(Ip, 8010, false);
			VideoSocket.MessageReceived += (sender, e) =>
			{
				VideoMessageReceived?.Invoke(this, new VideoMessageEventArgs
				{
					Frame = e.Data
				});
			};
			VideoSocket.ServerConnected += (sender, e) =>
			{
				//Logger.Write("Video socket connected.");
				InvokeChannelsActiveEvent();
			};
			VideoSocket.ServerDisconnected += (sender, e) =>
			{
				Logger.Write($"Video socket disconnected. Reason: {e}");
				InvokeChannelsActiveEvent();
			};
			InfoSocket = new WatsonWsClient(Ip, 8050, false);
			InfoSocket.ServerConnected += (sender, e) =>
			{
				//Logger.Write("Info socket connected.");
				InvokeChannelsActiveEvent();
				SendGetInfoRequest();
			};
			InfoSocket.ServerDisconnected += (sender, e) =>
			{
				Logger.Write($"Info socket disconnected. Reason: {e}");
				InvokeChannelsActiveEvent();
			};
			Trace.WriteLine("All sockets initializated.");
			BindInformationUpdate();
		}
		public override string ToString()
		{
			return Name;
		}
		private void BindInformationUpdate()
		{
			InfoMessageReceived += (sender, e) =>
			{
				Information = e.Info;
			};
		}
		public void Start()
		{
			new Task(async () =>
			{
				var startTime = DateTime.Now;
				Logger.Write("Connecting to the robot.");
				await ControlSocket.StartAsync();
				await FileSocket.StartAsync();
				await TerminalSocket.StartAsync();
				await VideoSocket.StartAsync();
				await InfoSocket.StartAsync();
				Debug.WriteLine($"Start execution took {DateTime.Now - startTime}");
			}).Start();
		}
		public void Stop()
		{
			//VideoSocket.Dispose();
			VideoSocket.Stop();
			//ControlSocket.Dispose();
			ControlSocket.Stop();
			//FileSocket.Dispose();
			FileSocket.Stop();
			//TerminalSocket.Dispose();
			TerminalSocket.Stop();
			//InfoSocket.Dispose();
			InfoSocket.Stop();
			InvokeChannelsActiveEvent();
			Logger.Write("Disconnected from the robot.");
		}
		public bool Started()
		{
			return ControlSocket != null && ControlSocket.Connected;
		}
		public void SendGetFiles()
        {
			FileSocket.SendAsync(Serialize(new GetFilesMessage()));
        }
		public void SendKey(string key, KeyState state = KeyState.Pressed, GamepadControls gamepad = null)
		{
			ControlSocket.SendAsync(Serialize(new ControlMessage
			{
				Key = key,
				KeyState = state,
				Gamepad = gamepad
			}));
		}
		private bool IsGamepadButtonPressed(GamepadButtons state, GamepadButtons button)
        {
			return (state & button) == button;
        }
		public void SendGamepadState(GamepadReading state)
        {
			var gamepadControls = new GamepadControls
			{
				ButtonA = IsGamepadButtonPressed(state.Buttons, GamepadButtons.A),
				ButtonB = IsGamepadButtonPressed(state.Buttons, GamepadButtons.B),
				ButtonX = IsGamepadButtonPressed(state.Buttons, GamepadButtons.X),
				ButtonY = IsGamepadButtonPressed(state.Buttons, GamepadButtons.Y),
				ButtonDPadDown = IsGamepadButtonPressed(state.Buttons, GamepadButtons.DPadDown),
				ButtonDPadLeft = IsGamepadButtonPressed(state.Buttons, GamepadButtons.DPadLeft),
				ButtonDPadRight = IsGamepadButtonPressed(state.Buttons, GamepadButtons.DPadRight),
				ButtonDPadUp = IsGamepadButtonPressed(state.Buttons, GamepadButtons.DPadRight),
				ButtonLeftShoulder = IsGamepadButtonPressed(state.Buttons, GamepadButtons.LeftShoulder),
				ButtonLeftThumbstick = IsGamepadButtonPressed(state.Buttons, GamepadButtons.LeftThumbstick),
				ButtonRightShoulder = IsGamepadButtonPressed(state.Buttons, GamepadButtons.RightShoulder),
				ButtonRightThumbstick = IsGamepadButtonPressed(state.Buttons, GamepadButtons.RightThumbstick),
				ButtonMenu = IsGamepadButtonPressed(state.Buttons, GamepadButtons.Menu),
				ButtonNone = IsGamepadButtonPressed(state.Buttons, GamepadButtons.None),
				ButtonPaddle1 = IsGamepadButtonPressed(state.Buttons, GamepadButtons.Paddle1),
				ButtonPaddle2 = IsGamepadButtonPressed(state.Buttons, GamepadButtons.Paddle2),
				ButtonPaddle3 = IsGamepadButtonPressed(state.Buttons, GamepadButtons.Paddle3),
				ButtonPaddle4 = IsGamepadButtonPressed(state.Buttons, GamepadButtons.Paddle4),
				
				LeftThumbstickX = state.LeftThumbstickX,
				LeftThumbstickY = state.LeftThumbstickY,
				RightThumbstickX = state.RightThumbstickX,
				RightThumbstickY = state.RightThumbstickY,
				LeftTrigger = state.LeftTrigger,
				RightTrigger = state.RightTrigger
			};
			SendKey("Gamepad", gamepad: gamepadControls);
        }
		public string Serialize(object obj)
		{
			return JsonSerializer.Serialize(obj);
		}
		void InvokeChannelsActiveEvent()
		{
			var channels = new Dictionary<string, bool>
			{
				{ "control", ControlSocket.Connected },
				{ "file", FileSocket.Connected },
				{ "terminal", TerminalSocket.Connected }
			};
			var amount = channels.Count;
			var active = channels.Where(x => x.Value).Count();
			RobotActiveStateChanged?.Invoke(this, new RobotActiveStateChangedEventArgs
			{
				ChannelsState = channels,
				ChannelsAmount = amount,
				ActiveChannels = active,
				AllChannelsActive = amount == active
			});
		}
		public void SendAddFile(string filename, string base64content)
        {
			FileSocket.SendAsync(Serialize(new AddFileMessage
			{
				FileName = filename, 
				FileContent = base64content
			}));
        }
		public void SendMarkFile(string filename, FilePropertyType propertyType, object propertyValue)
        {
			FileSocket.SendAsync(Serialize(new MarkFileMessage
			{
				FileName = filename,
				FilePropertyType = propertyType,
				FilePropertyValue = propertyValue
			}));
        }
		public void SendRemoveFile(string filename)
        {
			FileSocket.SendAsync(Serialize(new RemoveFileMessage
			{
				FileName = filename
			}));
        }
		public void SendGetFileRequest()
		{
			//Logger.Write("Sending file request.");
			TerminalSocket.SendAsync(Serialize(new TerminalMessage
			{
				GetFile = true
			}));
		}
		public void SendGetInfoRequest()
		{
			//Logger.Write("Sending info request.");
			InfoSocket.SendAsync(Serialize(new InfoMessage()));
		}
		public void RunFile(string filename)
		{
			FileSocket.SendAsync(Serialize(new RunFileMessage
			{
				FileName = filename
			}));
		}
		public void StopFile(string filename)
		{
			FileSocket.SendAsync(Serialize(new StopFileMessage
			{
				FileName = filename
			}));
		}
		public void SendMessage(string message)
		{
			TerminalSocket.SendAsync(Serialize(new TerminalMessage
			{
				Message = message + '\n'
			}));
		}
	}
}	
