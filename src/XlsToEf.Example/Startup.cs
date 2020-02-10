using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Scrutor;
using XlsToEf.Example.Controllers;
using XlsToEf.Example.Domain;
using XlsToEf.Example.ExampleCustomMapperField;
using XlsToEf.Example.Infrastructure;
using XlsToEf.Import;

namespace XlsToEf.Example
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddMvc();
                      services.AddScoped<DbContext, XlsToEfDbContext>(m => m.GetService<XlsToEfDbContext>());
            services.AddScoped(m => new XlsToEfDbContext(Configuration["Data:DefaultConnection:ConnectionString"]));
            services.AddMediatR(typeof (HomeController).GetTypeInfo().Assembly);
            services.AddScoped<ProductPropertyOverrider<Product>>();
            services.Scan(scan => scan
                .FromAssemblyOf<Address>()
                .AddClasses(x => x.Where(y => !y.IsAssignableFrom(typeof (XlsToEfDbContext))))
                .AddClasses(classes => classes.AssignableTo(typeof (UpdatePropertyOverrider<>)))
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

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConfiguration(Configuration.GetSection("Logging"));
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });

            foreach (var service in services)
            {
                Debug.WriteLine(service.ServiceType + " - " + service.ImplementationType);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
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
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "Content")),
                RequestPath = "/Content"
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
