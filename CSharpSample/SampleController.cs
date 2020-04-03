using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace DaprSamples
{
    public class SampleController : ControllerBase
    {
        private readonly ILogger logger;
        public const string StoreName = "statestore";

        public SampleController(ILoggerFactory logger)
        {
            this.logger = logger.CreateLogger("SAMPLE");
        }

        [HttpGet("test")]
        public async Task<ActionResult> GetAsync([FromServices] DaprClient daprClient)
        {
            var r = await daprClient.GetStateAsync<int>(StoreName, "key");
            this.logger.LogInformation($"GET!! ({r})");
            return Ok();
        }

        [HttpPost("test")]
        public async Task<ActionResult> TestPostAsync([FromServices] DaprClient daprClient)
        {
            this.logger.LogInformation("POST!!");
            var r = await daprClient.GetStateEntryAsync<int>(StoreName, "key");
            r.Value++;
            await r.SaveAsync();
            return Ok();
        }

        [Topic("sample")]
        [HttpPost("sample")]
        public ActionResult Post()
        {
            this.logger.LogInformation("C# got event (pub/sub");
            return Ok();
        }

        [HttpPost("hub")]
        public async Task<ActionResult> PostHubAsync()
        {
            this.logger.LogInformation("C# got event (binding)");
            using (var reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                this.logger.LogInformation(body);
            }
            return Ok();
        }

    }
}
