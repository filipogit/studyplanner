using Microsoft.EntityFrameworkCore;
using studyplanner.Models;

namespace studyplanner.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<StudyTask> Tasks => Set<StudyTask>();
}
