using DotNetFlix.Identity.Models;
using DotNetFlix.Identity.Models.Converters;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace DotNetFlix.Identity.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<FidoUser> FidoUsers { get; set; }
    public DbSet<FidoStoredCredential> FidoStoredCredentials { get; set; }
    public DbSet<FidoPublicKeyDescriptor> FidoPublicKeyDescriptors { get; set; }
    

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);

        // FidoUser Configuration
        builder.Entity<FidoUser>()
            .HasKey(u => u.UserId);

        builder.Entity<FidoUser>()
            .Property(u => u.UserId)
            .ValueGeneratedOnAdd();

        
        
        // FidoPublicKeyDescriptor configuration
        builder.Entity<FidoPublicKeyDescriptor>()
            .Property(e => e.Transports)
            .HasConversion(new AuthenticatorTransportArrayConverter());

        builder.Entity<FidoPublicKeyDescriptor>()
        .HasKey(d => d.Id);

        builder.Entity<FidoPublicKeyDescriptor>()
            .HasKey(u => u.DescriptorId);

        builder.Entity<FidoPublicKeyDescriptor>()
            .Property(u => u.DescriptorId)
            .ValueGeneratedOnAdd();



        // FidoStoredCredential configuration
        builder.Entity<FidoStoredCredential>()
            .Property(e => e.Transports)
            .HasConversion(new AuthenticatorTransportArrayConverter());

        builder.Entity<FidoStoredCredential>()
        .HasKey(d => d.Id);

        builder.Entity<FidoStoredCredential>()
           .Property(e => e.DevicePublicKeys)
           .HasConversion(
               v => ConvertToByteArray(v),
               v => ConvertToByteArrayList(v));

        builder.Entity<FidoStoredCredential>()
            .HasKey(u => u.CredentialId);

        builder.Entity<FidoStoredCredential>()
            .Property(u => u.CredentialId)
            .ValueGeneratedOnAdd();
    }

    private static byte[] ConvertToByteArray(List<byte[]> value)
    {
        if (value == null || value.Count == 0)
            return null;

        string json = JsonSerializer.Serialize(value);
        return Encoding.UTF8.GetBytes(json);
    }

    private static List<byte[]> ConvertToByteArrayList(byte[] value)
    {
        if (value == null)
            return null;

        string json = Encoding.UTF8.GetString(value);
        return JsonSerializer.Deserialize<List<byte[]>>(json);
    }


}
