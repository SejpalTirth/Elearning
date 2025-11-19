using System;
using System.Collections.Generic;

namespace NotificationService.DAL.Models;

public partial class Notification
{
    public int Id { get; set; }

    public Guid? UserId { get; set; }

    public int? TemplateId { get; set; }

    public string Title { get; set; } = null!;

    public string? Body { get; set; }

    public bool? IsRead { get; set; }

    public DateTime? SentAt { get; set; }

    public virtual NotificationTemplate? Template { get; set; }
}
