using System;
using System.Collections.Generic;

namespace LibData.Entity;

public partial class Hoadon
{
    public int Mahoadon { get; set; }

    public string Manguoidung { get; set; } = null!;

    public string Maban { get; set; } = null!;

    public string? Makhuyenmai { get; set; }

    public string? Yeucauthem { get; set; }

    public DateOnly Ngaytao { get; set; }

    public virtual ICollection<Chitietsanpham> Chitietsanphams { get; set; } = new List<Chitietsanpham>();

    public virtual Ban MabanNavigation { get; set; } = null!;

    public virtual Khuyenmai? MakhuyenmaiNavigation { get; set; }

    public virtual Nguoidung ManguoidungNavigation { get; set; } = null!;
}
