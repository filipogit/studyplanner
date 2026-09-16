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
}
