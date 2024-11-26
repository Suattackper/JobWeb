using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class JobSeekerJobField
{
    public int JobFieldId { get; set; }

    public string? JobFieldName { get; set; }

    public virtual ICollection<JobSeekerEnterprise> JobSeekerEnterprises { get; set; } = new List<JobSeekerEnterprise>();
}
