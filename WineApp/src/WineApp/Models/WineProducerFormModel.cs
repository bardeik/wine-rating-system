using System.ComponentModel.DataAnnotations;

namespace WineApp.Models;

public class WineProducerFormModel
{
    [Required] public string WineyardName { get; set; } = string.Empty;
    [Required] public string OrganisationNumber { get; set; } = string.Empty;
    [Required] public string ResponsibleProducerName { get; set; } = string.Empty;
    [Required] public string Address { get; set; } = string.Empty;
    [Required] public string Zip { get; set; } = string.Empty;
    [Required] public string City { get; set; } = string.Empty;
    [Required] public string Country { get; set; } = string.Empty;
    [Required, EmailAddress] public string ProducerEmail { get; set; } = string.Empty;
    [Required, EmailAddress] public string LoginEmail { get; set; } = string.Empty;
    [Required, MinLength(8)] public string Password { get; set; } = string.Empty;
}
