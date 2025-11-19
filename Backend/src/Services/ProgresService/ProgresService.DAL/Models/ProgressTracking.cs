using System;
using System.Collections.Generic;

namespace ProgresService.DAL.Models;

public partial class ProgressTracking
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public int CourseId { get; set; }

    public int? ModuleId { get; set; }

    public decimal ProgressPercent { get; set; }

    public DateTime? LastUpdated { get; set; }
}
