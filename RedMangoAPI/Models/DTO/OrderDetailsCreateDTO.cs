﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RedMangoAPI.Models.DTO
{
    public class OrderDetailsCreateDTO
    {
        [Required]
        public int MenuItemId { get; set; }
        [ForeignKey("MenuItemId")]
        public int Quantity { get; set; }
        [Required]
        public string ItemName { get; set; }
        [Required]
        public double Price { get; set; }
    }
}
