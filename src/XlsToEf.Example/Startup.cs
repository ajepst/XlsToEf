using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scrutor;
using XlsToEf.Example.Domain;
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
            services.AddScoped<DbContext, XlsToEfDbContext>();
            services.AddScoped(m => new XlsToEfDbContext("XlsToEf"));

            services.Scan(scan => scan
                // We start out with all types in the assembly of ITransientService
                .FromAssemblyOf<Address>()
                    // AddClasses starts out with all public, non-abstract types in this assembly.
                    // These types are then filtered by the delegate passed to the method.
                    // In this case, we filter out only the classes that are assignable to ITransientService
                    .AddClasses()
                        // Whe then specify what type we want to register these classes as.
                        // In this case, we wan to register the types as all of its implemented interfaces.
                        // So if a type implements 3 interfaces; A, B, C, we'd end up with three separate registrations.
                        .AsImplementedInterfaces()
                        // And lastly, we specify the lifetime of these registrations.
                        .WithTransientLifetime()
// Here we start again, with a new full set of classes from the assembly above.
// This time, filtering out only the classes assignable to IScopedService.
//                    .AddClasses(classes => classes.AssignableTo<DbContext>())
//                        // Now, we just want to register these types as a single interface, IScopedService.
//                        .As<XlsToEfDbContext>()
//                        // And again, just specify the lifetime.
//                        .WithScopedLifetime()
                .AddClasses(classes => classes.AssignableTo(typeof(UpdatePropertyOverrider<>)))
                                    .AsImplementedInterfaces()
                                    .WithTransientLifetime()
                        );
            services.Scan(scan => scan
                .FromAssemblyOf<XlsxToTableImporter>()
                       .AddClasses()
                        .AsImplementedInterfaces()
                        .WithScopedLifetime());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
