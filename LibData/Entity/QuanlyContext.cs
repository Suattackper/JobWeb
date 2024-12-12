using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LibData.Entity;

public partial class QuanlyContext : DbContext
{
    public QuanlyContext()
    {
    }

    public QuanlyContext(DbContextOptions<QuanlyContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Aa> Aas { get; set; }

    public virtual DbSet<Ban> Bans { get; set; }

    public virtual DbSet<Chitietsanpham> Chitietsanphams { get; set; }

    public virtual DbSet<Hoadon> Hoadons { get; set; }

    public virtual DbSet<Khuyenmai> Khuyenmais { get; set; }

    public virtual DbSet<Loai> Loais { get; set; }

    public virtual DbSet<Nguoidung> Nguoidungs { get; set; }

    public virtual DbSet<Quyen> Quyens { get; set; }

    public virtual DbSet<Sanpham> Sanphams { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=VVV\\ANHTAN;Initial Catalog=Quanly;Integrated Security=True;Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Aa>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("aa");

            entity.Property(e => e.Aa1)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("aa");
        });

        modelBuilder.Entity<Ban>(entity =>
        {
            entity.HasKey(e => e.Maban).HasName("PK__ban__0F4157A8D30F245B");

            entity.ToTable("ban");

            entity.Property(e => e.Maban)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CS_AS")
                .HasColumnName("maban");
            entity.Property(e => e.Hello)
                .HasDefaultValue("")
                .HasColumnName("hello");
            entity.Property(e => e.Tenban)
                .HasMaxLength(50)
                .HasColumnName("tenban");
        });

        modelBuilder.Entity<Chitietsanpham>(entity =>
        {
            entity.HasKey(e => new { e.Mahoadon, e.Masanpham }).HasName("PK__chitiets__6481AFBEDDD6BB02");

            entity.ToTable("chitietsanpham");

            entity.HasIndex(e => e.Masanpham, "IX_chitietsanpham_masanpham");

            entity.Property(e => e.Mahoadon).HasColumnName("mahoadon");
            entity.Property(e => e.Masanpham)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CS_AS")
                .HasColumnName("masanpham");
            entity.Property(e => e.Soluong)
                .HasDefaultValue(1)
                .HasColumnName("soluong");

            entity.HasOne(d => d.MahoadonNavigation).WithMany(p => p.Chitietsanphams)
                .HasForeignKey(d => d.Mahoadon)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__chitietsa__mahoa__7CA47C3F");

            entity.HasOne(d => d.MasanphamNavigation).WithMany(p => p.Chitietsanphams)
                .HasForeignKey(d => d.Masanpham)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__chitietsa__masan__7D98A078");
        });

