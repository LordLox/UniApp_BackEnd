using BackEnd;
using Microsoft.AspNetCore.Mvc;
using System.Text;

public static class BarcodeController
{
    public static void AddBarcodeController(this WebApplication app)
    {
        var globals = app.Services.CreateScope().ServiceProvider.GetRequiredService<Globals>();

        // Generate QR code for a specific user
        app.MapGet("/barcode/qr/{userid}", async (
            int userid,
            HttpContext context,
            [FromServices] BarcodeService barcodeService) =>
        {
            var contentType = context.Request.ContentType;
            var qrcode = await barcodeService.GenerateQrCodeAsync(userid);
            switch (contentType)
            {
                case "text/plain":
                    return Results.Text(qrcode.EncryptedBarcodeValue);
                default:
                    var filename = CryptoExtensions.RandomString(16) + ".png";
                    return Results.File(qrcode.BarcodeCodeImage, MimeTypes.GetMimeType(filename), filename);
            }
        })
        .WithTags("Barcode")
        .AddEndpointFilter(globals.ProfessorAuth);

        // Generate QR code for the authenticated user
        app.MapGet("/barcode/qr", async (
            HttpContext context,
            [FromServices] BarcodeService barcodeService,
            [FromServices] UsersService usersService) =>
        {
            var user = await usersService.GetUserFromAuthAsync(context.Request)
                ?? throw new UnauthorizedAccessException();

            var contentType = context.Request.ContentType;
            var qrcode = await barcodeService.GenerateQrCodeAsync(user.Id);
            switch (contentType)
            {
                case "text/plain":
                    return Results.Text(qrcode.EncryptedBarcodeValue);
                default:
                    var filename = CryptoExtensions.RandomString(16) + ".png";
                    return Results.File(qrcode.BarcodeCodeImage, MimeTypes.GetMimeType(filename), filename);
            }

        })
        .WithTags("Barcode")
        .AddEndpointFilter(globals.StudentAuth);

        // Encrypt a given string
        app.MapPost("/barcode/encrypt", async (HttpContext context, [FromServices] BarcodeService barcodeService) =>
        {
            var body = await context.Request.ReadRequestRawBodyAsync();

            return await barcodeService.EncryptAsync(body.Trim());
        })
        .WithTags("Barcode")
        .Accepts<string>("text/plain");

        // Decrypt a given string
        app.MapPost("/barcode/decrypt", async (HttpContext context, [FromServices] BarcodeService barcodeService) =>
        {
            var body = await context.Request.ReadRequestRawBodyAsync();

            return await barcodeService.DecryptAsync(body.Trim());
        })
        .WithTags("Barcode")
        .Accepts<string>("text/plain");

        // Validate a barcode
        app.MapPost("/barcode/validate", async (
            HttpContext context,
            [FromServices] BarcodeService barcodeService) =>
        {
            var body = await context.Request.ReadRequestRawBodyAsync();
            try
            {
                await barcodeService.ValidateBarcodeAsync(body.Trim());
            }
            catch (Exception e)
            {
                return Results.BadRequest(e.Message);
            }
            return Results.Ok();
        })
        .WithTags("Barcode")
        .AddEndpointFilter(globals.AdminAuth);
    }
}