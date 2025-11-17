using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CourseService.DAL.Models;

public partial class Module
{
    [Key]
    public int Id { get; set; }

    [StringLength(250)]
    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public int CourseId { get; set; }

    [ForeignKey("CourseId")]
    [InverseProperty("Modules")]
    public virtual Course Course { get; set; } = null!;
}
