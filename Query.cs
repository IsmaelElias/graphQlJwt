using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace testingAuthenticationWithRoles {
    public class Query {
       public string Unauthorized() {
           return "não autorizado";
       }

       [Authorize]
       public List<string> Authorized([Service] IHttpContextAccessor context) {
           return context.HttpContext.User.Claims.Select(x => $"{x.Type} : {x.Value}").ToList();
       }

       [Authorize]
       public List<string> AuthorizeBetterWay([GlobalState("currentUser")] CurrentUser user) {
           return user.Claims;
       }

       [Authorize(Roles = new [] { "operation", "Admin"})]
       public List<string> AuthorizedOperation([GlobalState("currentUser")] CurrentUser user) {
           return user.Claims;
       }

        [Authorize(Roles = new [] { "finance"})]
        public List<string> AuthorizedFinance([GlobalState("currentUser")] CurrentUser user) {
           return user.Claims;
       }

       [Authorize(Policy = "Admin")]
       public List<string> AuthorizedPolicy([GlobalState("currentUser")] CurrentUser user) {
           return user.Claims;
       }
    }
}