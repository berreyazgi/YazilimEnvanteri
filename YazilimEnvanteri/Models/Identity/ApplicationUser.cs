using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace YazilimEnvanteri.Models.Identity
{
    // Identity's AspNetUsers row (UserName = Email). Never passed to views directly -
    // see Models/ViewModels/UserRoleViewModel.
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string AdSoyad { get; set; } = string.Empty;
    }
}
