using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class JobSeekerUserLoginDataExternal
{
    public Guid Id { get; set; }

    public string? ProviderName { get; set; }

    public string? ExternalProviderToken { get; set; }

    public string? WsEndpoint { get; set; }

    public string? ExtraData { get; set; }

    public DateTime? IsCreatedAt { get; set; }

    public DateTime? IsUpdatedAt { get; set; }

    public virtual JobSeekerCandidateProfile? JobSeekerCandidateProfile { get; set; }
}
