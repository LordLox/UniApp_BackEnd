public class Settings
{
    // AES encryption key used for secure operations
    public string AESKey { get; set; } = string.Empty;

    // Time in seconds for which a barcode remains valid
    public int BCodeElapseSeconds { get; set; } = 30;
}