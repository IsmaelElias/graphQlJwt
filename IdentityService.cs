using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace testingAuthenticationWithRoles {
    public interface IIdentityService {
        Task<string> Authenticate(string email, string password);
    }

    public class Person {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PermissionName { get; set; }
    }

    public class IdentityService : IIdentityService {
        private readonly List<Person> persons = new List<Person>();

        public IdentityService() {
            var ismael = new Person {
                Id = Guid.NewGuid(),
                Name = "Ismael",
                Email = "ismael.esq@hotmail.com",
                Password = "123456",
                PermissionName = "Admin"
            };
            var danilo = new Person {
                Id = Guid.NewGuid(),
                Name = "Danilo",
                Email = "danilo@hotmail.com",
                Password = "123456",
                PermissionName = "operation"
            };
            var vinicius = new Person {
                Id = Guid.NewGuid(),
                Name = "Vinicius",
                Email = "vinicius@hotmail.com",
                Password = "123456",
                PermissionName = "finance"
            };

            persons.Add(ismael);
            persons.Add(danilo);
            persons.Add(vinicius);
        }

        public async Task<string> Authenticate(string email, string password) {
            Console.WriteLine("here");
            var person = persons.First((p) => p.Email == email && p.Password == password);

            if (person is null) {
                throw new AuthenticationException();
            }

            return await Task.FromResult(GenerateAccessToken(person));
        }

        private string GenerateAccessToken(Person person) {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("authenticating12093"));

            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, person.Id.ToString()),
                new Claim(ClaimTypes.Name, person.Email),
                new Claim(ClaimTypes.Role, person.PermissionName),
            };

            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken("issuer", "audience", claims, expires: DateTime.Now.AddMinutes(2), signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}