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
    private readonly IWebHostEnvironment _env;

    public TasksController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StudyTask>>> GetTasks()
    {
        return await _context.Tasks.Include(t => t.Attachments).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StudyTask>> GetTask(int id)
    {
        var task = await _context.Tasks.Include(t => t.Attachments).FirstOrDefaultAsync(t => t.Id == id);

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

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, StudyTask task)
    {
        if (id != task.Id)
            return BadRequest();

        var existingTask = await _context.Tasks.FindAsync(id);

        if (existingTask == null)
            return NotFound();

        existingTask.Title = task.Title;
        existingTask.Description = task.Description;
        existingTask.IsCompleted = task.IsCompleted;
        existingTask.DueDate = task.DueDate;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/upload")]
    public async Task<ActionResult<FileAttachment>> UploadFile(int id, IFormFile file)
    {
        var task = await _context.Tasks.FindAsync(id);

        if (task == null)
            return NotFound();

        if (file.Length == 0)
            return BadRequest(new { message = "File is empty" });

        var uploadsPath = Path.Combine(_env.ContentRootPath, "Uploads");
        Directory.CreateDirectory(uploadsPath);

        var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsPath, storedFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var attachment = new FileAttachment
        {
            FileName = file.FileName,
            StoredFileName = storedFileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            UploadedAt = DateTime.UtcNow,
            StudyTaskId = id
        };

        _context.FileAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, attachment);
    }

    [HttpGet("{taskId}/files/{fileId}")]
    public async Task<IActionResult> GetFile(int taskId, int fileId)
    {
        var attachment = await _context.FileAttachments
            .FirstOrDefaultAsync(f => f.Id == fileId && f.StudyTaskId == taskId);

        if (attachment == null)
            return NotFound();

        var filePath = Path.Combine(_env.ContentRootPath, "Uploads", attachment.StoredFileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound(new { message = "File not found on disk" });

        return PhysicalFile(filePath, attachment.ContentType, attachment.FileName);
    }
}
