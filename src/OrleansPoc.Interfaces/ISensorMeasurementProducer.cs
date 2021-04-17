using Orleans;
using System.Threading.Tasks;

namespace OrleansPoc.Interfaces
{
	public interface ISensorMeasurementProducer: IGrainWithGuidKey
	{
		Task<bool> RecordMeasurementAsync(int measurement);
	}
}
