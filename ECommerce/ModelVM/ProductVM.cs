namespace ECommerce.ModelVM
{
    public class ProductVM
    {
        public IEnumerable<Brand> brands { get; set; }
        public IEnumerable<Categroy> Categroys { get; set; }
        public IEnumerable<ProductSubImage> ProductSubImages { get; set; }
        public Product? product { get; set; }
        public IEnumerable<ProductColor>? productColors { get; set; }
    }
}
