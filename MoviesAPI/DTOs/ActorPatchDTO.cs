﻿using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.DTOs
{
    public class ActorPatchDTO
    {
        [Required]
        [StringLength(120)]
        public string A_Name { get; set; }

        public DateTime BirthDate { get; set; }
    }
}
