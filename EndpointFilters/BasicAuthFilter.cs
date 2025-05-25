using Mysqlx;

public class BasicAuthFilter(UsersService usersService, List<UserType> userTypes, IWebHostEnvironment environment) : IEndpointFilter
{
    private readonly UsersService _usersService = usersService;
    private readonly List<UserType> userTypes = userTypes;
    private readonly IWebHostEnvironment environment = environment;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        try
        {
            // Extract and validate the authentication header
            var authDto = httpContext.Request.GetBasicAuthenticationHeader();

            if (await _usersService.Authenticate(authDto))
            {
                // If no role specified, then it's a public endpoint
                if (userTypes.Count == 0)
                {
                    return await next(context);
                }
                var user = await _usersService.GetUserAsync(authDto.Username);
                // Check if the user has the required role
                if (!userTypes.Contains(user!.Type) && user!.Type != UserType.Admin)
                {
                    return Results.StatusCode(403);
                }
                return await next(context);
            }
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            // Return detailed error information in development environment
            if (environment.IsDevelopment())
                return Results.BadRequest(new ErrorOutput { Message = ex.Message, StackTrace = ex.StackTrace });
            else
                return Results.BadRequest(ex.Message);
        }

        return Results.Unauthorized();
    }
}

// Class to hold error information for development environment
class ErrorOutput
{
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; } = string.Empty;
}