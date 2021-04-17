using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrleansPoc.Http.Producer.Config;
using OrleansPoc.Models;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OrleansPoc.Http.Producer
{
	class ProducerHostedService : IHostedService
	{
		private readonly ProducerOptions producerOptions;
		private readonly ILogger logger;

		public ProducerHostedService(ProducerOptions producerOptions, ILogger<ProducerHostedService> logger)
		{
			this.producerOptions = producerOptions;
			this.logger = logger;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			var client = new HttpClient { BaseAddress = new Uri(producerOptions.ApiBaseAddress) };
			await Task.Factory.StartNew(async () =>
			{
				var random = new Random();
				var stopwatch = new Stopwatch();

				while (!cancellationToken.IsCancellationRequested)
				{
					stopwatch.Restart();
					var model = new SensorTelemetryModel
					{
						Id = producerOptions.SensorId,
						Timestamp = DateTimeOffset.UtcNow,
						Measurement = random.Next(0, 1000)
					};

					var content = JsonContent.Create(model);
					await client.PostAsync("/telemetry", content);
					stopwatch.Stop();

					logger.LogInformation($"Sensor ID: {model.Id} | Measurement: {model.Measurement} | {stopwatch.ElapsedMilliseconds}ms");

					await Task.Delay(producerOptions.MillisecondsDelay);
				}

			}, cancellationToken);
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			// Nothing to do here.
			return Task.CompletedTask;
		}
	}
}
