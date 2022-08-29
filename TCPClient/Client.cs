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
                btnStart.Enabled = true;

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
        string FillFileSavePath = string.Empty;
        
        public void SetFileReq(string FileName,string SavePath)
        {
            FillFileSavePath = $"{SavePath}{FileName}";
        }

        public string DownLoadStatus( ArraySegment<byte> data)
        {
            string content = Encoding.UTF8.GetString(data);

            if (content == "Wrong file name")
            {
                return content;
            }
            else
            {
                if (File.Exists(FillFileSavePath))
                {
                    File.Delete(FillFileSavePath);
                }

                var WriteData = data;
                using (FileStream fs = new FileStream(FillFileSavePath, FileMode.CreateNew))
                {
                    fs.Write(WriteData);
                    fs.Close();
                }

                return "Success";
            }
        }
    }
}
