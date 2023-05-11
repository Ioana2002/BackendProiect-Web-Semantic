using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;

namespace BackendProiect.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProjectController : Controller
    {
        [HttpGet]
        [Route("GetScrappResult")]
        public async Task<List<Movie>> GetScrappResult()
        {

            HttpClient hc = new HttpClient();
            HttpResponseMessage result = await hc.GetAsync($"https://en.wikipedia.org/wiki/Marvel_Cinematic_Universe");
            Stream stream = await result.Content.ReadAsStreamAsync();
            HtmlDocument doc = new HtmlDocument();
            doc.Load(stream);

            var Years = doc.DocumentNode.SelectNodes("//table[@class='wikitable']/tbody/tr/th[@scope='row']");
            var Names = doc.DocumentNode.SelectNodes("//table[@class='wikitable']/tbody/tr/td/i");

            var tuples = Years.Zip(Names, (yearNode, nameNode) => Tuple.Create(yearNode.InnerText, nameNode));


            var Datalst = new List<Movie>();

            /*foreach (var year in Years)
             {
                 Datalst.Add(new Movie{ Year = year.InnerHtml});
             }

             foreach (var name in Names)
             {
                 Datalst.Add(new Movie { Name = name.InnerHtml });
             }*/

            foreach (var tuple in tuples)
            {
                Datalst.Add(new Movie { Year = tuple.Item1, Name = tuple.Item2.InnerText });
            }

            return Datalst;
        }

        public class Movie
        {
            public string? Name { get; set; }
            public string? Year { get; set; }

        }

        [HttpPost]
        [Route("UploadMovie")]
        public async Task<IActionResult> PostJson([FromBody] Movie movie)
        {
            try
            {
                string apiUrl = "http://localhost:4000/movies";

                var jsonRequestBody = Newtonsoft.Json.JsonConvert.SerializeObject(movie);

                using var httpClient = new HttpClient();

                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);

                string responseContent = await response.Content.ReadAsStringAsync();

                return Ok(responseContent);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }

 
}
