using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Toolio.Web.Models;

namespace Toolio.Web.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }


    public DbSet<Category> Categories { get; set; }

    public DbSet<Equipment> Equipment { get; set; }

    public DbSet<Booking> Bookings { get; set; }

    public DbSet<Review> Reviews { get; set; }


    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        base.OnModelCreating(builder);


        // =====================================================
        // BOOKING -> EQUIPMENT
        // =====================================================
        // Prevent cascade deletion of bookings when
        // equipment is deleted.
        builder.Entity<Booking>()
            .HasOne(b => b.Equipment)
            .WithMany(e => e.Bookings)
            .HasForeignKey(b => b.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);


        // =====================================================
        // REVIEW -> EQUIPMENT
        // =====================================================
        // Prevent cascade deletion of reviews when
        // equipment is deleted.
        builder.Entity<Review>()
            .HasOne(r => r.Equipment)
            .WithMany(e => e.Reviews)
            .HasForeignKey(r => r.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);


        // =====================================================
        // REVIEW -> BOOKING
        // =====================================================
        // A review belongs to one booking.
        // Do not automatically delete the review.
        builder.Entity<Review>()
            .HasOne(r => r.Booking)
            .WithOne()
            .HasForeignKey<Review>(r => r.BookingId)
            .OnDelete(DeleteBehavior.Restrict);


        // =====================================================
        // ONE REVIEW PER BOOKING
        // =====================================================
        builder.Entity<Review>()
            .HasIndex(r => r.BookingId)
            .IsUnique();
    }
}