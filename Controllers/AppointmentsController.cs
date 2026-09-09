using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ozankaya_api.Data;
using ozankaya_api.Hubs;
using ozankaya_api.Models;

namespace ozankaya_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<AppointmentHub> _hubContext;

        public AppointmentsController(AppDbContext context, IHubContext<AppointmentHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments()
        {
            return await _context.Appointments
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.Time)
                .ToListAsync();
        }

        [HttpGet("slots")]
        public async Task<ActionResult<IEnumerable<string>>> GetDisabledSlots(int barberId, string date)
        {
            var bookedSlots = await _context.Appointments
                .Where(a => a.BarberId == barberId && a.Date == date)
                .Select(a => a.Time)
                .ToListAsync();

            var manuallyBlocked = await _context.BlockedSlots
                .Where(b => b.BarberId == barberId && b.Date == date)
                .Select(b => b.Time)
                .ToListAsync();

            return Ok(bookedSlots.Union(manuallyBlocked).ToList());
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("SlotUpdated", new
            {
                barberId = appointment.BarberId,
                date = appointment.Date,
                time = appointment.Time,
                isBooked = true
            });

            return Ok(appointment);
        }

        [HttpPost("{id}/accept")]
        public async Task<IActionResult> AcceptAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.IsAccepted = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Randevu başarıyla kabul edildi." });
        }

        [HttpPost("toggle-slot")]
        public async Task<IActionResult> ToggleSlot([FromBody] BlockedSlot slot)
        {
            var existing = await _context.BlockedSlots
                .FirstOrDefaultAsync(b => b.BarberId == slot.BarberId && b.Date == slot.Date && b.Time == slot.Time);

            bool isBooked;
            if (existing != null)
            {
                _context.BlockedSlots.Remove(existing);
                isBooked = false;
            }
            else
            {
                _context.BlockedSlots.Add(slot);
                isBooked = true;
            }

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("SlotUpdated", new
            {
                barberId = slot.BarberId,
                date = slot.Date,
                time = slot.Time,
                isBooked = isBooked
            });

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("SlotUpdated", new
            {
                barberId = appointment.BarberId,
                date = appointment.Date,
                time = appointment.Time,
                isBooked = false
            });

            return Ok();
        }
    }
}