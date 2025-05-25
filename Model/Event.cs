using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Events")]
public class Event : IEntityTypeConfiguration<Event>
{
    public int Id { get; set; }
    [Required, StringLength(255)]
    public string Name { get; set; } = string.Empty;
    public EventType Type { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public List<EventUser> EventsUsers { get; set; } = [];

    // Configure the Entity Framework model
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.Name, x.UserId }).IsUnique();
        builder.HasOne(x => x.User).WithMany(x => x.Events).HasForeignKey(x => x.UserId);
    }
}