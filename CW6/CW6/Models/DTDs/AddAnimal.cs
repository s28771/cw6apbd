﻿using System.ComponentModel.DataAnnotations;

namespace CW6.Models.DTDs;

public class AddAnimal
{
    [Required]
    [MinLength(1)]
    [MaxLength(200)]
    public string Name { get; set; }
    [MaxLength(200)]
    public string? Description { get; set; }
    [MaxLength(100)]
    public string? Category { get; set; } 

    [MaxLength(100)]
    public string? Area { get; set; }
}