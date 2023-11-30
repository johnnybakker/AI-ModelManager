using HillsModelManager.Models;
using Microsoft.EntityFrameworkCore;

namespace HillsModelManager.Database;

public class TrainDBContext : DbContext
{
    public DbSet<TrainDataset> TrainDatasets { get; set; }

    public string DbPath { get; }

    public TrainDBContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
		Console.WriteLine(path);
        DbPath = Path.Join(path, $"{nameof(HillsModelManager)}.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}