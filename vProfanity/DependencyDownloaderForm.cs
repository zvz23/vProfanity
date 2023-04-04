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

namespace vProfanity
{
    public partial class DependencyDownloaderForm : Form
    {
        public DependencyDownloaderForm()
        {
            InitializeComponent();
            progressBar1.Maximum = 100;
            progressBar1.Step = 1;
            progressBar1.Value = 0;
            
        }
       
        private async void button1_Click(object sender, EventArgs e)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromMinutes(5);
                

                var response = await httpClient.GetAsync(DependencyDownloader.DOWNLOAD_PAGE);
                var html = await response.Content.ReadAsStringAsync();

                var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(html);
                var form = htmlDoc.DocumentNode.SelectSingleNode("//form[@id='download-form']");
                string downloadUrl = form.GetAttributeValue<string>("action", null);
                string fileName = Path.Combine(AppConstants.APP_DATA_FOLDER_NAME, "vProfanityModel.zip");

                response = await httpClient.PostAsync(downloadUrl, null);
                response.EnsureSuccessStatusCode();
                var totalBytes = response.Content.Headers.ContentLength;

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                {
                    using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        var totalBytesRead = 0L;
                        var bytesRead = 0;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);

                            totalBytesRead += bytesRead;

                            if (totalBytes.HasValue)
                            {
                                var percentage = (totalBytesRead * 100) / totalBytes.Value;
                                progressBar1.Invoke(new Action(() => progressBar1.Value = (int)percentage));
                            }
                        }
                    }
                }


            }
        }
    }
}
