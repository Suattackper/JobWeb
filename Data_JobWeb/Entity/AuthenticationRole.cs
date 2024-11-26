using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class AuthenticationRole
{
    public int RoleId { get; set; }

    public string? RoleName { get; set; }

    public virtual ICollection<AuthenticationGrantedPermission> AuthenticationGrantedPermissions { get; set; } = new List<AuthenticationGrantedPermission>();

    public virtual ICollection<JobSeekerCandidateProfile> JobSeekerCandidateProfiles { get; set; } = new List<JobSeekerCandidateProfile>();

    public virtual ICollection<JobSeekerRecruiterProfile> JobSeekerRecruiterProfiles { get; set; } = new List<JobSeekerRecruiterProfile>();
}
