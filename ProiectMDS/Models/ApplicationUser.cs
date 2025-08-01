﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProiectMDS.Models
{
    public class ApplicationUser:IdentityUser
    {
        
    //USERI SI ROLURI
    //un user poate posta mai multe comentarii
    public virtual ICollection<Comment>? Comments { get; set; }

        //un user poate posta mai multe produse
        public virtual ICollection<Product>? Products { get; set; }

        // atribute suplimentare adaugate pentru user
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // variabila in care vom retine rolurile existente in baza de date
        // pentru popularea unui dropdown list
        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }
    }

}
