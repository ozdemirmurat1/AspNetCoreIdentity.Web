﻿using AspNetCoreIdentity.Core.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreIdentityApp.Service.Services
{
    public interface IMemberService
    {
        Task<UserViewModel> GetUserViewModelByUserNameAssync(string userName);

        Task LogOutAsync();

        Task<bool> CheckPasswordAsync(string userName, string password);

        Task<(bool, IEnumerable<IdentityError>)> ChangePasswordAsync(string userName, string oldPassword, string newPassword);

        Task<UserEditViewModel> GetUserEditViewModelAsync(string userName);

        SelectList GetGenderSelectList();

        Task<(bool, IEnumerable<IdentityError>)> EditUserAsync(UserEditViewModel request, string userName);

        List<ClaimViewModel> GetClaims(ClaimsPrincipal principal);
    }
}
