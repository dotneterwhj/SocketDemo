using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientDemo
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void SetTextBoxText(string msg)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string>(s => {
                    txtLog.Text = s + txtLog.Text;
                }), msg);
            }
            else
            {
                txtLog.Text = msg + txtLog.Text;
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            //1.创建客户端Socket
            Socket socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);

            //2.连接服务器端
            try
            {
                socket.Connect(IPAddress.Parse(txtIP.Text), Convert.ToInt32(txtPort.Text));
            }
            catch (Exception)
            {
                SetTextBoxText("服务器端异常！");
            }

            //3.接受服务器端的信息
            ThreadPool.QueueUserWorkItem(new WaitCallback(BeginReceiveServerData), socket);

        }

        private void BeginReceiveServerData(object state)
        {
            Socket socket = state as Socket;
            byte[] buffer = new byte[1024 * 1024];
            while (true)
            {
                int receiveLen = 0;
                try
                {
                    receiveLen = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                }
                catch (Exception)
                {
                    SetTextBoxText(string.Format("服务器端{0}异常退出", socket.RemoteEndPoint.ToString()));
                    StopConnect(socket);
                    return;
                }
               
                if (receiveLen == 0)
                {
                    SetTextBoxText(string.Format("服务器端{0}正常退出",socket.RemoteEndPoint.ToString()));
                    StopConnect(socket);
                    return;
                }
                string msg = Encoding.Default.GetString(buffer);
                SetTextBoxText(string.Format("接收到服务器端{0}的消息:{1}", socket.RemoteEndPoint.ToString(), msg));
                
            }
        }

        private void StopConnect(Socket socket)
        {
            if (socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }
    }
}
