using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class JobSeekerProvince
{
    public string Code { get; set; }

    public string? ProvinceName { get; set; }

    public virtual ICollection<JobSeekerDistrict> JobSeekerDistricts { get; set; } = new List<JobSeekerDistrict>();
}
