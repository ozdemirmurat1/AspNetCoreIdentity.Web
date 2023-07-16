using AspNetCoreIdentity.Web.Extensions;
using AspNetCoreIdentity.Web.Models;
using AspNetCoreIdentity.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Diagnostics;

namespace AspNetCoreIdentity.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult SignIn()
        {
            return View();
        }

        // Identity kütüphanesi returnUrl i dolduracak
        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel model,string returnUrl=null)
        {
            returnUrl = returnUrl ?? Url.Action("Index", "Home");

            var hasUser=await _userManager.FindByEmailAsync(model.Email);

            if (hasUser == null)
            {
                ModelState.AddModelError(string.Empty, "Email veya şifre yanlış");
                return View();
            }

            // bu işlemde cookie oluşturulur.
            var signInResult = await _signInManager.PasswordSignInAsync(hasUser, model.Password,model.RememberMe,true);

            if (signInResult.Succeeded)
            {
                return Redirect(returnUrl);
            }

            if (signInResult.IsLockedOut)
            {
                ModelState.AddModelErrorList(new List<string> { "3 dakika boyunca giriş yapamazsınız." });
                return View();
            }
                
            
            ModelState.AddModelErrorList(new List<string> { $"Email veya şifreniz yanlış",$"(Başarısız giriş sayısı:{await _userManager.GetAccessFailedCountAsync(hasUser)}" });
            

            return View();
            
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel request)
        {
            // Hash'lenen bir data geriye doğru çözülmez. Bu yüzden db'de hash'lenerek saklanır.
            // hash=> Password12* => ajdjkajkcjkdjcjkd
            //      => Password12* => dkckdscdscdscd

            if (!ModelState.IsValid)
                return View();

            var identityResult=await _userManager.CreateAsync(new() { UserName = request.UserName, PhoneNumber = request.Phone, Email = request.Email }, request.PasswordConfirm);

            if (identityResult.Succeeded)
            {
                // TempData datayı taşıma işlemlerinde kullanılır
                TempData["SuccessMessage"] = "Üyelik kayıt işlemi başarı ile gerçekleşmiştir.";
                return RedirectToAction(nameof(HomeController.SignUp));
            }

            ModelState.AddModelErrorList(identityResult.Errors.Select(x=>x.Description).ToList());

            return View();
        }

        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel request)
        {
            //7250 portundan ayağa kalkıyor htt://localhost:7250

            var hasUser=await _userManager.FindByEmailAsync(request.Email);

            if (hasUser == null)
            {
                /* String.Empty diyerek özellikle bir hata belirtmedik  */

                ModelState.AddModelError(String.Empty, "Bu email adresine sahip kullanıcı bulunamamıştır");
                return View();
            }

            string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(hasUser);

            var passwordResetLink = Url.Action("ResetPassword", "Home", new { userId = hasUser.Id, Token = passwordResetToken });

            // EmailService e ihtiyaç var.

            TempData["SuccessMessage"] = "Şifre yenileme linki e-posta adresinize gönderilmiştir.";
            return RedirectToAction(nameof(ForgetPassword));
               

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}