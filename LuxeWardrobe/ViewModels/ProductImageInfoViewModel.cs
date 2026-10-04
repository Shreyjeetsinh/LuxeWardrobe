namespace LuxeWardrobe.ViewModels
{
    public class ProductImageInfoViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string FileName { get; set; }
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }
    }
}
