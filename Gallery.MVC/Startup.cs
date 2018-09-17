using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Gallery.Logic.DataAccess;
using Gallery.MVC.GalleryResources;
using Google.Apis.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NJsonSchema;
using NSwag.AspNetCore;

namespace Gallery.MVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddAuthentication(TheAppContext.AuthCookieScheme)
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromDays(10 * 365);
                });

            services
                .AddMvc()
                .AddJsonOptions(options =>
                    options.SerializerSettings.ContractResolver = new NewtonsoftJsonContractResolver());

            services.AddLogging();
            services.AddSingleton<Custom_PreJit_On_Startup>();
            services.AddSingleton<ContentManager>();
            services.AddTransient<PhotosRepository>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseSwaggerUi(typeof(Startup).GetTypeInfo().Assembly, settings =>
            {
                settings.GeneratorSettings.DefaultPropertyNameHandling =
                    PropertyNameHandling.CamelCase;
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "gallery",
                    template: "{controller=Gallery}/{action=Index}/{id?}");

            });


            var preJit = app.ApplicationServices.GetRequiredService<Custom_PreJit_On_Startup>();
            preJit.Perform();

        }
    }
}
