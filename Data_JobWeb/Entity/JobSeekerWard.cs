using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class JobSeekerWard
{
    public string Code { get; set; }

    public string? WardName { get; set; }

    public string? DistrictCode { get; set; }

    public virtual JobSeekerDistrict? DistrictCodeNavigation { get; set; }
}
