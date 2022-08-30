using SuperSimpleTcp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
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

    }
}
