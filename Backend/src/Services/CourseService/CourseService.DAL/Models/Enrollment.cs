using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CourseService.DAL.Models;

public partial class Enrollment
{
    [Key]
    public int Id { get; set; }

    public int CourseId { get; set; }

    [StringLength(100)]
    public string UserId { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime EnrolledAt { get; set; }

    [ForeignKey("CourseId")]
    [InverseProperty("Enrollments")]
    public virtual Course Course { get; set; } = null!;
}
