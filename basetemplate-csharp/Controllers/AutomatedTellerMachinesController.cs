using basetemplate_csharp.Data;
using basetemplate_csharp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace basetemplate_csharp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutomatedTellerMachinesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AutomatedTellerMachinesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/AutomatedTellerMachines
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AutomatedTellerMachine>>> GetAutomatedTellerMachines()
        {
            return await _context.AutomatedTellerMachines.ToListAsync();
        }

        // GET: api/AutomatedTellerMachines/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AutomatedTellerMachine>> GetAutomatedTellerMachine(long id)
        {
            var atm = await _context.AutomatedTellerMachines.FindAsync(id);

            if (atm == null)
            {
                return NotFound();
            }

            return atm;
        }

        // POST: api/AutomatedTellerMachines
        [HttpPost]
        public async Task<ActionResult<AutomatedTellerMachine>> PostAutomatedTellerMachine(AutomatedTellerMachine atm)
        {
            _context.AutomatedTellerMachines.Add(atm);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAutomatedTellerMachine", new { id = atm.Id }, atm);
        }

        // PUT: api/AutomatedTellerMachines/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAutomatedTellerMachine(long id, AutomatedTellerMachine atm)
        {
            atm.Id = id;

            _context.Entry(atm).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AutomatedTellerMachineExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/AutomatedTellerMachines/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAutomatedTellerMachine(long id)
        {
            var atm = await _context.AutomatedTellerMachines.FindAsync(id);
            if (atm == null)
            {
                return NotFound();
            }

            _context.AutomatedTellerMachines.Remove(atm);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AutomatedTellerMachineExists(long id)
        {
            return _context.AutomatedTellerMachines.Any(e => e.Id == id);
        }
    }
}