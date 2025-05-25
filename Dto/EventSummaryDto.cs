public class EventSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public EventType Type { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<ParticipantSummaryDto> Participants { get; set; } = [];
}