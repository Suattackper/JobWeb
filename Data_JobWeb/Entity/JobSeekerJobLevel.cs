using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class JobSeekerJobLevel
{
    public int Id { get; set; }

    public string? JobLevelName { get; set; }

    public virtual ICollection<JobSeekerJobPosting> JobSeekerJobPostings { get; set; } = new List<JobSeekerJobPosting>();
}
