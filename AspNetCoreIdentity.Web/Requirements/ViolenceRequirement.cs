using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AspNetCoreIdentity.Web.Requirements
{
    public class ViolenceRequirement : IAuthorizationRequirement
    {
        public int ThresholdAge { get; set; }
    }

    // Aşağıdaki class ı dependency ınjection ettik program cs de
    public class ViolenceRequirementHandler : AuthorizationHandler<ViolenceRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ViolenceRequirement requirement)
        {
            if (!context.User.HasClaim(x => x.Type == "birthdate"))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            Claim birthDateClaim = context.User.FindFirst("birthdate");

            var today = DateTime.Now;
            var birthDate=Convert.ToDateTime(birthDateClaim.Value);
            var age=today.Year - birthDate.Year;

            if (birthDate > today.AddYears(-age)) age--;

            if (requirement.ThresholdAge>age)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
