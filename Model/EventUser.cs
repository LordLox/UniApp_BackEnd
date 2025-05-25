using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

[Table("EventUser")]
public class EventUser : IEntityTypeConfiguration<EventUser>
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    public DateTime EntryDate { get; set; }

    // Configure the Entity Framework model
    public void Configure(EntityTypeBuilder<EventUser> builder)
    {
        builder.HasKey(x => new { x.UserId, x.EventId });
        builder.HasOne(x => x.User).WithMany(x => x.EventUsers).HasForeignKey(x => x.UserId);
        builder.HasOne(x => x.Event).WithMany(x => x.EventsUsers).HasForeignKey(x => x.EventId);
    }
}