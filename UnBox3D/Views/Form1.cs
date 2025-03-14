using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UnBox3D.Views
{
    public partial class Form1 : Form
    {

        private const string testURL = "https://images.unsplash.com/photo-1726221439759-7e7c446a2e63?q=80&w=1974&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D";


        public Form1()
        {
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            backgroundWorker1.RunWorkerAsync();
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Enabled = true;
            MessageBox.Show("Done!");

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            MessageBox.Show("downloding");
            DownloadFile(textBox1.Text, textBox2.Text, blenderDlProgress, progressLabel);
        }
        private void DownloadFile(string link, string targetPath, ProgressBar P, Label label)
        {
            int bytesProcessed = 0;
            Stream remoteSteam = null;
            Stream localSteam = null;
            WebResponse response = null;

            try
            {
                WebRequest request = WebRequest.Create(link);
                if (request != null)
                {
                    double totalBytesToReceive = 0;
                    var SizeWebRequest = HttpWebRequest.Create(new Uri(link));
                    SizeWebRequest.Method = "HEAD";

                    using (var webResponse = SizeWebRequest.GetResponse())
                    {
                        var fileSize = webResponse.Headers.Get("Content-Length");
                        totalBytesToReceive = Convert.ToDouble(fileSize);
                    }

                    response = request.GetResponse();
                    if (response != null)
                    {
                        remoteSteam = response.GetResponseStream();

                        string filePath = targetPath;

                        localSteam = File.Create(filePath);

                        byte[] buffer = new byte[1024];
                        int bytesRead = 0;


                        do
                        {
                            bytesRead = remoteSteam.Read(buffer, 0, buffer.Length);

                            localSteam.Write(buffer, 0, bytesRead);

                            bytesProcessed += bytesRead;
                            double bytesIn = double.Parse(bytesProcessed.ToString());
                            double percentge = bytesIn / totalBytesToReceive * 100;
                            percentge = Math.Round(percentge, 0);


                            if (P.InvokeRequired)
                            {
                                P.Invoke(new Action(() => P.Value = int.Parse(Math.Truncate(percentge).ToString())));
                            }
                            else
                            {
                                P.Value = int.Parse(Math.Truncate(percentge).ToString());
                            }

                            if (label.InvokeRequired)
                            {
                                label.Invoke(new Action(() => label.Text = int.Parse(Math.Truncate(percentge).ToString()).ToString()));
                            }
                            else
                            {
                                label.Text = int.Parse(Math.Truncate(percentge).ToString()).ToString();
                            }

                        } while (bytesRead > 0);
                    }

                }
            }
            catch
            {

            }
            finally
            {
                response?.Close();
                remoteSteam?.Close();
                localSteam?.Close();
            }

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
