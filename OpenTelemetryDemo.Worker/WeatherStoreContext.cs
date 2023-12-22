using Microsoft.EntityFrameworkCore;
using OpenTelemetryDemo.Database.Entities;

namespace OpenTelemetryDemo.Worker;

public class WeatherStoreContext : DbContext
{
    public WeatherStoreContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        string path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "weatherstore.db");
    }

    public DbSet<WeatherEntity> WeatherStore { get; set; }
    public string DbPath { get; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}");
    }
}