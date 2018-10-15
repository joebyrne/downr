using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using downr.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.IO;
using Microsoft.AspNetCore.Rewrite;

namespace downr
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
            services.AddMvc();

            // add site services
            services.AddSingleton<IMarkdownContentLoader, DefaultMarkdownContentLoader>();
            services.AddSingleton<IYamlIndexer, DefaultYamlIndexer>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IYamlIndexer yamlIndexer)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            
            app.UseDeveloperExceptionPage();
                
            app.UseRewriter(
                new RewriteOptions()
                    .AddRewrite("posts/(.*(?<!a)$)(?<!jpg)(?<!png)", "$1", true)
            );

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse =
                    _ => _.Context.Response.Headers[HeaderNames.CacheControl] = 
                    "public,max-age=604800"  
            });

            app.UseMvc();

            if (string.IsNullOrWhiteSpace(env.WebRootPath))
            {
                env.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            // get the path to the content directory so the yaml headers can be indexed as metadata
            var contentPath = Path.Combine(env.WebRootPath, "posts");
            yamlIndexer.IndexContentFiles(contentPath);
        }
    }
}
