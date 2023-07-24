using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AspNetCoreIdentity.Web.Requirements
{
    public class ExchangeExpireRequirement:IAuthorizationRequirement
    {
        // Parametre alsaydı buradan halledecektik
    }

    //Aşağıdaki class ı dependency ınjection ettik program cs de
    public class ExchangeExpireRequirementHandler : AuthorizationHandler<ExchangeExpireRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ExchangeExpireRequirement requirement)
        {

            if (!context.User.HasClaim(x => x.Type == "ExchangeExpireDate"))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            Claim exchangeExpireDate = context.User.FindFirst("ExchangeExpireDate");

            if (DateTime.Now>Convert.ToDateTime(exchangeExpireDate.Value))
            {
                context.Fail();
                return Task.CompletedTask;
            }


            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
