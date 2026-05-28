using DaiQuanGia.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DaiQuanGia.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Override tên bảng từ "AspNetUsers" về "users"
        builder.ToTable("users");

        // Chỉ config các field TỰ THÊM
        // Id, Email, PasswordHash, UserName... do IdentityUser quản lý

        builder.Property(x => x.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Timezone)
            .HasColumnName("timezone")
            .HasMaxLength(100)
            .HasDefaultValue("Asia/Saigon")
            .IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasMany(x => x.RefreshTokens)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
