using System.ComponentModel.DataAnnotations;

namespace Toolio.Web.Models;

public class Category
{
    public int CategoryId { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Description { get; set; }

    public ICollection<Equipment> Equipment { get; set; }
        = new List<Equipment>();
}