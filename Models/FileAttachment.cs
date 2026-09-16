using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace studyplanner.Models;

public class FileAttachment
{
    public int Id { get; set; }

    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string StoredFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public int StudyTaskId { get; set; }

    [JsonIgnore]
    public StudyTask? StudyTask { get; set; }
}
