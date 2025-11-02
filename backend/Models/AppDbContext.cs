using System;
using backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Models;

public class AppDbContext : DbContext
{
   public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
   {
      Database.EnsureCreated();
   }

   public DbSet<Resource> Resources { get; set; }
   public DbSet<ResourceType> ResourceTypes { get; set; }
   public DbSet<Booking> Bookings { get; set; }
   public DbSet<ChatMessage> ChatMessages { get; set; }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      base.OnModelCreating(modelBuilder);

      modelBuilder.Entity<Booking>()
         .HasOne(b => b.Resource)
         .WithMany()
         .HasForeignKey(b => b.ResourceId);

      modelBuilder.Entity<Resource>()
         .HasOne(r => r.ResourceType)
         .WithMany()
         .HasForeignKey(r => r.ResourceTypeId);

      modelBuilder.Entity<ResourceType>().HasData(
         new ResourceType { Id = 1, Name = "Dropin-skrivbord" },
         new ResourceType { Id = 2, Name = "Mötesrum" },
         new ResourceType { Id = 3, Name = "VR-headset" },
         new ResourceType { Id = 4, Name = "AI-server" }
      );

      modelBuilder.Entity<Resource>().HasData(
         new Resource { Id = 1, Name = "Skrivbord 1", ResourceTypeId = 1 },
         new Resource { Id = 2, Name = "Skrivbord 2", ResourceTypeId = 1 },
         new Resource { Id = 3, Name = "Skrivbord 3", ResourceTypeId = 1 },
         new Resource { Id = 4, Name = "Skrivbord 4", ResourceTypeId = 1 },
         new Resource { Id = 5, Name = "Skrivbord 5", ResourceTypeId = 1 },
         new Resource { Id = 6, Name = "Skrivbord 6", ResourceTypeId = 1 },
         new Resource { Id = 7, Name = "Skrivbord 7", ResourceTypeId = 1 },
         new Resource { Id = 8, Name = "Skrivbord 8", ResourceTypeId = 1 },
         new Resource { Id = 9, Name = "Skrivbord 9", ResourceTypeId = 1 },
         new Resource { Id = 10, Name = "Skrivbord 10", ResourceTypeId = 1 },
         new Resource { Id = 11, Name = "Skrivbord 11", ResourceTypeId = 1 },
         new Resource { Id = 12, Name = "Skrivbord 12", ResourceTypeId = 1 },
         new Resource { Id = 13, Name = "Skrivbord 13", ResourceTypeId = 1 },
         new Resource { Id = 14, Name = "Skrivbord 14", ResourceTypeId = 1 },
         new Resource { Id = 15, Name = "Skrivbord 15", ResourceTypeId = 1 },
         new Resource { Id = 16, Name = "Mötesrum 1", ResourceTypeId = 2 },
         new Resource { Id = 17, Name = "Mötesrum 2", ResourceTypeId = 2 },
         new Resource { Id = 18, Name = "Mötesrum 3", ResourceTypeId = 2 },
         new Resource { Id = 19, Name = "Mötesrum 4", ResourceTypeId = 2 },
         new Resource { Id = 20, Name = "VR-headset 1", ResourceTypeId = 3 },
         new Resource { Id = 21, Name = "VR-headset 2", ResourceTypeId = 3 },
         new Resource { Id = 22, Name = "VR-headset 3", ResourceTypeId = 3 },
         new Resource { Id = 23, Name = "VR-headset 4", ResourceTypeId = 3 },
         new Resource { Id = 24, Name = "AI-server 1", ResourceTypeId = 4 }
      );
   }
}
