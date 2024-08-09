using basetemplate_csharp.Data;
using Microsoft.AspNetCore.Mvc;

namespace basetemplate_csharp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HealthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                // Verificar la conexión a la base de datos
                _context.Database.CanConnect();

                return Ok(new { Status = "Healthy", Message = "API is up and running", Timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Status = "Unhealthy", Message = $"API is experiencing issues: {ex.Message}", Timestamp = DateTime.UtcNow });
            }
        }
    }
}