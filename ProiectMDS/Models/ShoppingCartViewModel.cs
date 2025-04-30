using System.ComponentModel.DataAnnotations;

namespace ProiectMDS.Models
{
    public class ShoppingCartViewModel
    {
        [Key]
        public int Id { get; set; }
        public List<ShoppingCartItem> CartItems { get; set; }
        public decimal? TotalPrice { get; set; }
        public int? TotalQuantity { get; set; }
    }
}
