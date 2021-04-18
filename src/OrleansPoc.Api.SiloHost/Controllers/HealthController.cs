using Microsoft.AspNetCore.Mvc;

namespace OrleansPoc.Api.SiloHost.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class HealthController: ControllerBase
	{
		[HttpGet]
		public IActionResult Get()
		{
			return Ok();
		}
	}
}
