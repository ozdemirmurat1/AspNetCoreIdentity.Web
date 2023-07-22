using AspNetCoreIdentity.Web.Extensions;
using AspNetCoreIdentity.Web.Models;
using AspNetCoreIdentity.Web.Services;
using AspNetCoreIdentity.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Diagnostics;
using System.Security.Claims;

namespace AspNetCoreIdentity.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
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
        public async Task<IActionResult> SignIn(SignInViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            returnUrl ??= Url.Action("Index", "Home");

            var hasUser = await _userManager.FindByEmailAsync(model.Email);

            if (hasUser == null)
            {
                ModelState.AddModelError(string.Empty, "Email veya şifre yanlış");
                return View();
            }

            // bu işlemde cookie oluşturulur.
            var signInResult = await _signInManager.PasswordSignInAsync(hasUser, model.Password, model.RememberMe, true);

            if (signInResult.Succeeded)
            {
                return Redirect(returnUrl);
            }

            if (signInResult.IsLockedOut)
            {
                ModelState.AddModelErrorList(new List<string> { "3 dakika boyunca giriş yapamazsınız." });
                return View();
            }


            ModelState.AddModelErrorList(new List<string> { $"Email veya şifreniz yanlış", $"(Başarısız giriş sayısı:{await _userManager.GetAccessFailedCountAsync(hasUser)}" });


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

            var identityResult = await _userManager.CreateAsync(new() { UserName = request.UserName, PhoneNumber = request.Phone, Email = request.Email }, request.PasswordConfirm);

            if (!identityResult.Succeeded)
            {
                ModelState.AddModelErrorList(identityResult.Errors.Select(x => x.Description).ToList());
                return View();
            }

            var exchangeExpireClaim = new Claim("ExchangeExpireDate", DateTime.Now.AddDays(10).ToString());

            var user = await _userManager.FindByNameAsync(request.UserName);

            var claimResult = await _userManager.AddClaimAsync(user!, exchangeExpireClaim);

            if (!claimResult.Succeeded)
            {
                ModelState.AddModelErrorList(claimResult.Errors.Select(x => x.Description).ToList());
                return View();
            }


            // TempData datayı taşıma işlemlerinde kullanılır
            TempData["SuccessMessage"] = "Üyelik kayıt işlemi başarı ile gerçekleşmiştir.";
            return RedirectToAction(nameof(HomeController.SignUp));



        }

        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel request)
        {
            //7250 portundan ayağa kalkıyor htt://localhost:7250

            var hasUser = await _userManager.FindByEmailAsync(request.Email);

            if (hasUser == null)

            {
                // String.Empty diyerek özellikle bir hata belirtmedik.
                ModelState.AddModelError(String.Empty, "Bu email adresine sahip kullanıcı bulunamamıştır.");
                return View();
            }

            string passwordResestToken = await _userManager.GeneratePasswordResetTokenAsync(hasUser);

            var passwordResetLink = Url.Action("ResetPassword", "Home", new { userId = hasUser.Id, Token = passwordResestToken }, HttpContext.Request.Scheme);

            //örnek link https://localhost:7006?userId=12213&token=aajsdfjdsalkfjkdsfj

            await _emailService.SendResetPasswordEmail(passwordResetLink!, hasUser.Email!);

            TempData["SuccessMessage"] = "Şifre yenileme linki, eposta adresinize gönderilmiştir";

            return RedirectToAction(nameof(ForgetPassword));


        }

        public IActionResult ResetPassword(string userId, string token)
        {
            // Requestler arası data TempData üzerinden taşınır. ViewModel de hidden input type üzerinden de alınabilir

            TempData["userId"] = userId;
            TempData["token"] = token;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel request)
        {
            var userId = TempData["userId"];
            var token = TempData["token"];

            if (userId == null || token == null)
            {
                throw new Exception("Bir hata meydana geldi");
            }

            var hasUser = await _userManager.FindByIdAsync(userId.ToString()!);
            if (hasUser == null)
            {
                ModelState.AddModelError(String.Empty, "Kullanıcı Bulunamamıştır");

            }


            var result = await _userManager.ResetPasswordAsync(hasUser, token.ToString()!, request.Password);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Şifreniz başarıyla yenilenmiştir.";
            }
            else
            {
                ModelState.AddModelErrorList(result.Errors.Select(x => x.Description).ToList());

            }

            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}