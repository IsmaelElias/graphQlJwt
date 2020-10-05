using System.Threading.Tasks;
using HotChocolate;

namespace testingAuthenticationWithRoles {
    public class Mutation {
        public Task<string> GetToken(string email, string password, [Service] IIdentityService service) {
            return service.Authenticate(email, password);
        }
    }
}