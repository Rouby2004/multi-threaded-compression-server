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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CompressionClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        // Browse File
        // =========================
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtFile.Text = openFileDialog1.FileName;
            }
        }

        // =========================
        // Send File
        // =========================
        private async void btnSend_Click(object sender, EventArgs e)
        {
            // =========================
            // Validate File
            // =========================
            if (string.IsNullOrWhiteSpace(txtFile.Text))
            {
                MessageBox.Show("Please Select File");

                return;
            }

            if (!File.Exists(txtFile.Text))
            {
                MessageBox.Show("File Not Found");

                return;
            }

            btnSend.Enabled = false;

            lblStatus.Text = "Connecting To Server...";

            try
            {
                using (TcpClient client = new TcpClient())
                {
                    // =========================
                    // Connect To Server
                    // =========================
                    await client.ConnectAsync("127.0.0.1", 5000);

                    lblStatus.Text = "Connected";

                    using (NetworkStream stream = client.GetStream())
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        // =========================
                        // Read File
                        // =========================
                        byte[] fileData = File.ReadAllBytes(txtFile.Text);

                        lblStatus.Text = "Sending File...";

                        // =========================
                        // Send File Size
                        // =========================
                        writer.Write((long)fileData.Length);

                        // =========================
                        // Send File Data
                        // =========================
                        writer.Write(fileData);

                        writer.Flush();

                        lblStatus.Text = "Waiting For Compressed File...";

                        // =========================
                        // Receive Compressed Size
                        // =========================
                        long compressedSize = reader.ReadInt64();

                        // =========================
                        // Receive Compressed Data
                        // =========================
                        byte[] compressedData = new byte[compressedSize];

                        int totalRead = 0;

                        while (totalRead < compressedSize)
                        {
                            int bytesRead = stream.Read(
                                compressedData,
                                totalRead,
                                (int)(compressedSize - totalRead)
                            );

                            if (bytesRead == 0)
                                throw new Exception("Connection Closed");

                            totalRead += bytesRead;
                        }

                        lblStatus.Text = "Saving File...";

                        // =========================
                        // Create Output Path
                        // =========================
                        string originalName =
                            Path.GetFileNameWithoutExtension(txtFile.Text);

                        string outputPath = Path.Combine(
                            Path.GetDirectoryName(txtFile.Text),
                            originalName + "_compressed.gz"
                        );

                        // =========================
                        // Save File
                        // =========================
                        File.WriteAllBytes(outputPath, compressedData);

                        lblStatus.Text = "Compressed File Received";

                        MessageBox.Show(
                            "Compressed File Saved Successfully\n\n" +
                            outputPath,
                            "Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error";

                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                btnSend.Enabled = true;
            }
        }
    }
}
