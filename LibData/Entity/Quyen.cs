using System;
using System.Collections.Generic;

namespace LibData.Entity;

public partial class Quyen
{
    public string Maquyen { get; set; } = null!;

    public string Tenquyen { get; set; } = null!;

    public virtual ICollection<Nguoidung> Nguoidungs { get; set; } = new List<Nguoidung>();
}
