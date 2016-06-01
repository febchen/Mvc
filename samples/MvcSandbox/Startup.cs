// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Antiforgery;
using System.IO;
using System.Reflection;
#if testOnly && NETCOREAPP1_0
using System.Runtime.Loader;
#endif
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MvcSandbox
{
    public class Startup
    {
#if testOnly
        private static readonly Assembly _assembly;
#endif

        public static Type ExternalType { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
#if testOnly
#if NETCOREAPP1_0
            var path = "C:\\dd\\dnx\\MVC\\samples\\ClassLibrary\\bin\\Debug\\netstandard1.3\\ClassLibrary.dll";
            _assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
#else
            var path = "C:\\dd\\dnx\\MVC\\samples\\ClassLibrary\\bin\\Debug\\net451\\ClassLibrary.dll";
            _assembly = Assembly.LoadFrom(path);
#endif

            ExternalType = _assembly.GetType("ClassLibrary.Class1");
#else
            ExternalType = Type.GetType("ClassLibrary.Class1,ClassLibrary");
#endif

            services.AddMvc().AddCookieTempDataProvider();

            services.Configure<RazorViewEngineOptions>(options => options.AdditionalCompilationReferences.Add(
                MetadataReference.CreateFromFile(ExternalType.GetTypeInfo().Assembly.Location)));

            services.Insert(0, ServiceDescriptor.Singleton(
                typeof(IConfigureOptions<AntiforgeryOptions>),
                new ConfigureOptions<AntiforgeryOptions>(options => options.CookieName = "<choose a name>")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            loggerFactory
                .AddConsole()
                .AddDebug();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}