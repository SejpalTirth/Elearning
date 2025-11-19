using System;
using System.Collections.Generic;

namespace NotificationService.DAL.Models;

public partial class NotificationTemplate
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string BodyTemplate { get; set; } = null!;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
