using SuperSimpleTcp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System;

namespace TestProject2
{
    [TestClass]
    public class TP2
    {
        TCPClient.ClientAct ClientTest = new TCPClient.ClientAct();
        TCPServer.ServerAct ServerTest = new TCPServer.ServerAct();

        [TestMethod]
        public void TestClient_Normal()
        {
            ClientTest.SetFileReq("Test1.txt", "D:/shaurair/CSharp/TCPClientFileSavePath");
            var TestData = new ArraySegment<byte>(Encoding.ASCII.GetBytes("File exist"));

            Assert.AreEqual(ClientTest.DownLoadStatus(TestData), "File receiving");
        }
        [TestMethod]
        public void TestClient_Abnormal()
        {
            var TestData = new ArraySegment<byte>(Encoding.ASCII.GetBytes("Wrong file name"));

            Assert.AreEqual(ClientTest.DownLoadStatus(TestData), "Wrong file name");
        }

        [TestMethod]
        public void TestServer_Normal()
        {
            Assert.IsTrue(ServerTest.IsFileExist("Test1.txt"));
        }

        [TestMethod]
        public void TestServer_Abnormal()
        {
            Assert.IsFalse(ServerTest.IsFileExist(""));
        }

        [TestMethod]
        public void ClientServerConnection()
        {
            var expectedClientConnectedCount = 1;
            var clientConnectedCount = 0;

            // server init
            using var simpleTcpServer = new SimpleTcpServer("127.0.0.1:1900");
            simpleTcpServer.Start();
            simpleTcpServer.Events.ClientConnected += Events_ClientConnected;

            void Events_ClientConnected(object? sender, ConnectionEventArgs e)
            {
                clientConnectedCount++;
            }

            // wait server on line
            Thread.Sleep(3000);

            // client init
            using var simpleTcpClient = new SimpleTcpClient("127.0.0.1:1900");
            simpleTcpClient.Connect();

            // wait client connect
            Thread.Sleep(3000);

            simpleTcpClient.Disconnect();
            simpleTcpServer.Events.ClientConnected -= Events_ClientConnected;
            simpleTcpServer.Stop();

            // check connect count
            Assert.AreEqual(expectedClientConnectedCount, clientConnectedCount);
        }

        [TestMethod]
        public void ServerSendInfo()
        {
            string ClientIP = string.Empty;
            string ReceiveData = string.Empty;
            string TestInfo = "Test";

            // server init
            using var simpleTcpServer = new SimpleTcpServer("127.0.0.1:1900");
            simpleTcpServer.Start();
            simpleTcpServer.Events.ClientConnected += Events_ClientConnected;

            void Events_ClientConnected(object? sender, ConnectionEventArgs e)
            {
                ClientIP = e.IpPort;
            }

            Thread.Sleep(3000);

            // client init
            using var simpleTcpClient = new SimpleTcpClient("127.0.0.1:1900");
            simpleTcpClient.Connect();
            simpleTcpClient.Events.DataReceived += ClientEvents_DataReceived;

            // client receive info
            void ClientEvents_DataReceived(object? sender, DataReceivedEventArgs e)
            {
                ReceiveData = Encoding.UTF8.GetString(e.Data);
            }

            Thread.Sleep(3000);

            simpleTcpServer.Send(ClientIP, TestInfo);

            // wait client receive data
            Thread.Sleep(3000);

            simpleTcpClient.Disconnect();
            simpleTcpServer.Events.ClientConnected -= Events_ClientConnected;
            simpleTcpServer.Stop();

            Assert.AreEqual(ReceiveData, TestInfo);
        }

        [TestMethod]
        public void ServerSendFile()
        {
            string ServerFilepath = "D:/shaurair/CSharp/TCPServerFilePath/Test1.txt";
            string ClientSaveFilepath = "D:/shaurair/CSharp/TCPClientFileSavePath/Test1.txt";
            string ClientIP = string.Empty;

            if (File.Exists(ClientSaveFilepath))
            {
                File.Delete(ClientSaveFilepath);
            }

            // server init
            using var simpleTcpServer = new SimpleTcpServer("127.0.0.1:1900");
            simpleTcpServer.Start();
            simpleTcpServer.Events.ClientConnected += Events_ClientConnected;

            void Events_ClientConnected(object? sender, ConnectionEventArgs e)
            {
                ClientIP = e.IpPort;
            }

            Thread.Sleep(3000);

            // client init
            using var simpleTcpClient = new SimpleTcpClient("127.0.0.1:1900");
            simpleTcpClient.Connect();
            simpleTcpClient.Events.DataReceived += ClientEvents_DataReceived;

            // client receive file
            void ClientEvents_DataReceived(object? sender, DataReceivedEventArgs e)
            {
                var WriteData = e.Data;
                using (FileStream fs = new FileStream(ClientSaveFilepath, FileMode.CreateNew))
                {
                    fs.Write(WriteData);
                    fs.Close();
                }
            }

            Thread.Sleep(3000);

            // server send file
            using (var fs = new FileStream(ServerFilepath, FileMode.Open))
            {
                simpleTcpServer.SendAsync(ClientIP, fs.Length, fs);
            }

            // wait for client receive data
            Thread.Sleep(3000);

            //await Task.Delay(10);
            simpleTcpClient.Disconnect();
            simpleTcpServer.Events.ClientConnected -= Events_ClientConnected;
            simpleTcpServer.Stop();

            // check if file transfer correct
            bool CompareTwoFile(string File1, string File2)
            {
                byte[] FileContent1 = File.ReadAllBytes(File1);
                byte[] FileContent2 = File.ReadAllBytes(File2);
                if( FileContent1.Length != FileContent2.Length )
                {
                    return false;
                }
                else
                {
                    for (int i = 0; i < FileContent1.Length; i++)
                    {
                        if( FileContent1[i] != FileContent2[i])
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            Assert.IsTrue(CompareTwoFile(ServerFilepath,ClientSaveFilepath));
        }
    }
}
