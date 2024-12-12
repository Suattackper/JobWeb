using System;
using System.Collections.Generic;

namespace LibData.Entity;

public partial class Nguoidung
{
    public string Manguoidung { get; set; } = null!;

    public string Matkhau { get; set; } = null!;

    public string Hovaten { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Maquyen { get; set; } = null!;

    public DateOnly Ngaytao { get; set; }

    public string? Maxacnhan { get; set; }

    public virtual ICollection<Hoadon> Hoadons { get; set; } = new List<Hoadon>();

    public virtual Quyen MaquyenNavigation { get; set; } = null!;
}
