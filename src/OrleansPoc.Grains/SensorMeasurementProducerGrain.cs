using Orleans;
using Orleans.Streams;
using OrleansPoc.Interfaces;
using System;
using System.Threading.Tasks;

namespace OrleansPoc.Grains
{
	public class SensorMeasurementProducerGrain : Grain, ISensorMeasurementProducer
	{
		private IAsyncStream<int> stream;

		public async Task<bool> RecordMeasurementAsync(int measurement)
		{
			await stream.OnNextAsync(measurement);
			return true;
		}

		public override Task OnActivateAsync()
		{
			// Get the stream
			this.stream = GetStreamProvider("my-stream-provider")
				.GetStream<int>(GrainReference.GrainIdentity.PrimaryKey, "my-namespace");

			return Task.CompletedTask;
		}
	}
}
