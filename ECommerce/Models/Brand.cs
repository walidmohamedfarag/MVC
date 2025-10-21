﻿namespace ECommerce.Models
{
    public class Brand
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Status { get; set; }
        public string Image { get; set; } = string.Empty;
    }
}
