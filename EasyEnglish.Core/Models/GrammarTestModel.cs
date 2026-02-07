using EasyEnglish.Core.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EasyEnglish.Core.Models;

public class GrammarTestModel
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? LastReviewDate { get; set; }

    public int ReviewCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public virtual List<TestCardEntity> TestCards { get; set; } = new();
}
