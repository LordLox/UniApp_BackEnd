// Data Transfer Object for updating an existing event
public class EventUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public EventType Type { get; set; }
}