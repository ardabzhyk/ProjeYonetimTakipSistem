using System;
using System.Collections.Generic;

namespace ProjeYonetimTakipSistem.Models;

public partial class Gorevler
{
    public int Id { get; set; }

    public string Ad { get; set; } = null!;

    public string? Aciklama { get; set; }

    public DateTime BaslangicTarihi { get; set; }

    public DateTime BitisTarihi { get; set; }

    public int Oncelik { get; set; }

    public int Durum { get; set; }

    public int ProjeId { get; set; }

    public string AtananKullaniciId { get; set; } = null!;

    public string? ApplicationUserId { get; set; }

    public virtual AspNetUser? ApplicationUser { get; set; }

    public virtual AspNetUser AtananKullanici { get; set; } = null!;

    public virtual Projeler Proje { get; set; } = null!;
}
