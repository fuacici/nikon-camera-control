using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Xml;
using CameraControl.Devices;

namespace CameraControl.Core.Scripting.ScriptCommands
{
    public class PHDGuiding:BaseScript
    {

        public override bool Execute(ScriptObject scriptObject)
        {
            try
            {
                StaticHelper.Instance.SystemMessage = "PHDGuiding started";
                Executing = true;
                TcpClient socket = new TcpClient("localhost", 4300);
                Thread.Sleep(200);
                switch (MoveType)
                {
                    case "Move 1":
                        SendReceiveTest2(socket, 3);
                        break;
                    case "Move 2":
                        SendReceiveTest2(socket, 4);
                        break;
                    case "Move 3":
                        SendReceiveTest2(socket, 5);
                        break;
                    case "Move 4":
                        SendReceiveTest2(socket, 12);
                        break;
                    case "Move 5":
                        SendReceiveTest2(socket, 13);
                        break;
                }
                StaticHelper.Instance.SystemMessage = "PHDGuiding waiting....";
                Thread.Sleep(WaitTime);
                socket.Close();
            }
            catch (Exception exception)
            {
                StaticHelper.Instance.SystemMessage = "PHDGuiding error "+exception.Message;
                Log.Error("PHDGuiding error", exception);
            }
            StaticHelper.Instance.SystemMessage = "PHDGuiding done";
            return true;
        }

        public override IScriptCommand Create()
        {
            return new PHDGuiding();
        }

        public override XmlNode Save(XmlDocument doc)
        {
            XmlNode nameNode = doc.CreateElement("PHDGuiding");
            nameNode.Attributes.Append(ScriptManager.CreateAttribute(doc, "WaitTime", WaitTime.ToString()));
            nameNode.Attributes.Append(ScriptManager.CreateAttribute(doc, "MoveType", MoveType));
            return nameNode;
        }

        public override IScriptCommand Load(XmlNode node)
        {
            PHDGuiding res = new PHDGuiding
            {
                WaitTime = Convert.ToInt32(ScriptManager.GetValue(node, "WaitTime")),
                MoveType = ScriptManager.GetValue(node, "MoveType"),
                //Aperture = ScriptManager.GetValue(node, "Aperture")
            };
            return res;
        }

        public override UserControl GetConfig()
        {
            return new PHDGuidingControl(this);
        }

        public override string DisplayName
        {
            get { return string.Format("[{0}][MoveType={1}, WaitTime={2}]", Name, MoveType, WaitTime); }
            set { }
        }

        private int _waitTime;
        public int WaitTime
        {
            get { return _waitTime; }
            set
            {
                _waitTime = value;
                NotifyPropertyChanged("WaitTime");
                NotifyPropertyChanged("DisplayName");
            }
        }

        private string _moveType;
        public string MoveType
        {
            get { return _moveType; }
            set
            {
                _moveType = value;
                NotifyPropertyChanged("MoveType");
                NotifyPropertyChanged("DisplayName");
            }
        }

        public PHDGuiding()
        {
            Name = "PHDGuiding";
            WaitTime = 2000;
            MoveType = "Move 1";
        }

        public static int SendReceiveTest2(TcpClient server, byte opersEnum)
        {
            byte[] msg = Encoding.UTF8.GetBytes("This is a test");
            byte[] bytes = new byte[256];
            try
            {
                // Blocks until send returns. 
                int byteCount = server.Client.Send(new[] { opersEnum }, SocketFlags.None);
                Console.WriteLine("Sent {0} bytes.", byteCount);

                // Get reply from the server.
                byteCount = server.Client.Receive(bytes, SocketFlags.None);
                Console.WriteLine(byteCount);
                //if (byteCount > 0)
                //    Console.WriteLine(Encoding.UTF8.GetString(bytes));
            }
            catch (SocketException e)
            {
                Console.WriteLine("{0} Error code: {1}.", e.Message, e.ErrorCode);
                return (e.ErrorCode);
            }
            return 0;
        }
    }
}
