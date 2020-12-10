using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace HealthMonitor.WebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly HttpClient _client;

        public MessageController(HttpClient client)
        {
            this._client = client;
        }

        [HttpPost]
        public async Task HttpPost(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new ArgumentNullException("token", nameof(token));
                }

                using (var reader = new StreamReader(Request.Body))
                {
                    var requestContent = await reader.ReadToEndAsync();
                    var url = $"**";
                    var text = HttpUtility.UrlEncode(requestContent);

                    await _client.GetAsync(url + text);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
