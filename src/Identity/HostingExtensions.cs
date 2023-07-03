using DotNetFlix.Identity.Data;
using DotNetFlix.Identity.Models;
using DotNetFlix.Identity.Services;
using Duende.IdentityServer;
using Duende.IdentityServer.Services;
using Fido2NetLib;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using System.Configuration;

namespace DotNetFlix.Identity;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        
        builder.Services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                options.EmitStaticAudienceClaim = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryApiResources(Config.ApiResources)
            .AddInMemoryClients(Config.Clients)
            .AddAspNetIdentity<ApplicationUser>();
        
        builder.Services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                
                // register your IdentityServer with Google at https://console.developers.google.com
                // enable the Google+ API
                // set the redirect URI to https://localhost:5001/signin-google
                options.ClientId = "copy client ID from Google here";
                options.ClientSecret = "copy client secret from Google here";
            });

        builder.Services.AddScoped<IFidoCredentialStore, FidoCredentialStore>();

        var fidoConfig = new Fido2Configuration();

        builder.Configuration.Bind(nameof(Fido2Configuration), fidoConfig);
        
        builder.Services.AddFido2(options =>
        {
            options.ServerDomain = fidoConfig.ServerDomain;
            options.ServerName = fidoConfig.ServerName;
            options.Origins = fidoConfig.Origins;
            options.TimestampDriftTolerance = fidoConfig.TimestampDriftTolerance;
            options.MDSCacheDirPath = fidoConfig.MDSCacheDirPath;
            options.BackupEligibleCredentialPolicy = fidoConfig.BackupEligibleCredentialPolicy;
            options.BackedUpCredentialPolicy = fidoConfig.BackupEligibleCredentialPolicy;
        })
        .AddCachedMetadataService(config =>
        {
            config.AddFidoMetadataRepository(httpClientBuilder =>
            {
                //TODO: any specific config you want for accessing the MDS
            });
        });

        var userCodeDescriptor = ServiceDescriptor.Transient<IUserCodeService, CustomUserCodeService>();
        builder.Services.Replace(userCodeDescriptor);

        builder.Services.AddScoped<IEmailSender, EmailService>();

        builder.Services.AddCors(opt =>
        {
            opt.AddPolicy("CorsPolicy", policy =>
            {
                policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:7009", "https://ambitious-meadow-0ec297a00.3.azurestaticapps.net");
            });
        });

        return builder.Build();
    }
    
    public static WebApplication ConfigurePipeline(this WebApplication app)
    { 
        app.UseSerilogRequestLogging();
    
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors("CorsPolicy");
        app.UseIdentityServer();
        app.UseAuthorization();
        
        app.MapRazorPages()
            .RequireAuthorization();

        return app;
    }
}