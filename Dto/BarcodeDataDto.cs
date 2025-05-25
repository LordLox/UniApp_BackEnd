// Data Transfer Object for barcode data
public class BarcodeDataDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Badge { get; set; }
    public long Epoch { get; set; }  // Timestamp for barcode generation
}