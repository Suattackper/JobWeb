using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class JobSeekerNotification
{
    public string Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Type { get; set; }

    public Guid? IdUserReceive { get; set; }

    public Guid? IdConcern { get; set; }

    public DateTime? IsCreatedAt { get; set; }

    public bool? IsSent { get; set; }

    public bool? IsSeen { get; set; }
}
