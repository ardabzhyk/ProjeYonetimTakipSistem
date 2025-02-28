using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProjeYonetimTakipSistem.Models;

public partial class ProjeYonetimTakipSistemiContext : DbContext
{
    public ProjeYonetimTakipSistemiContext(DbContextOptions<ProjeYonetimTakipSistemiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Gorevler> Gorevlers { get; set; }

    public virtual DbSet<ProjeKullanicilar> ProjeKullanicilars { get; set; }

    public virtual DbSet<Projeler> Projelers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Ad).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.Soyad).HasMaxLength(50);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Gorevler>(entity =>
        {
            entity.ToTable("Gorevler");

            entity.HasIndex(e => e.ApplicationUserId, "IX_Gorevler_ApplicationUserId");

            entity.HasIndex(e => e.AtananKullaniciId, "IX_Gorevler_AtananKullaniciId");

            entity.HasIndex(e => e.ProjeId, "IX_Gorevler_ProjeId");

            entity.Property(e => e.Ad).HasMaxLength(100);

            entity.HasOne(d => d.ApplicationUser).WithMany(p => p.GorevlerApplicationUsers).HasForeignKey(d => d.ApplicationUserId);

            entity.HasOne(d => d.AtananKullanici).WithMany(p => p.GorevlerAtananKullanicis)
                .HasForeignKey(d => d.AtananKullaniciId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Proje).WithMany(p => p.Gorevlers).HasForeignKey(d => d.ProjeId);
        });

        modelBuilder.Entity<ProjeKullanicilar>(entity =>
        {
            entity.ToTable("ProjeKullanicilar");

            entity.HasIndex(e => e.ApplicationUserId, "IX_ProjeKullanicilar_ApplicationUserId");

            entity.HasIndex(e => e.KullaniciId, "IX_ProjeKullanicilar_KullaniciId");

            entity.HasIndex(e => e.ProjeId, "IX_ProjeKullanicilar_ProjeId");

            entity.HasOne(d => d.ApplicationUser).WithMany(p => p.ProjeKullanicilarApplicationUsers).HasForeignKey(d => d.ApplicationUserId);

            entity.HasOne(d => d.Kullanici).WithMany(p => p.ProjeKullanicilarKullanicis).HasForeignKey(d => d.KullaniciId);

            entity.HasOne(d => d.Proje).WithMany(p => p.ProjeKullanicilars).HasForeignKey(d => d.ProjeId);
        });

        modelBuilder.Entity<Projeler>(entity =>
        {
            entity.ToTable("Projeler");

            entity.Property(e => e.Ad).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
