﻿using AspNetCoreIdentity.Web.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AspNetCoreIdentity.Web.Seeds
{
    // Program.cs de seed data oluşturduk. Program ayağa kalkınca data yoksa seed yapar.
    public class PermissionSeed
    {
        public static async Task Seed(RoleManager<AppRole> roleManager)
        {
            var hasBasicRole = await roleManager.RoleExistsAsync("BasicRole");
            var hasAdvancedRole = await roleManager.RoleExistsAsync("AdvancedRole");
            var hasAdminRole = await roleManager.RoleExistsAsync("AdminRole");

            if(!hasBasicRole) 
            {
                await roleManager.CreateAsync(new AppRole() { Name = "BasicRole" });

                var basicRole = (await roleManager.FindByNameAsync("BasicRole"))!;

                await AddReadPermission(basicRole, roleManager);
            }

            if (!hasAdvancedRole)
            {
                await roleManager.CreateAsync(new AppRole() { Name = "AdvancedRole" });

                var basicRole = (await roleManager.FindByNameAsync("AdvancedRole"))!;

                await AddReadPermission(basicRole, roleManager);
                await AddUpdateAndCreatePermission(basicRole, roleManager);
            }

            if (!hasAdminRole)
            {
                await roleManager.CreateAsync(new AppRole() { Name = "AdminRole" });

                var basicRole = (await roleManager.FindByNameAsync("AdminRole"))!;

                await AddReadPermission(basicRole, roleManager);
                await AddUpdateAndCreatePermission(basicRole, roleManager);
                await AddDeletePermission(basicRole, roleManager);
            }
        }

        public static async Task AddReadPermission(AppRole role,RoleManager<AppRole> roleManager)
        {

            await roleManager.AddClaimAsync(role, new Claim("Permission", PermissionsRoot.Permission.Stock.Read));
            await roleManager.AddClaimAsync(role, new Claim("Permission", PermissionsRoot.Permission.Order.Read));
            await roleManager.AddClaimAsync(role, new Claim("Permission", PermissionsRoot.Permission.Catalog.Read));
        }

        public static async Task AddUpdateAndCreatePermission(AppRole role, RoleManager<AppRole> roleManager)
        {

            await roleManager.AddClaimAsync(role, new Claim("Permission", PermissionsRoot.Permission.Stock.Create));
            await roleManager.AddClaimAsync(role, new Claim("Permission", PermissionsRoot.Permission.Order.Create));
            await roleManager.AddClaimAsync(role, new Claim("Permission", PermissionsRoot.Permission.Catalog.Create));

            await roleManager.AddClaimAsync(role, new Claim("Permission", PermissionsRoot.Permission.Stock.Update));
            await roleManager.AddClaimAsync(role, new Claim("Permission", PermissionsRoot.Permission.Order.Update));
            await roleManager.AddClaimAsync(role, new Claim("Permission", PermissionsRoot.Permission.Catalog.Update));
        }

        public static async Task AddDeletePermission(AppRole role, RoleManager<AppRole> roleManager)
        {

            await roleManager.AddClaimAsync(role, new Claim("Permission", PermissionsRoot.Permission.Stock.Delete));
            await roleManager.AddClaimAsync(role, new Claim("Permission", PermissionsRoot.Permission.Order.Delete));
            await roleManager.AddClaimAsync(role, new Claim("Permission", PermissionsRoot.Permission.Catalog.Delete));
        }
    }
}
