using Dapr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DaprSamples
{
    public class SampleController : ControllerBase
    {
        private readonly ILogger logger;

        public SampleController(ILoggerFactory logger)
        {
            this.logger = logger.CreateLogger("SAMPLE");
        }

        [HttpGet("test")]
        public ActionResult Get()
        {
            this.logger.LogInformation("GET!!");
            return Ok();
        }

        [HttpPost("test")]
        public ActionResult TestPost()
        {
            this.logger.LogInformation("POST!!");
            return Ok();
        }

        [Topic("sample")]
        [HttpPost("sample")]
        public ActionResult Post()
        {
            this.logger.LogInformation("got event");
            return Ok();
        }

    }
}
