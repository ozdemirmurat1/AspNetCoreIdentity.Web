using AspNetCoreIdentity.Web.Extensions;
using AspNetCoreIdentity.Repository.Models;
using AspNetCoreIdentity.Core.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;
using System.Security.Claims;
using AspNetCoreIdentity.Core.Models;
using AspNetCoreIdentityApp.Service.Services;

namespace AspNetCoreIdentity.Web.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IFileProvider _fileProvider;
        private readonly IMemberService _memberService;

        //=> sadece get i olan bir property anlamına gelir.
        private string userName => User.Identity!.Name;

        //private readonly IHttpContextAccessor _contextAccessor;

        public MemberController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IFileProvider fileProvider, IMemberService memberService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _fileProvider = fileProvider;
            _memberService = memberService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _memberService.GetUserViewModelByUserNameAssync(userName));
        }

        public async Task LogOut()
        {
           await _memberService.LogOutAsync();      
        }

        public IActionResult PasswordChange()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PasswordChange(PasswordChangeViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var currentUser =await _userManager.FindByNameAsync(User.Identity.Name);

            var checkOldPassword =await _userManager.CheckPasswordAsync(currentUser, request.PasswordOld);

            if (!checkOldPassword)
            {
                ModelState.AddModelError(string.Empty, "Eski şifreniz yanlış");
                return View();
            }

            var resultChangePassword = await _userManager.ChangePasswordAsync(currentUser,request.PasswordOld,request.PasswordNew);

            if (!resultChangePassword.Succeeded)
            {
                ModelState.AddModelErrorList(resultChangePassword.Errors);
                return View();
            }

            //// SecutityStamp'ı hassas bilgiler değiştiğinde güncelliyoruz.Örneğin kullanıcın hesabı hem mobilde hem web de açık. web de güncelleme yaptı. Bu değişiklikler mobil e de yansısın.
            // kullanıcının cookie'si yenilenmesi için ilk önce signOut yaptık daha sonra PasswordSignIn ile login olduk.
            await _userManager.UpdateSecurityStampAsync(currentUser);
            await _signInManager.SignOutAsync();
            await _signInManager.PasswordSignInAsync(currentUser, request.PasswordNew, true,false);

            TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirilmiştir.";

            return View();
        }

        public async Task<IActionResult> UserEdit()
        {
            ViewBag.genderList = new SelectList(Enum.GetNames(typeof(Gender)));

            var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name!)!;

            var userEditViewModel = new UserEditViewModel()
            {
                UserName=currentUser.UserName,
                Email=currentUser.Email,
                Phone=currentUser.PhoneNumber,
                BirthDate=currentUser.BirthDate,
                City=currentUser.City,
                Gender=currentUser.Gender,
            };

            return View(userEditViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UserEdit(UserEditViewModel request)
        {
            if (!ModelState.IsValid)
                return View();


            var currentUser=await _userManager.FindByNameAsync(User.Identity!.Name!);

            currentUser.UserName=request.UserName;
            currentUser.Email=request.Email;
            currentUser.BirthDate=request.BirthDate;
            currentUser.City=request.City;
            currentUser.Gender=request.Gender;
            currentUser.PhoneNumber = request.Phone;

            

            if(request.Picture!=null && request.Picture.Length>0)
            {
                // benim referans klasörüm AspNetCoreIdentity.Web bunu program cs de belirttik.Bunun altındaki klasörü tarar.

                var wwrootFolder = _fileProvider.GetDirectoryContents("wwwroot");

                var randomFileName=  $"{Guid.NewGuid().ToString()}{Path.GetExtension(request.Picture.FileName)}"; // .jpg

                var newPicturePath = Path.Combine(wwrootFolder!.First(x => x.Name == "userpictures").PhysicalPath!, randomFileName);

                using var stream = new FileStream(newPicturePath, FileMode.Create);

                await request.Picture.CopyToAsync(stream);

                currentUser.Picture = randomFileName;
            }

            var updateToUserResult=  await _userManager.UpdateAsync(currentUser);

            if (!updateToUserResult.Succeeded)
            {
                ModelState.AddModelErrorList(updateToUserResult.Errors);
                return View();
            }

            // SecutityStamp'ı hassas bilgiler değiştiğinde güncelliyoruz.Örneğin kullanıcın hesabı hem mobilde hem web de açık. web de güncelleme yaptı. Bu değişiklikler mobil e de yansısın.

            await _userManager.UpdateSecurityStampAsync(currentUser);
            await _signInManager.SignOutAsync();

            if (request.BirthDate.HasValue)
            {
                await _signInManager.SignInWithClaimsAsync(currentUser, true, new[] { new Claim("birthdate", currentUser.BirthDate.Value.ToString()) });
            }
            else
            {
                await _signInManager.SignInAsync(currentUser, true);
            }

            TempData["SuccessMessage"] = "Üye bilgileri başarıyla değiştirilmiştir.";

            var userEditViewModel = new UserEditViewModel()
            {
                UserName = currentUser.UserName,
                Email = currentUser.Email,
                Phone = currentUser.PhoneNumber,
                BirthDate = currentUser.BirthDate,
                City = currentUser.City,
                Gender = currentUser.Gender,
            };

            return RedirectToAction("Index");

        }

        public async Task<IActionResult> AccessDenied(string ReturnUrl)
        {
            string message=string.Empty;
            message = "Bu sayfayı görmeye yetkiniz yoktur.Yetki almak için yöneticinizle görüşünüz.";

            ViewBag.message = message;

            return View();
        }

        [HttpGet]
        public IActionResult Claims()
        {
            var userClaims = User.Claims.Select(x => new ClaimViewModel()
            {
                Issuer=x.Issuer,
                Type=x.Type,
                Value=x.Value,
            }).ToList();

            return View(userClaims);
        }

        [Authorize(Policy = "AnkaraPolicy")]
        [HttpGet]
        public IActionResult AnkaraPage()
        {
            return View();
        }

        [Authorize(Policy = "ExchangePolicy")]
        [HttpGet]
        public IActionResult ExchangePage()
        {
            return View();
        }


        [Authorize(Policy = "ViolencePolicy")]
        [HttpGet]
        public IActionResult ViolencePage()
        {
            return View();
        }



    }
}
