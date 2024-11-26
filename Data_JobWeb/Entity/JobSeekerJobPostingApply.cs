using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class JobSeekerJobPostingApply
{
    public int Id { get; set; }

    public Guid? JobPostingId { get; set; }

    public Guid? CandidateId { get; set; }

    public DateTime? ApplyTime { get; set; }

    public string? StatusCode { get; set; }

    public virtual JobSeekerCandidateProfile? Candidate { get; set; }

    public virtual JobSeekerJobPosting? JobPosting { get; set; }

    public virtual JobSeekerStatusCode? StatusCodeNavigation { get; set; }
}
