using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace TcpFileClient
{
    public partial class Form1 : Form
    {
        string m_spliter = "'\\'";
        string m_fName = string.Empty;
        string[] m_split = null;
        byte[] m_clientData = null;
        private const int ChunkSize = 1024; // 작은 데이터 조각의 크기

        enum DataPacketType { TEXT = 1, IMAGE};


        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            char[] delimeter = m_spliter.ToCharArray();

            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

            openFileDialog.ShowDialog();

            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            textBox1.Text = openFileDialog.FileName;
            pictureBox1.ImageLocation = openFileDialog.FileName;

            m_split = textBox1.Text.Split(delimeter);
            int limit = m_split.Length;

            m_fName = m_split[limit - 1].ToString();

            if (textBox1.Text != null)
                button2.Enabled = true;          
        }

                private void button2_Click(object sender, EventArgs e)
                {
                    Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    byte[] fileName = Encoding.UTF8.GetBytes(m_fName);
                    byte[] fileData = File.ReadAllBytes(textBox1.Text);
                    byte[] fileNameLen = BitConverter.GetBytes(fileName.Length);
                    byte[] fileType = BitConverter.GetBytes((int)DataPacketType.IMAGE);
                    m_clientData = new byte[fileType.Length + 4 + fileName.Length + fileData.Length];

                    fileType.CopyTo(m_clientData, 0);
                    fileNameLen.CopyTo(m_clientData, 4);
                    fileName.CopyTo(m_clientData, 8);
                    fileData.CopyTo(m_clientData, 8 + fileName.Length);

                    clientSocket.Connect(IPAddress.Parse("192.168.56.1"), 9050);
                    clientSocket.Send(m_clientData);
                    clientSocket.Close();
                }

        private void button3_Click(object sender, EventArgs e)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            byte[] textData = Encoding.UTF8.GetBytes(textBox2.Text);
            byte[] fileType = BitConverter.GetBytes((int)DataPacketType.TEXT);
            m_clientData = new byte[fileType.Length + textData.Length];

            fileType.CopyTo(m_clientData, 0);
            textData.CopyTo(m_clientData, 4);

            clientSocket.Connect(IPAddress.Parse("192.168.56.1"), 9050);
            clientSocket.Send(m_clientData);
            clientSocket.Close();
        }
    }
}
