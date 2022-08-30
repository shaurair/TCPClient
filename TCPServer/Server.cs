using SuperSimpleTcp;
using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace TCPServer
{
    public partial class Server : Form
    {
        public Server()
        {
            InitializeComponent();
        }

        SimpleTcpServer server;
        ServerAct Server_Act = new ServerAct();
        string ClientIP = string.Empty;
        private void Form1_Load(object sender, EventArgs e)
        {
            server = new SimpleTcpServer("127.0.0.1:1900");
            server.Events.ClientConnected += Events_ClientConnected;
            server.Events.DataReceived += Events_DataReceived;

            server.Start();
        }

        private void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                string content = Encoding.UTF8.GetString(e.Data);
                string FullFilepath = $"{Server_Act.FilePath}/{content}";

                txtFileReq.Text = content;
                if(Server_Act.IsFileExist(content))
                {
                    server.Send(ClientIP, "File exist");
                    using (var fs = new FileStream(FullFilepath, FileMode.Open))
                    {
                        server.SendAsync(ClientIP, fs.Length, fs);
                    }
                }
                else
                {
                    server.Send(ClientIP, "Wrong file name");
                }
            });
        }

        private void Events_ClientConnected(object sender, ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                ClientIP = e.IpPort;
                string[] IpPort = Regex.Split(e.IpPort, ":");
                txtClientIP.Text = IpPort[0];
                txtClientPort.Text = IpPort[1];
            });
            
        }
    }

    public class ServerAct
    {
        string ClientIP = string.Empty;
        string ServerFilepath = "D:/shaurair/CSharp/TCPServerFilePath/";
        bool IsSendFile = false;
        string SendMessage = string.Empty;

        public string FilePath
        {
            get { return ServerFilepath; }
        }

        public bool IsFileExist(string FileName)
        {
            if (!File.Exists($"{ServerFilepath}{FileName}") || FileName == string.Empty)
            {
                return false;
            }
            return true;
        }
    }


}
