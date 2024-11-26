using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class JobSeekerRecruiterProfile
{
    public Guid RecruiterId { get; set; }

    public string? Fullname { get; set; }
    public string? Gender { get; set; }
    public string? Email { get; set; }

    public string? PhoneNumb { get; set; }

    public string? AvatarLink { get; set; }

    public string? LinkedinUrl { get; set; }

    public Guid? EnterpriseId { get; set; }

    public int? RoleId { get; set; }

    public DateTime? IsCreatedAt { get; set; }

    public DateTime? IsUpdatedAt { get; set; }

    public virtual JobSeekerEnterprise? Enterprise { get; set; }

    public virtual JobSeekerUserLoginDatum? Recruiter { get; set; }

    public virtual AuthenticationRole? Role { get; set; }
}
