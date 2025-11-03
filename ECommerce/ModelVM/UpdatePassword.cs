namespace ECommerce.ModelVM
{
    public class UpdatePassword
    {
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

    }
}
