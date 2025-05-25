using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Users")]
public class User : IEntityTypeConfiguration<User>
{
    public int Id { get; set; }
    [Required, StringLength(255)]
    public string Name { get; set; } = string.Empty;
    public int Badge { get; set; }
    [Required, StringLength(255)]
    public string Username { get; set; } = string.Empty;
    public UserType Type { get; set; }
    public string HashedPassword { get; set; } = string.Empty;
    public byte[] PasswordSalt { get; set; } = [];
    public string QrCodeB64 { get; set; } = string.Empty;
    public DateTime QrCodeGeneratedAt { get; set; }
    public List<Event> Events { get; set; } = [];
    public List<EventUser> EventUsers { get; set; } = [];

    // Configure the Entity Framework model
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasAlternateKey(x => x.Badge);
        builder.HasAlternateKey(x => x.Username);
        builder.HasMany(x => x.Events).WithOne(x => x.User).HasForeignKey(x => x.UserId);
    }
}