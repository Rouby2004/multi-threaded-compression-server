using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CompressionServer
{
    public partial class ServerForm : Form
    {
        public ServerForm()
        {
            InitializeComponent();
        }
        TcpListener listener;
        bool serverRunning = false;

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, 5000);

                listener.Start();

                serverRunning = true;

                lblStatus.Text = "Server Running...";

                btnStart.Enabled = false;

                AddLog("Server Started On Port 5000");

                Task.Run(() => AcceptClients());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AcceptClients()
        {
            while (serverRunning)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();

                    AddLog("Client Connected");

                    Task.Run(() => HandleClient(client));
                }
                catch (Exception ex)
                {
                    AddLog("Accept Error: " + ex.Message);
                }
            }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                using (NetworkStream stream = client.GetStream())
                using (BinaryReader reader = new BinaryReader(stream))
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    // =========================
                    // Receive File Size
                    // =========================
                    long fileSize = reader.ReadInt64();

                    AddLog("Receiving File Size: " + fileSize + " bytes");

                    // =========================
                    // Receive File Data
                    // =========================
                    byte[] fileData = new byte[fileSize];

                    int totalRead = 0;

                    while (totalRead < fileSize)
                    {
                        int bytesRead = stream.Read(
                            fileData,
                            totalRead,
                            (int)(fileSize - totalRead)
                        );

                        if (bytesRead == 0)
                            throw new Exception("Connection Closed");

                        totalRead += bytesRead;
                    }

                    AddLog("File Received Successfully");

                    // =========================
                    // Compress File
                    // =========================
                    byte[] compressedData;

                    using (MemoryStream output = new MemoryStream())
                    {
                        using (GZipStream gzip =
                            new GZipStream(output, CompressionMode.Compress))
                        {
                            gzip.Write(fileData, 0, fileData.Length);
                        }

                        compressedData = output.ToArray();
                    }

                    AddLog("File Compressed");

                    // =========================
                    // Send Compressed Size
                    // =========================
                    writer.Write((long)compressedData.Length);

                    // =========================
                    // Send Compressed File
                    // =========================
                    writer.Write(compressedData);

                    writer.Flush();

                    AddLog("Compressed File Sent");

                    AddLog("--------------------------------");
                }
            }
            catch (Exception ex)
            {
                AddLog("Client Error: " + ex.Message);
            }
            finally
            {
                client.Close();

                AddLog("Client Disconnected");
            }
        }

        private void AddLog(string message)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() =>
                {
                    lstLogs.Items.Add(
                        DateTime.Now.ToString("HH:mm:ss") +
                        " - " +
                        message
                    );

                    lstLogs.TopIndex = lstLogs.Items.Count - 1;
                }));
            }
            else
            {
                lstLogs.Items.Add(
                    DateTime.Now.ToString("HH:mm:ss") +
                    " - " +
                    message
                );

                lstLogs.TopIndex = lstLogs.Items.Count - 1;
            }
        }

        private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                serverRunning = false;

                listener?.Stop();
            }
            catch
            {

            }
        }
    }
}
