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
using Newtonsoft.Json.Linq;

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
        public async Task<IActionResult> PostJson([FromBody] Movie[] movies)
        {
            try
            {
                string responseData = "";
                foreach (Movie movie in movies)
                {
                    string apiUrl = "http://localhost:4000/movies";

                    var jsonRequestBody = Newtonsoft.Json.JsonConvert.SerializeObject(movie);

                    using var httpClient = new HttpClient();

                    var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);

                    responseData = await response.Content.ReadAsStringAsync();
                }

                return Ok(responseData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("GetMovieJson")]
        public async Task<IActionResult> GetMovieJson()
        {
            HttpClient httpClient = new HttpClient();
            var response = await httpClient.GetAsync("http://localhost:4000/movies");

            var jsonString = await response.Content.ReadAsStringAsync();

            return Ok(jsonString);
            
        }

        [HttpDelete]
        [Route("DeleteMovie")]
        public async Task<IActionResult> DeleteMovie([FromBody] MovieData data)
        {
            try
            {
                
                if (string.IsNullOrEmpty(data.Name))
                {
                    return BadRequest("Invalid name");
                }
                else
                {
                    string apiUrl = "http://localhost:4000/movies/";

                    using var httpClient = new HttpClient();

                    var jsonRequestBody = JsonConvert.SerializeObject(data);
                    var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Delete,
                        RequestUri = new Uri(apiUrl),
                        Content = content
                    };

                    HttpResponseMessage response = await httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok();
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return NotFound("Movie not found");
                    }
                    else
                    {
                        return BadRequest("Failed to delete movie");
                    }
                }
                
               
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        public class MovieData
        {
            public string Name { get; set; }
        }

        [HttpPost]
        [Route("PostRdfServer")]
        public async Task<IActionResult> PostRdfServer([FromBody] FrontEndDataModel frontEndData)
        {
            try
            {
                HttpClient httpclient = new HttpClient();

                string rdfData = GenerateRDFData(frontEndData);

                string rdfServerUrl = "http://localhost:8080/rdf4j-server/repositories/grafexamen/statements";
                
                var content = new StringContent(rdfData, Encoding.UTF8, "application/sparql-update");

                string requestUrl = $"{rdfServerUrl}?update=";

                HttpResponseMessage response = await httpclient.PostAsync(requestUrl, content);

                return Ok(response);


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        private string GenerateRDFData(FrontEndDataModel frontEndData)
        {
            
            string rdfData = $"prefix : <http://ioana.ro#> INSERT DATA {{ graph :grafNou {{ :{frontEndData.Subject} :{frontEndData.Predicate} :{frontEndData.Object} }} }}";

            return rdfData;
        }

        public class FrontEndDataModel
        {
            public string Subject { get; set; }
            public string Predicate { get; set; }
            public string Object { get; set; }
        }

        [HttpGet]
        [Route("GetRdfServer")]
        public async Task<IActionResult> GetRdfServer()
        {
            try
            {
                 HttpClient httpClient = new HttpClient();

                 string rdfServerUrl = "http://localhost:8080/rdf4j-server/repositories/grafexamen";
                 string query = "SELECT ?subject ?predicate ?object WHERE { ?subject ?predicate ?object }";

                 string requestUrl = $"{rdfServerUrl}?query={Uri.EscapeDataString(query)}";

                 HttpResponseMessage response = await httpClient.GetAsync(requestUrl);

                 var responseData = await response.Content.ReadAsStringAsync();

                 return Ok(responseData);  

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        

    }

 
}
