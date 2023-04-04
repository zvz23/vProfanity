using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace vProfanity.Services
{
    public static class DependencyDownloader
    {
        public static readonly  string DOWNLOAD_PAGE  = "https://drive.google.com/uc?id=1_wMubLljuMEOxo1T07PoGd0YGTx6hqcw&export=download";

        public static async Task Download()
        {
            using(var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(DOWNLOAD_PAGE);
                var html = await response.Content.ReadAsStringAsync();

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);
                var form = htmlDoc.DocumentNode.SelectSingleNode("//form[@id='download-form']");
                string downloadUrl = form.GetAttributeValue<string>("action", null);
            }
            
            


        }
    }


}
