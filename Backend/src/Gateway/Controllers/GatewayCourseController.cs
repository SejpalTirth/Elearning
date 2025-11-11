using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GatewayCourseController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public GatewayCourseController(HttpClient httpclient)
        {
            _httpClient = httpclient ?? throw new ArgumentNullException(nameof(httpclient));
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var url = $"http://localhost:5257/api/course";
            
            var response = await _httpClient.GetAsync(url);

            if(!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode,"Error calling the Courseservice");
            }

            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Getbyid(int id)
        {
            var url = $"http://localhost:5257/api/course/{id}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, "Error calling the Courseservice");
            }

            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
    }
}
