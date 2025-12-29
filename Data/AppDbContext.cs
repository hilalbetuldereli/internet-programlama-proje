// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using TarifDefteri.Models;

namespace TarifDefteri.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Bir kullanıcı bir tarife sadece 1 puan verebilir
        modelBuilder.Entity<Rating>()
            .HasIndex(r => new { r.UserId, r.RecipeId })
            .IsUnique();
        
        // Email unique olmalı
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        // Username unique olmalı
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
        
        // Seed Data - Başlangıç Kategorileri
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Çorba", Description = "Çorba tarifleri" },
            new Category { Id = 2, Name = "Ana Yemek", Description = "Ana yemek tarifleri" },
            new Category { Id = 3, Name = "Tatlı", Description = "Tatlı tarifleri" },
            new Category { Id = 4, Name = "Salata", Description = "Salata tarifleri" },
            new Category { Id = 5, Name = "İçecek", Description = "İçecek tarifleri" }
        );
        
        // Seed Data - Test Kullanıcıları (Şifre: 123456)
        modelBuilder.Entity<User>().HasData(
    new User 
    { 
        Id = 1, 
        Username = "admin", 
        Email = "admin@tarifdefteri.com",
        Password = "123456",
        Role = "Admin",
        FullName = "Admin User",
        CreatedDate = new DateTime(2024, 1, 1),
        IsApproved = true  
    },
    new User 
    { 
        Id = 2, 
        Username = "chef1", 
        Email = "chef@tarifdefteri.com",
        Password = "123456",
        Role = "Chef",
        FullName = "Ahmet Şef",
        CreatedDate = new DateTime(2024, 1, 1),
        IsApproved = true  
    },
    new User 
    { 
        Id = 3, 
        Username = "user1", 
        Email = "user@tarifdefteri.com",
        Password = "123456",
        Role = "User",
        FullName = "Normal Kullanıcı",
        CreatedDate = new DateTime(2024, 1, 1),
        IsApproved = true  
    }
);
    }
}