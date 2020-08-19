using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using CodeJar.Domain;
using CodeJar.Infrastructure;
using CodeJar.Infrastructure.Guids;
using CodeJar.ServiceBusAzure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CodeJar.CronJob;

namespace CodeJar.WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        readonly string MyAllowSpecificOrigins = "Policy";

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("Policy", builder => builder
                    .WithOrigins("http://localhost:5002")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                );
            });

            services.AddControllers();

            services.AddSingleton<IQueueClient, QueueClient>(_ => new QueueClient(Configuration.GetConnectionString("AzureServiceBus"), "codejar"));
            services.AddSingleton<ISequentialGuidGenerator, SequentialGuidGenerator>();

            services.AddScoped<SqlConnection>(_ => new SqlConnection(Configuration.GetConnectionString("Storage")));
            services.AddScoped<IBatchRepository, SqlBatchRepository>();
            services.AddScoped<ICodeRepository, SqlCodeRepository>();
            services.AddScoped<PaginationCount>();

            services.AddHostedService<ReceiveServiceBus>();

            services.AddCronJob<CodeActivationCronJob>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                c.CronExpression = @"* * * * *";
            });

            services.AddCronJob<CodeExpirationCronJob>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                c.CronExpression = @"* * * * *";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(MyAllowSpecificOrigins);

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
