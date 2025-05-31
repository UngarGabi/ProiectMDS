using System.ComponentModel.DataAnnotations;

namespace ProiectMDS.Models
{
    public class FavoriteItem
    {
        public int Id { get; set; }
        public Product Product { get; set; }

    }
}
