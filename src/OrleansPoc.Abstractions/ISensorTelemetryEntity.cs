using System;

namespace OrleansPoc.Abstractions
{
	public interface ISensorTelemetryEntity
	{
		string Id { get; set; }
		DateTimeOffset Timestamp { get; set; }
		int Measurement { get; set; }
	}
}
