using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using OrleansPoc.Interfaces;
using OrleansPoc.Models;
using System;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class TelemetryController : ControllerBase
	{
		private readonly IGrainFactory grainFactory;
		private readonly ILogger<TelemetryController> logger;

		public TelemetryController(IGrainFactory grainFactory, ILogger<TelemetryController> logger)
		{
			this.grainFactory = grainFactory;
			this.logger = logger;
		}

		[HttpPost]
		public async Task<IActionResult> Post([FromBody] SensorTelemetryModel model)
		{
			var sensor = grainFactory.GetGrain<ISensorMeasurementProducer>(Guid.Parse(model.Id));
			await sensor.RecordMeasurementAsync(model.Measurement);

			return Ok();
		}
	}
}
