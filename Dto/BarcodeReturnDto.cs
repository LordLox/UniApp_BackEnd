// Data Transfer Object for returning barcode information
public class BarcodeReturnDto
{
    public string EncryptedBarcodeValue { get; set; } = string.Empty;
    public byte[] BarcodeCodeImage { get; set; } = [];  // QR code image as byte array
}