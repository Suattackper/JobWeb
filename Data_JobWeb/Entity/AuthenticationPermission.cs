using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class AuthenticationPermission
{
    public int PermissionId { get; set; }

    public string? PermissionDescri { get; set; }

    public virtual ICollection<AuthenticationGrantedPermission> AuthenticationGrantedPermissions { get; set; } = new List<AuthenticationGrantedPermission>();
}
