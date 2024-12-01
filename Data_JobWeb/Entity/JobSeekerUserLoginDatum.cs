using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class JobSeekerUserLoginDatum
{
    public Guid Id { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public int? RoleId { get; set; }

    public bool? EmailVerified { get; set; }

    public string? StatusCode { get; set; }

    public DateTime? LastActiveTime { get; set; }

    public bool? IsActive { get; set; }
    public bool? IsDisable { get; set; }

    public DateTime? IsCreatedAt { get; set; }

    public DateTime? IsUpdatedAt { get; set; }
    public string? AvartarUrl { get; set; }

    public virtual JobSeekerCandidateProfile? JobSeekerCandidateProfile { get; set; }

    public virtual JobSeekerRecruiterProfile? JobSeekerRecruiterProfile { get; set; }

    public virtual JobSeekerStatusCode? StatusCodeNavigation { get; set; }
}
