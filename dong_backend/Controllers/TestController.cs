using Microsoft.AspNetCore.Mvc;

namespace dong_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello, this is a simple GET API");
        }
    }
}
