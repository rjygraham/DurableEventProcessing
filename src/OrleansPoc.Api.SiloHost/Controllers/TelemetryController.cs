using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using OrleansPoc.Api.Interfaces;
using OrleansPoc.Entities;
using OrleansPoc.Models;
using System;
using System.Threading.Tasks;

namespace OrleansPoc.Api.SiloHost.Controllers
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
			// Validate model.

			// Convert to entity.
			var entity = new SensorTelemetryEntity
			{
				Id = model.Id,
				Measurement = model.Measurement,
				Timestamp = model.Timestamp
			};

			// Get Grain and process event.
			var sensor = grainFactory.GetGrain<ISensorTypeAIntakeGrain>(Guid.Parse(model.Id));
			var result = await sensor.RecordMeasurementAsync(entity);

			return result 
				? Ok() 
				: StatusCode(500);
		}
	}
}
