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

    // Hämta alla uppgifter med tillhörande filer
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

    // Skapa en ny uppgift
    [HttpPost]
    public async Task<ActionResult<StudyTask>> CreateTask(StudyTask task)
    {
        task.CreatedAt = DateTime.UtcNow;

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    // Uppdatera en befintlig uppgift
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

    // Ta bort en uppgift och dess filer från disk
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _context.Tasks.Include(t => t.Attachments).FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            return NotFound();

        if (task.Attachments != null)
        {
            foreach (var attachment in task.Attachments)
            {
                var filePath = Path.Combine(_env.ContentRootPath, "Uploads", attachment.StoredFileName);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Ladda upp en fil och koppla den till en uppgift
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

    // Hämta en uppladdad fil för nedladdning
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
