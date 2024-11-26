using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class JobSeekerEducationDetail
{
    public Guid EducationId { get; set; }

    public string? SchoolName { get; set; }

    public string? Major { get; set; }

    public string? Degree { get; set; }

    public string? Description { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public Guid? CandidateId { get; set; }

    public DateTime? IsCreatedAt { get; set; }

    public DateTime? IsUpdatedAt { get; set; }

    public DateTime? IsDeletedAt { get; set; }

    public virtual JobSeekerCandidateProfile? Candidate { get; set; }
}
