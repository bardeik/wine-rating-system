using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace WineApp.Models;

public class Wine
{
    public int WineId { get; set; }
    
    [Required(ErrorMessage = "Rating name is required")]
    [StringLength(100, ErrorMessage = "Rating name cannot exceed 100 characters")]
    public string RatingName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public WineGroup Group { get; set; }
    
    [Required]
    public WineClass Class { get; set; }
    
    [Required]
    public WineCategory Category { get; set; }
    
    [Required(ErrorMessage = "Wine Producer is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid wine producer")]
    public int WineProducerId { get; set; }
}
