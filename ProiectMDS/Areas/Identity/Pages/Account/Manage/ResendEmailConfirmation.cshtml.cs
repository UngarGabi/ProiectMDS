using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProiectMDS.Areas.Identity.Pages.Account.Manage
{
    
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ResendEmailConfirmationModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }
       

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Products");
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Page(
            "/Account/ConfirmEmail",
            null,
            new { userId = user.Id, code },
            Request.Scheme);

            await _emailSender.SendEmailAsync(
                user.Email,
                "Confirmă-ți emailul",
                $"Te rugăm să confirmi contul apăsând <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>aici</a>.");

            return Page();

        }
    }
}
