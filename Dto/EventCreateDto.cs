// Data Transfer Object for creating a new event
public class EventCreateDto
{
    public string Name { get; set; } = string.Empty;
    public int UserId { get; set; }  // ID of the user creating the event
    public EventType Type { get; set; }
}