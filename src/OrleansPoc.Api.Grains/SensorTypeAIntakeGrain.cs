using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Streams;
using OrleansPoc.Abstractions;
using OrleansPoc.Api.Interfaces;
using System;
using System.Threading.Tasks;

namespace OrleansPoc.Api.Grains
{
	public class SensorTypeAIntakeGrain : Grain, ISensorTypeAIntakeGrain
	{
		private readonly ILogger logger;

		private IAsyncStream<ISensorTelemetryEntity> stream;

		public SensorTypeAIntakeGrain(ILogger<SensorTypeAIntakeGrain> logger)
		{
			this.logger = logger;
		}

		public override Task OnActivateAsync()
		{
			this.stream = GetStreamProvider("my-stream-provider").GetStream<ISensorTelemetryEntity>(GrainReference.GrainIdentity.PrimaryKey, "my-namespace");
			return Task.CompletedTask;
		}

		public async Task<bool> RecordMeasurementAsync(ISensorTelemetryEntity entity)
		{
			try
			{
				await stream.OnNextAsync(entity);
			}
			catch (Exception ex)
			{
				logger.LogError($"Error processing message: {ex.Message}");
				return false;
			}

			return true;
		}
	}
}
