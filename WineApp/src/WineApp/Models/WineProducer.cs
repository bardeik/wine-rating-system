using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace WineApp.Models;

public class WineProducer
{
    public int WineProducerId { get; set; }
    
    [Required(ErrorMessage = "Wineyard name is required")]
    [StringLength(100, ErrorMessage = "Wineyard name cannot exceed 100 characters")]
    public string WineyardName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Organisation number is required")]
    [StringLength(20, ErrorMessage = "Organisation number cannot exceed 20 characters")]
    public string OrganisationNumber { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Responsible producer name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string ResponsibleProducerName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Address is required")]
    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    public string Address { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "City is required")]
    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string City { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Country is required")]
    [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
    public string Country { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Zip code is required")]
    [StringLength(20, ErrorMessage = "Zip code cannot exceed 20 characters")]
    public string Zip { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; set; } = string.Empty;
}
