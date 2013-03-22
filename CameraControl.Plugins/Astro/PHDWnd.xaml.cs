using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CameraControl.Plugins.Astro
{

    public enum OpersEnum
    {
        MSG_PAUSE = 1,
        MSG_RESUME,
        MSG_MOVE1,
        MSG_MOVE2,
        MSG_MOVE3,
        MSG_IMAGE,
        MSG_GUIDE,
        MSG_CAMCONNECT,
        MSG_CAMDISCONNECT,
        MSG_REQDIST,
        MSG_REQFRAME,
        MSG_MOVE4,
        MSG_MOVE5,
        MSG_Wrong
    }

    /// <summary>
    /// Interaction logic for PHDWnd.xaml
    /// </summary>
    public partial class PHDWnd
    {
        private Socket socket;
        public PHDWnd()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Connect();
        }

        private void Connect()
        {
            try
            {
                IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 4300);

                // Create a TCP/IP  socket.
                socket = new Socket(AddressFamily.InterNetwork,
                                           SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(remoteEP);
            }
            catch (Exception exception)
            {
                
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            SendReceiveTest2(socket,OpersEnum.MSG_PAUSE);
        }

        public static int SendReceiveTest2(Socket server, OpersEnum opersEnum)
        {
            byte[] msg = Encoding.UTF8.GetBytes("This is a test");
            byte[] bytes = new byte[256];
            try
            {
                // Blocks until send returns. 
                int byteCount = server.Send(new[]{(byte)opersEnum}, SocketFlags.None);
                Console.WriteLine("Sent {0} bytes.", byteCount);

                // Get reply from the server.
                byteCount = server.Receive(bytes, SocketFlags.None);
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

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            SendReceiveTest2(socket, OpersEnum.MSG_RESUME);
        }

        private void but_move_Click(object sender, RoutedEventArgs e)
        {
            SendReceiveTest2(socket, OpersEnum.MSG_MOVE2);
        }

        private void btn_guid_Click(object sender, RoutedEventArgs e)
        {
            SendReceiveTest2(socket, OpersEnum.MSG_GUIDE);
        }
    }
}
