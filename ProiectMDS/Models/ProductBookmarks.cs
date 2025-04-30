using System.ComponentModel.DataAnnotations.Schema;

namespace ProiectMDS.Models
{
    public class ProductBookmarks
    {
        // tabelul asociativ care face legatura intre Product si Bookmark
        // un articol are mai multe colectii din care face parte
        // iar o colectie contine mai multe articole in cadrul ei
        public class ProductBookmark
        {
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            // cheie primara compusa (Id, ProductId, BookmarkId)
            public int Id { get; set; }
            public int? ProductId { get; set; }
            public int? BookmarkId { get; set; }

            public virtual Product? Product { get; set; }
            public virtual Bookmark? Bookmark { get; set; }

            public DateTime BookmarkDate { get; set; }
        }
    }
}
