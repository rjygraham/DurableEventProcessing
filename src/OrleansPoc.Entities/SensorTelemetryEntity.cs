using OrleansPoc.Abstractions;
using System;
using System.Text.Json.Serialization;

namespace OrleansPoc.Entities
{
	public class SensorTelemetryEntity: ISensorTelemetryEntity
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("timestamp")]
		public DateTimeOffset Timestamp { get; set; }

		[JsonPropertyName("measurement")]
		public int Measurement { get; set; }
	}
}
