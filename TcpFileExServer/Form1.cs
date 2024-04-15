using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;

namespace TcpFileExServer
{
    public partial class Form1 : Form
    {

        bool initialFlag = true;
        string receivedPath = string.Empty;
        enum DataPacketType { TEXT = 1, IMAGE};
        int dataType = 0;
        string textData = string.Empty;

        public Form1()
        {
            InitializeComponent();

            Thread t_handler = new Thread(StartListening);
            t_handler.IsBackground = true;
            t_handler.Start();
        }

        public static ManualResetEvent allDone = new ManualResetEvent(false);
        private void StartListening()
        {
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, 9050);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEP);
                listener.Listen(10);

                while(true)
                {
                    allDone.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                }
            } catch(SocketException se)
            {
                Trace.WriteLine(string.Format("SocketException : {0}",se.Message));
            } catch(Exception ex)
            {
                Trace.WriteLine(string.Format("Exception : {0}",ex.Message));
            }
           
              
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Console.WriteLine("접속");
            allDone.Set();

            Socket listener = ar.AsyncState as Socket;
            Socket handler = listener.EndAccept(ar);

            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            initialFlag = true;
        }

        private void ReadCallback(IAsyncResult ar)
        {
            int fileNameLen = 0;
            string content = string.Empty;
            StateObject state = ar.AsyncState as StateObject;
            Socket handler = state.workSocket;
            int bytesRead = handler.EndReceive(ar);

            if(bytesRead > 0)
            {

                if(initialFlag)
                {
                    dataType = BitConverter.ToInt32(state.buffer, 0);

                    if(dataType == (int)DataPacketType.IMAGE)
                    {
                        fileNameLen = BitConverter.ToInt32(state.buffer, 4);
                        string fileName = Encoding.UTF8.GetString(state.buffer, 8, fileNameLen);

                        string pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        string pathDownload = Path.Combine(pathUser, "Downloads");

                        receivedPath = Path.Combine(pathDownload, fileName);

                        if (File.Exists(receivedPath))
                            File.Delete(receivedPath);

                    }
                    else if(dataType == (int)DataPacketType.TEXT)
                    {
                        textData = Encoding.UTF8.GetString(state.buffer, 4, bytesRead - 4);
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                    }
                }

                if (dataType == (int)DataPacketType.IMAGE)
                {
                    BinaryWriter bw = new BinaryWriter(File.Open(receivedPath, FileMode.Append));
                    if (initialFlag)
                        bw.Write(state.buffer, 8 + fileNameLen, bytesRead - (8 + fileNameLen));
                    else
                        bw.Write(state.buffer, 0, bytesRead);

                    initialFlag = false;
                    bw.Close();
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
            }
            else
            {

                if (dataType == (int)DataPacketType.IMAGE)
                {
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox1.ImageLocation = receivedPath;
                    Invoke((MethodInvoker)delegate
                    {
                        label1.Text = "Data has been received";
                    });
                }
                else if (dataType == (int)DataPacketType.TEXT)
                    Invoke((MethodInvoker)delegate
                    {
                        textBox1.Text = textData;
                    });

            }

            
        }
        


        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
