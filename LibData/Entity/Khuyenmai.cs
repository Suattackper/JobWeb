using System;
using System.Collections.Generic;

namespace LibData.Entity;

public partial class Khuyenmai
{
    public string Makhuyenmai { get; set; } = null!;

    public string Tenkhuyenmai { get; set; } = null!;

    public int Soluong { get; set; }

    public int Dieukien { get; set; }

    public int Giam { get; set; }

    public DateOnly Ngaybatdau { get; set; }

    public DateOnly Ngayketthuc { get; set; }

    public virtual ICollection<Hoadon> Hoadons { get; set; } = new List<Hoadon>();
}