        modelBuilder.Entity<Hoadon>(entity =>
        {
            entity.HasKey(e => e.Mahoadon);

            entity.ToTable("hoadon");

            entity.HasIndex(e => e.Maban, "IX_hoadon_maban");

            entity.HasIndex(e => e.Makhuyenmai, "IX_hoadon_makhuyenmai");

            entity.HasIndex(e => e.Manguoidung, "IX_hoadon_manguoidung");

            entity.Property(e => e.Mahoadon).HasColumnName("mahoadon");
            entity.Property(e => e.Maban)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CS_AS")
                .HasColumnName("maban");
            entity.Property(e => e.Makhuyenmai)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CS_AS")
                .HasColumnName("makhuyenmai");
            entity.Property(e => e.Manguoidung)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CS_AS")
                .HasColumnName("manguoidung");
            entity.Property(e => e.Ngaytao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("ngaytao");
            entity.Property(e => e.Yeucauthem)
                .HasMaxLength(200)
                .HasColumnName("yeucauthem");

            entity.HasOne(d => d.MabanNavigation).WithMany(p => p.Hoadons)
                .HasForeignKey(d => d.Maban)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__hoadon__maban__178D7CA5");

            entity.HasOne(d => d.MakhuyenmaiNavigation).WithMany(p => p.Hoadons)
                .HasForeignKey(d => d.Makhuyenmai)
                .HasConstraintName("FK__hoadon__makhuyen__1881A0DE");

            entity.HasOne(d => d.ManguoidungNavigation).WithMany(p => p.Hoadons)
                .HasForeignKey(d => d.Manguoidung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__hoadon__manguoid__1699586C");
        });

        modelBuilder.Entity<Khuyenmai>(entity =>
        {
            entity.HasKey(e => e.Makhuyenmai).HasName("PK__khuyenma__77F420CDFAF775F5");

            entity.ToTable("khuyenmai");

            entity.Property(e => e.Makhuyenmai)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CS_AS")
                .HasColumnName("makhuyenmai");
            entity.Property(e => e.Dieukien).HasColumnName("dieukien");
            entity.Property(e => e.Giam).HasColumnName("giam");
            entity.Property(e => e.Ngaybatdau)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("ngaybatdau");
            entity.Property(e => e.Ngayketthuc).HasColumnName("ngayketthuc");
            entity.Property(e => e.Soluong)
                .HasDefaultValue(1)
                .HasColumnName("soluong");
            entity.Property(e => e.Tenkhuyenmai)
                .HasMaxLength(50)
                .HasColumnName("tenkhuyenmai");
        });

        modelBuilder.Entity<Loai>(entity =>
        {
            entity.HasKey(e => e.Maloai).HasName("PK__loai__734B3AEA7EF8FCC9");

            entity.ToTable("loai");

            entity.Property(e => e.Maloai)
                .HasMaxLength(50)
                .HasColumnName("maloai");
            entity.Property(e => e.Tenloai)
                .HasMaxLength(50)
                .HasColumnName("tenloai");
        });

        modelBuilder.Entity<Nguoidung>(entity =>
        {
            entity.HasKey(e => e.Manguoidung).HasName("PK__nguoidun__2D5730E67E248AB2");

            entity.ToTable("nguoidung");

            entity.HasIndex(e => e.Maquyen, "IX_nguoidung_maquyen");

            entity.HasIndex(e => e.Email, "UQ__nguoidun__AB6E6164FF455854").IsUnique();

            entity.Property(e => e.Manguoidung)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CS_AS")
                .HasColumnName("manguoidung");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.Hovaten)
                .HasMaxLength(50)
                .HasColumnName("hovaten");
            entity.Property(e => e.Maquyen)
                .HasMaxLength(50)
                .HasDefaultValueSql("((0))")
                .HasColumnName("maquyen");
            entity.Property(e => e.Matkhau)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CS_AS")
                .HasColumnName("matkhau");
            entity.Property(e => e.Maxacnhan)
                .HasMaxLength(50)
                .HasColumnName("maxacnhan");
            entity.Property(e => e.Ngaytao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("ngaytao");

            entity.HasOne(d => d.MaquyenNavigation).WithMany(p => p.Nguoidungs)
                .HasForeignKey(d => d.Maquyen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__nguoidung__maquy__7DCDAAA2");
        });

        modelBuilder.Entity<Quyen>(entity =>
        {
            entity.HasKey(e => e.Maquyen).HasName("PK__quyen__AA0E356E08C0C08B");

            entity.ToTable("quyen");

            entity.Property(e => e.Maquyen)
                .HasMaxLength(50)
                .HasColumnName("maquyen");
            entity.Property(e => e.Tenquyen)
                .HasMaxLength(50)
                .HasColumnName("tenquyen");
        });

        modelBuilder.Entity<Sanpham>(entity =>
        {
            entity.HasKey(e => e.Masanpham).HasName("PK__sanpham__8F121B2F1609F04C");

            entity.ToTable("sanpham");

            entity.HasIndex(e => e.Maloai, "IX_sanpham_maloai");

            entity.Property(e => e.Masanpham)
                .HasMaxLength(50)
                .UseCollation("SQL_Latin1_General_CP1_CS_AS")
                .HasColumnName("masanpham");
            entity.Property(e => e.Anh)
                .HasColumnType("image")
                .HasColumnName("anh");
            entity.Property(e => e.Gia).HasColumnName("gia");
            entity.Property(e => e.Maloai)
                .HasMaxLength(50)
                .HasDefaultValueSql("((0))")
                .HasColumnName("maloai");
            entity.Property(e => e.Mota)
                .HasMaxLength(50)
                .HasDefaultValue("Không có mô t?")
                .HasColumnName("mota");
            entity.Property(e => e.Ngaythem)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("ngaythem");
            entity.Property(e => e.Tensanpham)
                .HasMaxLength(50)
                .HasColumnName("tensanpham");

            entity.HasOne(d => d.MaloaiNavigation).WithMany(p => p.Sanphams)
                .HasForeignKey(d => d.Maloai)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__sanpham__maloai__0A338187");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
