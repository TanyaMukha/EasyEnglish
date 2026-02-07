namespace EasyEnglish.Core.Models;

public class GrammarTestResultModel
{
    public int TestId { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public int IncorrectAnswers { get; set; }
    public double Percentage { get; set; }
    public DateTime CompletedAt { get; set; }
    public Dictionary<int, string> Answers { get; set; } = new();

    public string Grade => Percentage switch
    {
        >= 90 => "Відмінно",
        >= 80 => "Добре",
        >= 70 => "Задовільно",
        >= 60 => "Достатньо",
        _ => "Незадовільно"
    };
}