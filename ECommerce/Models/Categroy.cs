﻿namespace ECommerce.Models
{
    public class Categroy
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Status { get; set; }
    }
}
