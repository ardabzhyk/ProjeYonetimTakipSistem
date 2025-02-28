using System;
using System.Collections.Generic;

namespace ProjeYonetimTakipSistem.Models;

public partial class ProjeKullanicilar
{
    public int Id { get; set; }

    public int ProjeId { get; set; }

    public string KullaniciId { get; set; } = null!;

    public DateTime AtamaTarihi { get; set; }

    public string? ApplicationUserId { get; set; }

    public virtual AspNetUser? ApplicationUser { get; set; }

    public virtual AspNetUser Kullanici { get; set; } = null!;

    public virtual Projeler Proje { get; set; } = null!;
}
