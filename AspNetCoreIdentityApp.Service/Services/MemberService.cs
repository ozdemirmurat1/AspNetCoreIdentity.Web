using AspNetCoreIdentity.Core.ViewModels;
using AspNetCoreIdentity.Repository.Models;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreIdentityApp.Service.Services
{
    public class MemberService:IMemberService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public MemberService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task LogOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<UserViewModel> GetUserViewModelByUserNameAssync(string userName)
        {
            var currentUser=await _userManager.FindByNameAsync(userName);

            return new UserViewModel
            {
                Email = currentUser.Email,
                UserName = currentUser.UserName,
                PhoneNumber = currentUser.PhoneNumber,
                PictureUrl = currentUser.Picture
            };
        }

        public async Task<bool> CheckPasswordAsync(string userName,string password)
        {
            var currentUser = await _userManager.FindByNameAsync(userName);

            return await _userManager.CheckPasswordAsync(currentUser, password);
        }

        public async Task<(bool,IEnumerable<IdentityError>)> ChangePasswordAsync(string userName,string oldPassword,string newPassword)
        {
            var currentUser = await _userManager.FindByNameAsync(userName);

            var resultChangePassword = await _userManager.ChangePasswordAsync(currentUser, oldPassword, newPassword);

            if (!resultChangePassword.Succeeded)
            {
                return (false, resultChangePassword.Errors);
            }

            //// SecutityStamp'ı hassas bilgiler değiştiğinde güncelliyoruz.Örneğin kullanıcın hesabı hem mobilde hem web de açık. web de güncelleme yaptı. Bu değişiklikler mobil e de yansısın.
            // kullanıcının cookie'si yenilenmesi için ilk önce signOut yaptık daha sonra PasswordSignIn ile login olduk.
            await _userManager.UpdateSecurityStampAsync(currentUser);
            await _signInManager.SignOutAsync();
            await _signInManager.PasswordSignInAsync(currentUser, newPassword, true, false);

            return (true, null);


        }
    }
}
