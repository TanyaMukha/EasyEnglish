namespace EasyEnglish.Core.Interfaces;

public interface IReviewInfo
{
    DateTime? LastReviewDate { get; set; }

    int ReviewCount { get; set; }
}
