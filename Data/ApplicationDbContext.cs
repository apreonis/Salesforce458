using InventoryManagement.Data.Models;
using InventoryManagement.Data.Models.CustomId;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace InventoryManagement.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<CustomFieldDefinition> CustomFieldDefinitions { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<ItemLike> ItemLikes { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<InventoryAccess> InventoryAccesses { get; set; }
    public DbSet<Tag> Tags { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .Property(u => u.DisplayName)
            .HasMaxLength(256);

        builder.Entity<Tag>()
            .HasIndex(t => t.Name)
            .IsUnique();

        builder.Entity<Item>()
            .HasIndex(i => new { i.InventoryId, i.CustomId })
            .IsUnique();

        builder.Entity<Item>()
            .Property(i => i.RowVersion)
            .IsRequired()
            .IsConcurrencyToken()
            .ValueGeneratedNever()
            .HasColumnType("bytea");

        builder.Entity<Inventory>()
            .Property(i => i.RowVersion)
            .IsRequired()
            .IsConcurrencyToken()
            .ValueGeneratedNever()
            .HasColumnType("bytea");

        builder.Entity<Inventory>()
            .Property(i => i.CustomIdFormat)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<CustomIdFormat>(v, (JsonSerializerOptions?)null) ?? new CustomIdFormat());

        builder.Entity<Inventory>()
            .HasOne(i => i.Owner)
            .WithMany(u => u.OwnedInventories)
            .HasForeignKey(i => i.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<InventoryAccess>()
            .HasKey(ia => new { ia.InventoryId, ia.UserId });

        builder.Entity<InventoryAccess>()
            .HasOne(ia => ia.Inventory)
            .WithMany(i => i.AccessList)
            .HasForeignKey(ia => ia.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<InventoryAccess>()
            .HasOne(ia => ia.User)
            .WithMany(u => u.InventoryAccesses)
            .HasForeignKey(ia => ia.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ItemLike>()
            .HasKey(il => new { il.ItemId, il.UserId });

        builder.Entity<ItemLike>()
            .HasOne(il => il.Item)
            .WithMany(i => i.Likes)
            .HasForeignKey(il => il.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ItemLike>()
            .HasOne(il => il.User)
            .WithMany(u => u.ItemLikes)
            .HasForeignKey(il => il.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CustomFieldDefinition>()
            .HasIndex(c => new { c.InventoryId, c.FieldType, c.FieldIndex })
            .IsUnique();

        builder.Entity<CustomFieldDefinition>()
            .HasOne(c => c.Inventory)
            .WithMany(i => i.CustomFieldDefinitions)
            .HasForeignKey(c => c.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Item>()
            .HasOne(i => i.Inventory)
            .WithMany(inv => inv.Items)
            .HasForeignKey(i => i.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Item>()
            .HasOne(i => i.CreatedBy)
            .WithMany(u => u.Items)
            .HasForeignKey(i => i.CreatedById)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Comment>()
            .HasOne(c => c.Inventory)
            .WithMany(i => i.Comments)
            .HasForeignKey(c => c.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Inventory>()
            .HasMany(i => i.Tags)
            .WithMany(t => t.Inventories)
            .UsingEntity(j => j.ToTable("InventoryTags"));
    }
}