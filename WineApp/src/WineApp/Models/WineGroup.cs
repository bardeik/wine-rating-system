using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WineApp.Models;

public enum WineGroup
{
    [Display(Name = "A1 - Godkjente sorter friland (Norge)")]
    A1,
    
    [Display(Name = "A2 - Nordiske gjesteviner")]
    A2,
    
    [Display(Name = "B - Godkjente sorter veksthus")]
    B,
    
    [Display(Name = "C - Prøvesorter friland")]
    C,
    
    [Display(Name = "D - Prøvesorter veksthus")]
    D
}
