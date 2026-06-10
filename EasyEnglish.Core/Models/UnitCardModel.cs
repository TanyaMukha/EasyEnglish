namespace EasyEnglish.Core.Models;

public class UnitCardModel
{
    public int Id { get; set; }
    public Guid RecordGuid { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int EasyWordsCount { get; set; }
    public int MediumWordsCount { get; set; }
    public int HardWordsCount { get; set; }
    public int TotalWordsCount { get; set; }
}
