using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjeYonetimTakipSistem.Models;

namespace ProjeYonetimTakipSistem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Proje> Projeler { get; set; }
        public DbSet<ProjeKullanici> ProjeKullanicilar { get; set; }
        public DbSet<Gorev> Gorevler { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ProjeKullanici>()
                .HasKey(pk => pk.Id);

            builder.Entity<ProjeKullanici>()
                .HasOne(pk => pk.Proje)
                .WithMany(p => p.ProjeKullanicilari)
                .HasForeignKey(pk => pk.ProjeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProjeKullanici>()
                .HasOne(pk => pk.Kullanici)
                .WithMany()
                .HasForeignKey(pk => pk.KullaniciId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Gorev>()
                .HasOne(g => g.Proje)
                .WithMany(p => p.Gorevler)
                .HasForeignKey(g => g.ProjeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Gorev>()
                .HasOne(g => g.AtananKullanici)
                .WithMany()
                .HasForeignKey(g => g.AtananKullaniciId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 