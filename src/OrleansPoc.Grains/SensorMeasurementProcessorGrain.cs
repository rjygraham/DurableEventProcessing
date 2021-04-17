using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Streams;
using Orleans.Streams.Core;
using OrleansPoc.Interfaces;
using System;
using System.Threading.Tasks;

namespace OrleansPoc.Grains
{
	[ImplicitStreamSubscription("my-namespace")]
	public class SensorMeasurementProcessorGrain : Grain, ISensorMeasurementProcessor, IStreamSubscriptionObserver
	{
		private class LoggerObserver : IAsyncObserver<int>
		{
			private readonly Guid id;
			private readonly ILogger<ISensorMeasurementProcessor> logger;

			public LoggerObserver(Guid id, ILogger<ISensorMeasurementProcessor> logger)
			{
				this.id = id;
				this.logger = logger;
			}

			public Task OnCompletedAsync()
			{
				this.logger.LogInformation("OnCompletedAsync");
				return Task.CompletedTask;
			}

			public Task OnErrorAsync(Exception ex)
			{
				this.logger.LogInformation("OnErrorAsync: {Exception}", ex);
				return Task.CompletedTask;
			}

			public Task OnNextAsync(int item, StreamSequenceToken token = null)
			{
				logger.LogInformation($"Id: {id}, Value: {item}");
				return Task.CompletedTask;
			}
		}

		private readonly ILogger<SensorMeasurementProcessorGrain> logger;
		private LoggerObserver observer;

		public SensorMeasurementProcessorGrain(ILogger<SensorMeasurementProcessorGrain> logger)
		{
			this.logger = logger;
		}

		public override Task OnActivateAsync()
		{
			observer = new LoggerObserver(GrainReference.GrainIdentity.PrimaryKey, logger);
			return Task.CompletedTask;
		}

		public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
		{
			// Plug our LoggerObserver to the stream
			var handle = handleFactory.Create<int>();
			await handle.ResumeAsync(observer);
		}
	}
}
