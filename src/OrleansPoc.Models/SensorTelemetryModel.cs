using System;
using System.Text.Json.Serialization;

namespace OrleansPoc.Models
{
	public class SensorTelemetryModel
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("timestamp")]
		public DateTimeOffset Timestamp { get; set; }

		[JsonPropertyName("measurement")]
		public int Measurement { get; set; }
	}
}
