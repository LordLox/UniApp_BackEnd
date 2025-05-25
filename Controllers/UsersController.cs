using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

public static class UsersController
{
    public static void AddUsersController(this WebApplication app)
    {
        var globals = app.Services.CreateScope().ServiceProvider.GetRequiredService<Globals>();

        // Get all users (Admin only)
        app.MapGet("/users", async ([FromServices] UsersService usersService) =>
        {
            var users = await usersService.GetAllUsersAsync();
            if (users.Count == 0)
                return Results.NotFound();
            return Results.Ok(users);
        })
        .WithTags("Users")
        .AddEndpointFilter(globals.AdminAuth);

        // Get a specific user by ID (Admin only)
        app.MapGet("/users/{id}", async (int id, [FromServices] UsersService usersService) =>
        {
            var user = await usersService.GetUserAsync(id);
            if (user == null)
                return Results.NotFound();
            return Results.Ok(user);
        })
        .WithTags("Users")
        .AddEndpointFilter(globals.AdminAuth);

        // Create a new user (Admin only)
        app.MapPost("/users", async ([FromBody] UserCreateDto newUser, [FromServices] UsersService usersService) =>
        {
            var userId = await usersService.CreateUserAsync(newUser);
            return Results.Created($"/users/{userId}", userId);
        })
        .WithTags("Users")
        .AddEndpointFilter(globals.AdminAuth);

        // Change password for the authenticated user
        app.MapPost("/users/changepass", async (HttpContext context, [FromServices] UsersService usersService) =>
        {
            var user = await usersService.GetUserFromAuthAsync(context.Request)
                ?? throw new UnauthorizedAccessException();

            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
            var body = await reader.ReadToEndAsync();
            try
            {
                await usersService.ChangePasswordAsync(user.Id, body.Trim());
            }
            catch (PasswordTooWeakException e)
            {
                return Results.BadRequest(e.Message);
            }

            return Results.Ok();
        })
        .WithTags("Users")
        .Produces<string>(400, "text/plain")
        .Produces<string>(200, "text/plain")
        .AddEndpointFilter(globals.AllAuth);

        // Change password for a specific user (Admin only)
        app.MapPost("/users/changepass/{userId}", async (int userId, HttpContext context, [FromServices] UsersService usersService) =>
        {
            var body = await context.Request.ReadRequestRawBodyAsync();
            try
            {
                await usersService.ChangePasswordAsync(userId, body.Trim());
            }
            catch (PasswordTooWeakException e)
            {
                return Results.BadRequest(e.Message);
            }

            return Results.Ok();
        })
        .WithTags("Users")
        .AddEndpointFilter(globals.AdminAuth);

        // Update a user (Admin only)
        app.MapPatch("/users/{id}", async (int id, [FromBody] UserUpdateDto editedUser, [FromServices] UsersService usersService) =>
        {
            await usersService.UpdateUserAsync(id, editedUser);
        })
        .WithTags("Users")
        .AddEndpointFilter(globals.AdminAuth);

        // Delete a user (Admin only)
        app.MapDelete("/users/{id}", async (int id, [FromServices] UsersService usersService) =>
        {
            await usersService.DeleteUserAsync(id);
        })
        .WithTags("Users")
        .AddEndpointFilter(globals.AdminAuth);

        // Validate user credentials (Admin only, for testing purposes)
        app.MapPost("/users/validatecreds", async ([FromBody] AuthDto auth, [FromServices] UsersService usersService) =>
        {
            if (!await usersService.Authenticate(auth))
                return Results.Unauthorized();
            return Results.Ok();
        })
        .WithTags("Auth")
        .AddEndpointFilter(globals.AdminAuth);

        // Check user role (Admin only, for testing purposes)
        app.MapPost("/users/checkrole", async ([FromBody] AuthDto auth, [FromServices] UsersService usersService) =>
        {
            if (!await usersService.Authenticate(auth))
                return Results.Unauthorized();
            return Results.Ok();
        })
        .WithTags("Auth")
        .AddEndpointFilter(globals.AdminAuth);

        // Get encrypted user info for the authenticated user
        app.MapGet("/users/userinfo", async (HttpContext context, [FromServices] UsersService usersService, [FromServices] BarcodeService barcodeService) =>
        {
            var userInfo = await usersService.GetUserInfoFromAuthAsync(context.Request)
                ?? throw new UnauthorizedAccessException();

            return Results.Text(barcodeService.Encrypt(JsonSerializer.Serialize(userInfo)));
        })
        .WithTags("Auth")
        .AddEndpointFilter(globals.AllAuth);
    }
}