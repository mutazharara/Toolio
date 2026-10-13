using System.ComponentModel.DataAnnotations;

namespace Toolio.Web.Models;

public class Review
{
    public int ReviewId { get; set; }

    [Required]
    public int BookingId { get; set; }

    public Booking? Booking { get; set; }

    [Required]
    public int EquipmentId { get; set; }

    public Equipment? Equipment { get; set; }

    [Required]
    public string ReviewerId { get; set; } = string.Empty;

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [StringLength(500)]
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;
}