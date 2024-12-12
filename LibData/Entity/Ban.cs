using System;
using System.Collections.Generic;

namespace LibData.Entity;

public partial class Ban
{
    public string Maban { get; set; } = null!;

    public string Tenban { get; set; } = null!;

    public string Hello { get; set; } = null!;

    public virtual ICollection<Hoadon> Hoadons { get; set; } = new List<Hoadon>();
}
