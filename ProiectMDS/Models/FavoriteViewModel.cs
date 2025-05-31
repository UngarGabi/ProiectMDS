using System.ComponentModel.DataAnnotations;

namespace ProiectMDS.Models
{
    public class FavoriteViewModel
    {
        [Key]
        public int Id { get; set; }
        public List<FavoriteItem> FavItems { get; set; }


    }
}
