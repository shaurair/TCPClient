using SuperSimpleTcp;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TCPClient
{
    public partial class Client : Form
    {
        public Client()
        {
            InitializeComponent();
        }

        SimpleTcpClient client;
        ClientAct Client_Act = new ClientAct();

        private void Form1_Load(object sender, EventArgs e)
        {
            client = new($"{txtIP.Text}:{txtPort.Text}");
            client.Events.Connected += Events_Connected;
            client.Events.DataReceived += Events_DataReceived;
            client.Events.Disconnected += Events_Disconnected;

            try
            {
                client.Connect();
                btnStart.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Events_DataReceived(object sender, DataReceivedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtMessage.Text = Client_Act.DownLoadStatus(e.Data);
                if (txtMessage.Text != "File receiving")
                {
                    btnStart.Enabled = true;
                }
            });
        }

        private void Events_Disconnected(object sender, ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtMessage.Text = "Disconnected";
            });
        }

        private void Events_Connected(object sender, ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtMessage.Text = "Connected";
            });
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                btnStart.Enabled = false;
                Client_Act.SetFileReq(txtFileName.Text,txtFilePath.Text);
                client.Send(txtFileName.Text);
                btnStart.Enabled = true;
            });
        }
    }

    public class ClientAct
    {
        string FullFileSavePath = string.Empty;
        bool IsFileTransfer = false;

        public void SetFileReq(string FileName, string SavePath)
        {
            FullFileSavePath = $"{SavePath}{FileName}";
            IsFileTransfer = false;
        }

        public string DownLoadStatus( ArraySegment<byte> data)
        {
            string content = Encoding.UTF8.GetString(data);
            if(IsFileTransfer)
            {
                if (File.Exists(FullFileSavePath))
                {
                    File.Delete(FullFileSavePath);
                }

                var WriteData = data;
                using (FileStream fs = new FileStream(FullFileSavePath, FileMode.CreateNew))
                {
                    fs.Write(WriteData);
                    fs.Close();
                }

                return "Success";
            }

            if (content == "Wrong file name")
            {
                return content;
            }
            else
            {
                IsFileTransfer = true;

                return "File receiving";
            }
        }
    }
}
