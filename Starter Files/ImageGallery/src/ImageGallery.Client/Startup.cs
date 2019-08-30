﻿using ImageGallery.Client.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;

namespace ImageGallery.Client
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // register an IHttpContextAccessor so we can access the current
            // HttpContext in services by injecting it
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            }).AddCookie("Cookies")
            .AddOpenIdConnect("oidc", options =>
           {
               options.SignInScheme = "Cookies";
               options.Authority = "https://localhost:44361";
               options.ClientId = "imagegalleryclient";
               options.ResponseType = "code id_token";
               //options.CallbackPath = new PathString("/signin-oidc");//https://localhost:44344/signin-oidc
               //options.SignedOutCallbackPath = new PathString("/signout-callback-oidc");//"https://localhost:44344/signout-callback-oidc"
               options.Scope.Add("openid");
               options.Scope.Add("profile");
               options.Scope.Add("address");
               options.Scope.Add("roles");
               options.SaveTokens = true;
               options.ClientSecret = "secret";
               options.GetClaimsFromUserInfoEndpoint = true;
               //Allow Add, Change, Remove Claims filters 
               //Remove Filter for amr to include it in claims
               options.ClaimActions.Remove("amr");
               //Add filter which delete claim sid/idp in claims
               options.ClaimActions.DeleteClaim("sid");
               options.ClaimActions.DeleteClaim("idp");
           });

            // my services
            services.AddScoped<IImageGalleryHttpClient, ImageGalleryHttpClient>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
            }

            //add before mvc -> want to block request 4 unauth. users
            app.UseAuthentication();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Gallery}/{action=Index}/{id?}");
            });
        }
    }
}
