using EasyEnglish.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyEnglish.Core.Interfaces.Fields;
using MukhaLab.Database;

namespace EasyEnglish.Core.Models;

public class StudyCardModel : AbstractModel, IReviewInfo, IRateInfo, IAuditInfo
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Dialogue { get; set; }

    public DateTime? LastReviewDate { get; set; }

    public int ReviewCount { get; set; } = 0;

    public float Rate { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public int UnitId { get; set; }

    public UnitEntity? Unit { get; set; }
}
