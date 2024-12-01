using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class JobSeekerNotificationType
{
    public string Id { get; set; }

    public string? TypeName { get; set; }

    public string? Description { get; set; }
    public Guid? IdUser { get; set; }
    public DateTime? IsCreateAt { get; set; }

}
