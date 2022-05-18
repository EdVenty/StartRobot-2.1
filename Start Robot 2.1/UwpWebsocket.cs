using System.Collections;
using System.Collections.Generic;
using Windows.Networking.Sockets;
using System.Threading.Tasks;
using Windows.Web;
using System.Threading;
using System;   //	System.Uri
using System.IO;

using Windows.System.Threading;

using System.Runtime.InteropServices.WindowsRuntime;    //	AsBuffer
using Windows.Storage.Streams;          // DataWriter
using System.Diagnostics;
//	https://github.com/Microsoft/Windows-universal-samples/blob/master/Samples/WebSocket/cs/Scenario2_Binary.xaml.cs


namespace Start_Robot_2._1
{
	public class ErrorEventArgs : EventArgs
	{
		public Exception Exception;
		public string Message;
	}
	public class MessageEventArgs : EventArgs
	{
		public byte[] Data;
		public SocketMessageType Type;
	}
	public class CloseEventArgs : EventArgs
	{
		public ushort Code;
		public string Reason;
	}
	public class OpenEventArgs : EventArgs
	{

	}
	class WebSocket
	{
		public readonly SocketMessageType MessageType;
		public readonly string Url;
		public bool IsAlive { get; private set; }
		public Queue<string> OutputMessagesQueue { get; private set; }
		public event EventHandler<MessageEventArgs> OnMessage;
		public event EventHandler<CloseEventArgs> OnClose;
		public event EventHandler<OpenEventArgs> OnOpen;
		public event EventHandler<ErrorEventArgs> OnError;

		private Windows.Networking.Sockets.StreamWebSocket MessageWebSocket;

		public WebSocket(string url, SocketMessageType messageType = SocketMessageType.Utf8)
		{
			Url = url;
			MessageType = messageType;
			OutputMessagesQueue = new Queue<string>();
			MessageWebSocket = new Windows.Networking.Sockets.StreamWebSocket();
			MessageWebSocket.Control.NoDelay = true;
			// MessageWebSocket.Control.MessageType = MessageType;
			//MessageWebSocket.MessageReceived += MessageWebSocket_MessageReceived;
			MessageWebSocket.Closed += MessageWebSocket_Closed;
		}

		public void ConnectAsync()
		{
			try
			{
				Task connectTask = MessageWebSocket.ConnectAsync(new Uri(Url)).AsTask();

				connectTask.ContinueWith(_ =>
				{
					IsAlive = true;
					Task.Run(() => ReceiveMessageUsingStreamWebSocket());
					// Loop().Start();
					//OnOpen?.BeginInvoke(this, new OpenEventArgs(), (c) => { }, null);
				});
			}
			catch(Exception ex)
			{
				OnError?.BeginInvoke(this, new ErrorEventArgs
				{
					Exception = ex,
					Message = ex.Message
				}, (c) => { }, null);
			}
		}

		public void Close()
		{
			MessageWebSocket.Close(0, "Yes");
		}

		private async Task Loop()
		{
			while (true)
			{
				foreach(var msg in OutputMessagesQueue)
				{
					if (!IsAlive)
					{
						break;
					}
					try
					{
						using (var dataWriter = new DataWriter(MessageWebSocket.OutputStream))
						{
							dataWriter.WriteString(msg);
							await dataWriter.StoreAsync();
							dataWriter.DetachStream();
						}
					}
					catch (Exception ex)
					{
						Windows.Web.WebErrorStatus webErrorStatus = Windows.Networking.Sockets.WebSocketError.GetStatus(ex.GetBaseException().HResult);
						Trace.WriteLine($"Palundra! Error occured in message sending: {ex.Message}");
					}
				}
				if (!IsAlive)
				{
					break;
				}
				await Task.Delay(40);
			}
		}

		public void SendAsync(string message, Action<object> zatichka)
		{
			// OutputMessagesQueue.Enqueue(message);
			/*new Task(async () =>
			{
				using (var dataWriter = new DataWriter(MessageWebSocket.OutputStream))
				{
					dataWriter.WriteString(message);
					await dataWriter.StoreAsync();
					dataWriter.DetachStream();
				}
			}).Start();*/
			new Task(async () =>
			{
				using (var dataWriter = new DataWriter(MessageWebSocket.OutputStream))
				{
					dataWriter.WriteString(message);
					await dataWriter.StoreAsync();
					dataWriter.DetachStream();
				}
			}).Start();
		}

		private async void ReceiveMessageUsingStreamWebSocket()
		{
			while (true)
			{
				Stream readStream = MessageWebSocket.InputStream.AsStreamForRead();
				int bytesReceived = 0;
				try
				{
					await Task.Delay(2000);
					byte[] readBuffer = new byte[1000];

					while (true)
					{
						int read = await readStream.ReadAsync(readBuffer, 0, readBuffer.Length);
                        //bytesReceived += read;
                        if (OnMessage != null)
                        {
							OnMessage(this, new MessageEventArgs
							{
								Data = readBuffer
							});
						};
						//DataReceivedField.Text = bytesReceived.ToString();
					}
				}
				catch (Exception ex)
				{
					WebErrorStatus status = WebSocketError.GetStatus(ex.GetBaseException().HResult);

					switch (status)
					{
						case WebErrorStatus.OperationCanceled:
							Debug.WriteLine("WebErrorStatus.OperationCanceled");
							break;

						default:
							Debug.WriteLine("Error: " + status);
							Debug.WriteLine(ex.Message);
							break;
					}
				}
			}
		}

		private void MessageWebSocket_Closed(IWebSocket sender, WebSocketClosedEventArgs args)
		{
			IsAlive = false;
			OnClose?.BeginInvoke(this, new CloseEventArgs
			{
				Code = args.Code,
				Reason = args.Reason
			}, (c) => { }, null);
		}

		/*private void MessageWebSocket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
		{
			if(args.MessageType == SocketMessageType.Binary || !args.IsMessageComplete)
            {
				return;
            }
			try
			{
				using (DataReader dataReader = args.GetDataReader())
				{
					IBuffer message;
                    
						dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
						message = dataReader.ReadBuffer(dataReader.UnconsumedBufferLength);
						MessageWebSocket.Dispose();
					OnMessage?.BeginInvoke(this, new MessageEventArgs
					{
						Data = message
					}, (c) => { }, null);
				}
			}
			catch (Exception ex)
			{
				Windows.Web.WebErrorStatus webErrorStatus = Windows.Networking.Sockets.WebSocketError.GetStatus(ex.GetBaseException().HResult);
				OnError?.BeginInvoke(this, new ErrorEventArgs
				{
					Exception = ex,
					Message = ex.Message
				}, (c) => { }, null);
			}
		}*/
	}
}