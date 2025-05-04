using System.ComponentModel.DataAnnotations;

namespace ProiectMDS.Models
{
    public class Request
    {
        [Key]
        public int Id { get; set; }
        public int? ProductId { get; set; } // Produsul care trebuie aprobat
        [Required]
        public string RequestedByUserId { get; set; } = string.Empty; // Utilizatorul care a solicitat
        [Required]
        public string RequestType { get; set; } = string.Empty; // Tipul de cerere: "ADD" sau "EDIT"

        public string Status { get; set; } = "Pending"; // Statusul cererii: "Pending", "Approved", "Rejected"

        public virtual Product? Product { get; set; }
        public virtual ApplicationUser? RequestedByUser { get; set; }
    }
}
