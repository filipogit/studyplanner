using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using studyplanner.Data;
using studyplanner.Models;

namespace studyplanner.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _context;

    public TasksController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StudyTask>>> GetTasks()
    {
        return await _context.Tasks.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StudyTask>> GetTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
            return NotFound();

        return task;
    }

    [HttpPost]
    public async Task<ActionResult<StudyTask>> CreateTask(StudyTask task)
    {
        task.CreatedAt = DateTime.UtcNow;

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }
}
