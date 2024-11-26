using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class JobSeekerJobCategory
{
    public int Id { get; set; }

    public string? JobCategoryName { get; set; }

    public string? AppIconName { get; set; }

    public DateTime? IsCreatedAt { get; set; }

    public DateTime? IsUpdatedAt { get; set; }

    public virtual ICollection<JobSeekerJobPosting> JobSeekerJobPostings { get; set; } = new List<JobSeekerJobPosting>();
}
