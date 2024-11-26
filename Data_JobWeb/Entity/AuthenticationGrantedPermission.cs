using System;
using System.Collections.Generic;

namespace Data_JobWeb.Entity;

public partial class AuthenticationGrantedPermission
{
    public int Id { get; set; }

    public int? RoleId { get; set; }

    public int? PermissionId { get; set; }

    public virtual AuthenticationPermission? Permission { get; set; }

    public virtual AuthenticationRole? Role { get; set; }
}
