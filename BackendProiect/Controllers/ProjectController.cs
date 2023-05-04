using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;

namespace BackendProiect.Controllers
{
    [ApiController]
    public class ProjectController : Controller
    {
        [HttpGet]
        [Route("GetScrappResult")]
        public async Task<List<string>> GetScrappResult()
        {
            List<string> Datalst = new List<string>();

            HttpClient hc = new HttpClient();
            HttpResponseMessage result = await hc.GetAsync($"https://ro.wikipedia.org/wiki/List%C4%83_de_r%C3%A2uri_din_America");
            Stream stream = await result.Content.ReadAsStreamAsync();
            HtmlDocument doc = new HtmlDocument();
            doc.Load(stream);

            var HeaderNames = doc.DocumentNode.SelectNodes("//span[@class='mw-headline']");

            foreach (var HeaderName in HeaderNames)
            {
                Datalst.Add(HeaderName.InnerText);
            }
            return Datalst;
        }

    }
}
