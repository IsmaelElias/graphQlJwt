using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Interceptors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace testingAuthenticationWithRoles
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddHttpContextAccessor();

            services.AddAuthorization(x => {
                x.AddPolicy("Admin", builder => builder.RequireAuthenticatedUser().RequireRole("Admin"));

                x.AddPolicy("Financeiro", builder => builder.RequireRole("finance"));
            });

            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters {
                    ClockSkew = TimeSpan.FromMinutes(5),
                    ValidateLifetime = true,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = "audience",
                    ValidIssuer = "issuer",
                    RequireSignedTokens = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("authenticating12093"))
                };

                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
            });

            services.AddQueryRequestInterceptor(AuthenticationInterceptor());

            services.AddGraphQL(
                SchemaBuilder
                    .New()
                    .AddQueryType<Query>()
                    .AddMutationType<Mutation>()
                    .AddAuthorizeDirectiveType()
                    .Create()
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UsePlayground();
            app.UseGraphQL();
        }

        private static OnCreateRequestAsync AuthenticationInterceptor() {
            return (context, builder, token) => {
                if (context.GetUser().Identity.IsAuthenticated) {
                    builder.SetProperty("currentUser", new CurrentUser(Guid.Parse(context.User.FindFirstValue(ClaimTypes.NameIdentifier)), 
                    context.User.Claims.Select(x => $"{x.Type} : {x.Value}").ToList()));
                }

                return Task.CompletedTask;
            };
        }
    }
}
