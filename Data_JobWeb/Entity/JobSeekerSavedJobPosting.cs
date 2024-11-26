using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class JobSeekerSavedJobPosting
{
    public int Id { get; set; }

    public Guid? CandidateId { get; set; }

    public Guid? JobPostingId { get; set; }

    public DateTime? SavedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual JobSeekerCandidateProfile? Candidate { get; set; }

    public virtual JobSeekerJobPosting? JobPosting { get; set; }
}
