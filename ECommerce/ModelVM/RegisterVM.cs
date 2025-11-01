﻿
namespace ECommerce.ModelVM
{
    public class RegisterVM
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required , EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;
        [Required , DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required , DataType(DataType.Password) , Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
