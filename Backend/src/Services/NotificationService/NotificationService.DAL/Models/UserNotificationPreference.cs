using System;
using System.Collections.Generic;

namespace NotificationService.DAL.Models;

public partial class UserNotificationPreference
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public string NotificationType { get; set; } = null!;

    public bool IsEnabled { get; set; }
}
