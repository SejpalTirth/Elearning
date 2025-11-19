using System;
using System.Collections.Generic;

namespace ProgresService.DAL.Models;

public partial class CourseCompletion
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public int CourseId { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? CertificateUrl { get; set; }
}
