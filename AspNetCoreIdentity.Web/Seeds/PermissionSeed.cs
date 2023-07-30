using AspNetCoreIdentity.Web.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AspNetCoreIdentity.Web.Seeds
{
    // Program.cs de seed data oluşturduk
    public class PermissionSeed
    {
        public static async Task Seed(RoleManager<AppRole> roleManager)
        {
            var hasBasicRole = await roleManager.RoleExistsAsync("BasicRole");

            if(!hasBasicRole) 
            {
                await roleManager.CreateAsync(new AppRole() { Name = "BasicRole" });

                var basicRole = (await roleManager.FindByNameAsync("BasicRole"))!;

               await  roleManager.AddClaimAsync(basicRole, new Claim("Permission", PermissionsRoot.Permission.Stock.Read));
               await  roleManager.AddClaimAsync(basicRole, new Claim("Permission", PermissionsRoot.Permission.Order.Read));
               await  roleManager.AddClaimAsync(basicRole, new Claim("Permission", PermissionsRoot.Permission.Catalog.Read));
            }
        }
    }
}
