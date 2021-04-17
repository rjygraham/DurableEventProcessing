using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;
using Orleans.Streams.Core;
using OrleansPoc.Abstractions;
using OrleansPoc.Processor.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrleansPoc.Processor.Grains
{
	[ImplicitStreamSubscription("my-namespace")]
	public class SensorTypeAProcessorGrain : Grain, ISensorTypeAProcessorGrain, IStreamSubscriptionObserver, IAsyncObserver<ISensorTelemetryEntity>
	{
		private readonly IPersistentState<SensorTypeAState> persistentState;
		private readonly ILogger logger;

		private int previousMeasurement;
		public SensorTypeAProcessorGrain
		(
			[PersistentState(stateName: "Measurements", storageName: nameof(SensorTypeAProcessorGrain))] IPersistentState<SensorTypeAState> persistentState,
			ILogger<SensorTypeAProcessorGrain> logger
		)
		{
			this.persistentState = persistentState;
			this.logger = logger;
		}

		public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
		{
			// Plug our LoggerObserver to the stream
			var handle = handleFactory.Create<ISensorTelemetryEntity>();
			await handle.ResumeAsync(this);
		}

		public Task OnCompletedAsync()
		{
			logger.LogInformation($"Complete. Id: {GrainReference.GrainIdentity.PrimaryKey}");
			return Task.CompletedTask;
		}

		public Task OnErrorAsync(Exception ex)
		{
			logger.LogInformation($"Exception. Id: {GrainReference.GrainIdentity.PrimaryKey}, Ex: {ex.Message}");
			return Task.CompletedTask;
		}

		public async Task OnNextAsync(ISensorTelemetryEntity item, StreamSequenceToken token = null)
		{
			persistentState.State.Id = Guid.NewGuid().ToString();
			persistentState.State.Timestamp = item.Timestamp;
			persistentState.State.CurrentMeasurement = item.Measurement;

			// Don't process if current measurement is already in state or if current measurement happened prior to most recent measurement.
			if (persistentState.State.Measurements.ContainsKey(item.Timestamp) || (persistentState.State.Measurements.Count > 0 && item.Timestamp < persistentState.State.Measurements.Keys.Max()))
			{
				return;
			}

			persistentState.State.Measurements.Add(item.Timestamp, item.Measurement);

			var oldMeasurements = persistentState.State.Measurements.Keys.Where(w => w < DateTime.UtcNow.AddMinutes(-3));
			logger.LogInformation($"{IdentityString}: Total measurements: {persistentState.State.Measurements.Count}, Stale measurements to be removed: {oldMeasurements.Count()}");
			foreach (var oldMeasurement in oldMeasurements)
			{
				persistentState.State.Measurements.Remove(oldMeasurement);
			}


			if (persistentState.State.Measurements.Count > 1)
			{
				previousMeasurement = persistentState.State.Measurements.OrderByDescending(o => o.Key).Skip(1).FirstOrDefault().Value;
				logger.LogInformation($"{IdentityString}: Previous Measurement {previousMeasurement}");
			}

			if (item.Measurement < 100 || item.Measurement > 900)
			{
				logger.LogError($"{IdentityString}: Measurement {item.Measurement} exceeds minimum or maximum values.");
				return;
			}

			if (persistentState.State.Measurements.Values.Count > 0)
			{
				var average = persistentState.State.Measurements.Values.Average();

				if (average < 300 || average > 700)
				{
					logger.LogError($"{IdentityString}: Measurement {item.Measurement} exceeds minimum or maximum average value.");
					return;
				}
			}

			logger.LogInformation($"{IdentityString}: Measurement {item.Measurement} within acceptable ranges.");

			await persistentState.WriteStateAsync();
		}
	}
}
