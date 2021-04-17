using Orleans;
using OrleansPoc.Abstractions;
using System.Threading.Tasks;

namespace OrleansPoc.Api.Interfaces
{
	public interface ISensorTypeAIntakeGrain: IGrainWithGuidKey
	{
		Task<bool> RecordMeasurementAsync(ISensorTelemetryEntity entity);
	}
}
