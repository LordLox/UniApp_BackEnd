// Data Transfer Object for event history
public class HistoryDto
{
    public string EventName { get; set; } = string.Empty;
    public string UserBirthName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int UserBadge { get; set; }
    public DateTime EventEntryDate { get; set; }
}