using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class JobSeekerStatusCode
{
    public string Id { get; set; }

    public string? CodeValue { get; set; }

    public virtual ICollection<JobSeekerJobPostingApply> JobSeekerJobPostingApplies { get; set; } = new List<JobSeekerJobPostingApply>();

    public virtual ICollection<JobSeekerJobPosting> JobSeekerJobPostings { get; set; } = new List<JobSeekerJobPosting>();

    public virtual ICollection<JobSeekerUserLoginDatum> JobSeekerUserLoginData { get; set; } = new List<JobSeekerUserLoginDatum>();
}
