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
        public string PromoCode { get; set; }
        public decimal DiscountValue { get; set; } // reducerea in bani
        public decimal? FinalPrice => TotalPrice - DiscountValue;
    }
}
