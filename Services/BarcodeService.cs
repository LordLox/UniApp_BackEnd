using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QRCoder;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public class BarcodeService(IDbContextFactory<ApplicationContext> dbContextFactory, IOptions<Settings> options, IMapper mapper)
{
    private readonly IDbContextFactory<ApplicationContext> dbContextFactory = dbContextFactory;
    private readonly IOptions<Settings> options = options;
    private readonly IMapper mapper = mapper;

    // Generates a QR code for a given user ID
    public async Task<BarcodeReturnDto> GenerateQrCodeAsync(int id)
    {
        var bcodeReturn = new BarcodeReturnDto();
        var context = await dbContextFactory.CreateDbContextAsync();
        var user = await context.Users.SingleAsync(x => x.Id == id);
        var bcodeData = mapper.Map<BarcodeDataDto>(user);
        bcodeData.Epoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var jsonBcodeData = JsonSerializer.Serialize(bcodeData);

        // Encrypt the plain text
        bcodeReturn.EncryptedBarcodeValue = Encrypt(jsonBcodeData);

        user.QrCodeB64 = bcodeReturn.EncryptedBarcodeValue;
        user.QrCodeGeneratedAt = DateTimeOffset.FromUnixTimeMilliseconds(bcodeData.Epoch).UtcDateTime;

        // Generate QR code
        using QRCodeGenerator qrGenerator = new();
        using QRCodeData qrCodeData = qrGenerator.CreateQrCode(bcodeReturn.EncryptedBarcodeValue, QRCodeGenerator.ECCLevel.Q);
        using PngByteQRCode qrCode = new(qrCodeData);
        byte[] qrCodeImage = qrCode.GetGraphic(20);

        await context.SaveChangesAsync();

        // Return QR code as a base64 string
        bcodeReturn.BarcodeCodeImage = qrCodeImage;
        return bcodeReturn;
    }

    // Encrypts a plain text string
    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(options.Value.AESKey);
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }
        var iv = aes.IV;
        var encryptedContent = ms.ToArray();
        var result = new byte[iv.Length + encryptedContent.Length];
        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(encryptedContent, 0, result, iv.Length, encryptedContent.Length);
        return Convert.ToBase64String(result);
    }

    // Asynchronous wrapper for Encrypt method
    public async Task<string> EncryptAsync(string plainText)
    {
        return await Task.FromResult(Encrypt(plainText));
    }

    // Decrypts an encrypted string
    public string Decrypt(string encryptedText)
    {
        var fullCipher = Convert.FromBase64String(encryptedText);
        using var aes = Aes.Create();
        var iv = new byte[aes.BlockSize / 8];
        var cipher = new byte[fullCipher.Length - iv.Length];
        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);
        aes.Key = Encoding.UTF8.GetBytes(options.Value.AESKey);
        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(cipher);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        var decryptedText = sr.ReadToEnd();
        return decryptedText;
    }

    // Asynchronous wrapper for Decrypt method
    public async Task<string> DecryptAsync(string encryptedText)
    {
        return await Task.FromResult(Decrypt(encryptedText));
    }

    // Validates a barcode string
    public async Task<BarcodeDataDto> ValidateBarcodeAsync(string barcodeStringB64)
    {
        var decryptedBarcode = await DecryptAsync(barcodeStringB64);
        var userData = JsonSerializer.Deserialize<BarcodeDataDto>(decryptedBarcode)!;

        var context = await dbContextFactory.CreateDbContextAsync();
        var user = await context.Users.SingleAsync(x => x.Id == userData.Id);
        var scannedDate = DateTimeOffset.FromUnixTimeMilliseconds(userData.Epoch).UtcDateTime;

        var curDate = DateTime.UtcNow;

        if (user.QrCodeGeneratedAt != scannedDate || curDate <= scannedDate || curDate >= scannedDate.AddSeconds(options.Value.BCodeElapseSeconds))
        {
            throw new Exception("Barcode is expired");
        }

        return userData;
    }
}