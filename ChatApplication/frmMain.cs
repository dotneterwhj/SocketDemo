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

namespace ChatApplication
{
    public partial class frmMain : Form
    {
        private List<Socket> clientSockets = new List<Socket>();
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //1、创建服务器端Socket
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //2、服务器端绑定ip和端口号
            socket.Bind(new IPEndPoint(IPAddress.Parse(txtIP.Text), Convert.ToInt32(txtPort.Text)));
            //3、开启监听
            socket.Listen(10);//即使设置成很大，也跟操作系统自身相关。
            SetTextBoxText(string.Format("服务已开启\r\n"));
            //4、等待客户端的连接  由于Accept会阻塞当前线程 所以需要开辟新线程来执行 并且需要一直监听所以需要循环
            //Socket proxSocket =  socket.Accept();
            ThreadPool.QueueUserWorkItem(new WaitCallback(BeginAcceptClient), socket);

        }

        /// <summary>
        /// 开始等待客户端的连接
        /// </summary>
        /// <param name="state"></param>
        private void BeginAcceptClient(object state)
        {
            Socket socket = state as Socket;
            while (true)
            {
                Socket proxSocket = socket.Accept();
                SetTextBoxText(string.Format("客户端{0}已经连接\r\n", proxSocket.RemoteEndPoint.ToString()));
                clientSockets.Add(proxSocket);
                //客户端连接之后 接受客户端的消息 由于Receive也会阻塞当前线程 所以也需要开辟线程执行 并且一直接受消息
                //proxSocket.Receive()
                ThreadPool.QueueUserWorkItem(new WaitCallback(ReciveClientData), proxSocket);
                
            }
            
        }
        /// <summary>
        /// 接受客户端传来的数据
        /// </summary>
        /// <param name="state"></param>
        private void ReciveClientData(object state)
        {
            Socket proxSocket = state as Socket;
            byte[] buffer = new byte[1024 * 1024];
            while (true)
            {
                int reciveLen = 0;
                try
                {
                    reciveLen = proxSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    //客户端异常退出；
                    SetTextBoxText(string.Format("客户端{0}异常退出\r\n", proxSocket.RemoteEndPoint.ToString()));
                    clientSockets.Remove(proxSocket);
                    return;
                }
                if (reciveLen == 0)
                {
                    //客户端正常退出
                    SetTextBoxText(string.Format("客户端{0}正常退出\r\n", proxSocket.RemoteEndPoint.ToString()));
                    clientSockets.Remove(proxSocket);
                    return;
                }
                string msg = Encoding.Default.GetString(buffer);
                SetTextBoxText(msg);
            }
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

        private void btnSendMsg_Click(object sender, EventArgs e)
        {
            
            foreach (Socket socket in clientSockets)
            {
                if (socket.Connected)
                {
                    byte[] buffer = Encoding.Default.GetBytes(txtMsg.Text);
                    socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
                }
            }
        }
    }
}
