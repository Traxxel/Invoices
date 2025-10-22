namespace Invoice.Application.Interfaces;

public interface IDatabaseSeeder
{
    Task<bool> SeedAsync();
    Task<bool> IsSeededAsync();
    Task<bool> ClearSeedDataAsync();
    Task<SeedResult> GetSeedStatusAsync();
}

public class SeedResult
{
    public bool IsSeeded { get; set; }
    public int SeedCount { get; set; }
    public DateTime? LastSeeded { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

