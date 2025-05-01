using System.ComponentModel.DataAnnotations;

namespace ProiectMDS.Models
{
    public class Request
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public List<Comment> ListOfComments { get; set; }
        public string CommentContent { get; set; }
        public int? ProductId { get; set; }
        public decimal? Rating { get; set; }

        public virtual Product? Product { get; set; }
        //public decimal? Score { get; set; }
    }
}
