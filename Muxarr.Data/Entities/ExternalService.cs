using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Muxarr.Core.Config;

namespace Muxarr.Data.Entities;

public enum ExternalServiceType
{
    Sonarr = 0,
    Radarr = 1
}

public class ExternalService : AuditableEntity, IApiCredentials
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ExternalServiceType Type { get; set; }
    public string Url { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;

    public List<MediaInfo> MediaInfos { get; set; } = [];
}

public class ExternalServiceConfiguration : AuditEntityConfiguration<ExternalService>
{
    public override void Configure(EntityTypeBuilder<ExternalService> builder)
    {
        base.Configure(builder);

        builder.ToTable(nameof(ExternalService));

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Type)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.ApiKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasMany(e => e.MediaInfos)
            .WithOne(e => e.ExternalService)
            .HasForeignKey(e => e.ExternalServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
