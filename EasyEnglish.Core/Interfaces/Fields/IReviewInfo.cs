namespace EasyEnglish.Core.Interfaces.Fields;

public interface IReviewInfo
{
    DateTime? LastReviewDate { get; set; }

    int ReviewCount { get; set; }
}
