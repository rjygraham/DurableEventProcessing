using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OrleansPoc.Processor.Grains
{
	public class SensorTypeAState
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("pk")]
		public string PartitionKey { get; set; }

		[JsonProperty("timestamp")]
		public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

		[JsonIgnore]
		public Dictionary<DateTimeOffset, int> Measurements { get; set; }

		[JsonProperty("measurement")]
		public int CurrentMeasurement { get; set; }

		public SensorTypeAState()
		{
		}

		public SensorTypeAState(Dictionary<DateTimeOffset, int> measurements)
		{
			Measurements = measurements;
		}
	}
}
