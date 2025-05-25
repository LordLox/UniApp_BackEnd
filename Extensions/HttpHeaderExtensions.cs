using System.Text;

public static class HttpHeaderExtensions
{
    // Extract and parse the Basic Authentication header from an HTTP request
    public static AuthDto GetBasicAuthenticationHeader(this HttpRequest request)
    {
        if (!request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            throw new Exception("No authorization header");
        }
        var authHeader = authorizationHeader.ToString();
        if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("Invalid authorization header");
        }

        var token = authHeader.Substring("Basic ".Length).Trim();
        var credentialString = Encoding.UTF8.GetString(Convert.FromBase64String(token));
        var credentials = credentialString.Split(':');

        if (credentials.Length > 2)
        {
            throw new Exception("Invalid credentials");
        }

        var authDto = new AuthDto
        {
            Username = credentials[0],
            Password = credentials[1]
        };

        return authDto;
    }

    // Read the raw body of an HTTP request as a string
    public async static Task<string> ReadRequestRawBodyAsync(this HttpRequest request)
    {
        using var reader = new StreamReader(request.Body, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}