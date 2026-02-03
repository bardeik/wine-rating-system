using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace WineApp.Models;

public class WineRating
{
    public int WineRatingId { get; set; }
    
    [Required]
    [Range(0, 10, ErrorMessage = "Visuality must be between 0 and 10")]
    public int Visuality { get; set; }
    
    [Required]
    [Range(0, 10, ErrorMessage = "Nose must be between 0 and 10")]
    public int Nose { get; set; }
    
    [Required]
    [Range(0, 10, ErrorMessage = "Taste must be between 0 and 10")]
    public int Taste { get; set; }
    
    [Required(ErrorMessage = "Judge ID is required")]
    [StringLength(50, ErrorMessage = "Judge ID cannot exceed 50 characters")]
    public string JudgeId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Wine ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid wine")]
    public int WineId { get; set; }
}
