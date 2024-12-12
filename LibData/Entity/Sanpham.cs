using System;
using System.Collections.Generic;

namespace LibData.Entity;

public partial class Sanpham
{
    public string Masanpham { get; set; } = null!;

    public string Tensanpham { get; set; } = null!;

    public string Maloai { get; set; } = null!;

    public int Gia { get; set; }

    public string? Mota { get; set; }

    public byte[]? Anh { get; set; }

    public DateOnly Ngaythem { get; set; }

    public virtual ICollection<Chitietsanpham> Chitietsanphams { get; set; } = new List<Chitietsanpham>();

    public virtual Loai MaloaiNavigation { get; set; } = null!;
}
