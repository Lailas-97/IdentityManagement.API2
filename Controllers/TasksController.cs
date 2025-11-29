using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using IdentityManagement.API.Data;
using IdentityManagement.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Alias so we can still use Task for async methods
using TaskEntity = IdentityManagement.API.Models.TaskItem;

namespace IdentityManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper: get current user id from JWT
        private string GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new System.Exception("User id not found in token.");
            }
            return userId;
        }

        // GET: api/tasks
        [HttpGet]
        public async Task<IActionResult> GetMyTasks()
        {
            var userId = GetUserId();

            var tasks = await _context.TaskItems
                .Where(t => t.UserId == userId)
                .ToListAsync();

            return Ok(tasks);
        }

        public class CreateTaskRequest
        {
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
        }

        // POST: api/tasks
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
        {
            var userId = GetUserId();

            var task = new TaskEntity
            {
                Title = request.Title,
                Description = request.Description,
                IsCompleted = false,
                UserId = userId
            };

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
        }

        // GET: api/tasks/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var userId = GetUserId();

            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
                return NotFound();

            return Ok(task);
        }

        public class UpdateTaskRequest
        {
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public bool IsCompleted { get; set; }
        }

        // PUT: api/tasks/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskRequest request)
        {
            var userId = GetUserId();

            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
                return NotFound();

            task.Title = request.Title;
            task.Description = request.Description;
            task.IsCompleted = request.IsCompleted;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/tasks/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = GetUserId();

            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
                return NotFound();

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}