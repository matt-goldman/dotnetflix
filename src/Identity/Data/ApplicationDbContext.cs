using DotNetFlix.Identity.Models;
using DotNetFlix.Identity.Models.Converters;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DotNetFlix.Identity.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<FidoUser> FidoUsers { get; set; }
    public DbSet<FidoStoredCredential> FidoStoredCredentials { get; set; }
    public DbSet<PublicKeyCredentialDescriptor> FidoPublicKeyDescriptors { get; set; }
    

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
        
        builder.Entity<PublicKeyCredentialDescriptor>()
            .Property(e => e.Transports)
            .HasConversion(new AuthenticatorTransportArrayConverter());

        builder.Entity<PublicKeyCredentialDescriptor>()
        .HasKey(d => d.Id);

        builder.Entity<PublicKeyCredentialDescriptor>()
            .Property(d => d.Id)
            .HasColumnType("varbinary(MAX)");
    }
}
