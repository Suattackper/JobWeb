using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class JobSeekerDistrict
{
    public string Code { get; set; }

    public string? DistrictName { get; set; }

    public string? ProvinceCode { get; set; }

    public virtual ICollection<JobSeekerWard> JobSeekerWards { get; set; } = new List<JobSeekerWard>();

    public virtual JobSeekerProvince? ProvinceCodeNavigation { get; set; }
}
