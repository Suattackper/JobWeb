using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class JobSeekerNotification
{
    public string Id { get; set; }

    public string? NotifyTypeId { get; set; }

    public Guid? UserLoginDataId { get; set; }

    public Guid? JobId { get; set; }

    public DateTime? IsCreatedAt { get; set; }

    public bool? IsSent { get; set; }

    public bool? IsSeen { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual JobSeekerJobPosting? Job { get; set; }

    public virtual JobSeekerNotificationType? NotifyType { get; set; }

    public virtual JobSeekerUserLoginDatum? UserLoginData { get; set; }
}
