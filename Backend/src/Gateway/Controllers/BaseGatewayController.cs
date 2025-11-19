using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers
{
    [ApiController]
    public abstract class BaseGatewayController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        protected BaseGatewayController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected async Task<IActionResult> ForwardGet(string url)
        {
            var response = await _httpClient.GetAsync(url);
            return await FormatResponse(response);
        }

        protected async Task<IActionResult> ForwardPost(string url, object body)
        {
            var response = await _httpClient.PostAsJsonAsync(url, body);
            return await FormatResponse(response);
        }

        protected async Task<IActionResult> ForwardPut(string url, object body)
        {
            var response = await _httpClient.PutAsJsonAsync(url, body);
            return await FormatResponse(response);
        }

        protected async Task<IActionResult> ForwardDelete(string url)
        {
            var response = await _httpClient.DeleteAsync(url);
            return await FormatResponse(response);
        }

        private async Task<IActionResult> FormatResponse(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, content);

            return Content(content, "application/json");
        }
    }
}
