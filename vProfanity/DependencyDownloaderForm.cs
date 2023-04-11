using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using vProfanity.Services;
using HtmlAgilityPack;
using System.IO;
using System.Security.Policy;
using System.Net;

namespace vProfanity
{
    public partial class DependencyDownloaderForm : Form
    {
        public DependencyDownloaderForm()
        {
            InitializeComponent();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerAsync();

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            using (var webClient = new WebClient())
            {
                webClient.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36");
                string html = webClient.DownloadString(DependencyDownloader.DOWNLOAD_PAGE);
                var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(html);
                var form = htmlDoc.DocumentNode.SelectSingleNode("//form[@id='download-form']");
                string downloadUrl = form.GetAttributeValue<string>("action", null);

                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                webClient.DownloadFileAsync(new Uri(downloadUrl), @"D:\vProfanityModel.zip");
                while (webClient.IsBusy)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }



        }
        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Report the progress of the download to the BackgroundWorker
            backgroundWorker1.ReportProgress(e.ProgressPercentage);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("Error: " + e.Error.Message);
            }
            else
            {
                MessageBox.Show("Download complete.");
            }
        }
    }
}
