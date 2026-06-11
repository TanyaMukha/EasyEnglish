namespace EasyEnglish.Core.Models;

public class UnitCardModel
{
    public int Id { get; set; }
    public Guid RecordGuid { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int EasyCount { get; set; }
    public int MediumCount { get; set; }
    public int HardCount { get; set; }
    public int TotalCount { get; set; }
}
