using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XlsToEf.Core.Example.Domain;
using XlsToEf.Core.Example.ExampleCustomMapperField;
using XlsToEf.Core.Example.Infrastructure;
using XlsToEf.Core.Import;

namespace XlsToEf.Core.Example
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddScoped<DbContext, XlsToEfDbContext>(m => m.GetService<XlsToEfDbContext>());
            services.AddDbContext<XlsToEfDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddMediatR(typeof(Startup));
            services.AddScoped<ProductPropertyOverrider<Product>>();
            services.Scan(scan => scan
                .FromAssemblyOf<Address>()
                .AddClasses(x => x.Where(y => !y.IsAssignableFrom(typeof(XlsToEfDbContext))))
                .AddClasses(classes => classes.AssignableTo(typeof(UpdatePropertyOverrider<>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime()
            );
            services.Scan(scan => scan
                .FromAssemblyOf<XlsxToTableImporter>()
                .AddClasses()
                .AsImplementedInterfaces()
                .WithTransientLifetime()
            );
            services.Scan(scan => scan
                .FromAssemblyOf<XlsxToTableImporter>()
                .AddClasses()
                .AsSelf()
                .WithTransientLifetime()
            );

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

 
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
