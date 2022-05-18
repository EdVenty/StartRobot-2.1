using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;

namespace Start_Robot_2._1
{
	public class RobotIsRunningException : Exception
    {
		public RobotIsRunningException()
			: base("Robot is running and new robot can't be started.")
		{ }
    }
	public class RobotIsNotRunningException : Exception
	{
		public RobotIsNotRunningException()
			: base("Robot is not running now and can't be stopped.")
		{ }
	}
	public class RobotsManager
    {
		public event EventHandler OnRobotsLoaded;
		public event EventHandler OnRobotsSaved;
		public event EventHandler OnRobotsUpdated;
		public event EventHandler OnRobotRun;
		public event EventHandler OnRobotStop;
		public event EventHandler OnRobotConnectionInProgress;
		public event EventHandler OnRobotConnected;

		private readonly ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
		private readonly Logger Logger;

		public List<Robot> Robots { get; private set; }
		public Robot RunningRobot { get; private set; }
		public bool IsRobotRunning { get; private set; }

		public RobotsManager(Logger logger)
        {
			Logger = logger;
			Robots = new List<Robot>();
			OnRobotRun += (sender, e) => IsRobotRunning = true;
			OnRobotStop += (sender, e) => IsRobotRunning = false;
		}

		public void RunRobot(Robot robot)
        {
			if(RunningRobot != null)
            {
				throw new RobotIsRunningException();
            }
			RunningRobot = robot;
			RunningRobot.RobotActiveStateChanged += (sender, e) =>
			{
				if (e.AllChannelsActive && OnRobotConnected != null)
				{
					OnRobotConnected?.Invoke(this, null);
				}
                else
                {
					OnRobotConnectionInProgress?.Invoke(this, null);
                }
			};
			RunningRobot.Initialize(Logger);
			RunningRobot.Start();
            OnRobotRun?.Invoke(this, null);
			OnRobotConnectionInProgress.Invoke(this, null);
		}

		public void StopRobot()
        {
			if(RunningRobot == null)
            {
				throw new RobotIsNotRunningException();
            }
			RunningRobot.Stop();
			RunningRobot = null;
            OnRobotStop?.Invoke(this, null);
        }

		public void LoadRobots()
		{
			var rawRobotsXML = (string)roamingSettings.Values["Robots"];
			Robots = rawRobotsXML != null ? Deserialize<List<Robot>>(rawRobotsXML) : new List<Robot>();
            OnRobotsLoaded?.Invoke(null, new EventArgs());
        }

		public void SaveRobots()
		{
			roamingSettings.Values["Robots"] = Serialize(Robots);
            OnRobotsSaved?.Invoke(null, new EventArgs());
        }

		public void UpdateRobots()
		{
            OnRobotsUpdated?.Invoke(null, new EventArgs());
        }

		public static string Serialize(object obj)
		{
			using (var sw = new StringWriter())
			{
				var serializer = new XmlSerializer(obj.GetType());
				serializer.Serialize(sw, obj);
				return sw.ToString();
			}
		}

		public static T Deserialize<T>(string xml)
		{
			using (var sw = new StringReader(xml))
			{
				var serializer = new XmlSerializer(typeof(T));
				return (T)serializer.Deserialize(sw);
			}
		}

		public void RemoveRobot(Robot robot)
        {
			Robots.Remove(robot);
        }

		public void AddRobot(Robot robot)
        {
			Robots.Add(robot);
		}
	}
}
