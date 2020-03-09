using Dapr;
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
        public async System.Threading.Tasks.Task<ActionResult> GetAsync([FromServices] StateClient stateClient)
        {
            var r = await stateClient.GetStateEntryAsync<int>("sample", "key");
            this.logger.LogInformation($"GET!! ({r})");
            return Ok();
        }

        [HttpPost("test")]
        public async System.Threading.Tasks.Task<ActionResult> TestPostAsync([FromServices] StateClient stateClient)
        {
            this.logger.LogInformation("POST!!");
            var r = await stateClient.GetStateEntryAsync<int>("sample", "key");
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
