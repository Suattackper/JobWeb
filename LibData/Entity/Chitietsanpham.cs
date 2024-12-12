using System;
using System.Collections.Generic;

namespace LibData.Entity;

public partial class Chitietsanpham
{
    public int Mahoadon { get; set; }

    public string Masanpham { get; set; } = null!;

    public int Soluong { get; set; }

    public virtual Hoadon MahoadonNavigation { get; set; } = null!;

    public virtual Sanpham MasanphamNavigation { get; set; } = null!;
}
