using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DaprSamples
{
    public class SampleController : Controller
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

        [HttpPost("pub")]
        public async Task<ActionResult> PubPostAsync([FromServices] DaprClient daprClient)
        {
            this.logger.LogInformation("PUB Start");

            foreach (var e in Enumerable.Range(0, 20))
            {
                await daprClient.PublishEventAsync("sample", e);
            }
            this.logger.LogInformation("PUB End");
            return Ok();
        }

        [HttpPost("bind")]
        public async Task<ActionResult> BindPostAsync([FromServices] DaprClient daprClient)
        {
            this.logger.LogInformation("BIND Start");

            foreach (var e in Enumerable.Range(0, 20))
            {
                await daprClient.InvokeBindingAsync("hub", e);
            }
            this.logger.LogInformation("BIND End");
            return Ok();
        }

        [Topic("sample")]
        [HttpPost("sample")]
        public async Task<ActionResult> PostAsync()
        {
            this.logger.LogInformation("C# got event (pub/sub");
            using (var reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                this.logger.LogInformation(body);
            }

            await Task.Delay(200);
            this.logger.LogInformation("C# done with event (pub/sub");
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
            this.logger.LogInformation("C# done event (binding)");
            return base.Json(new { Message = "hello" });
        }

    }
}
