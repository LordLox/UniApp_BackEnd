using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using System.Text.Json;

public static class EventsController
{
    public static void AddEventsController(this WebApplication app)
    {
        var globals = app.Services.CreateScope().ServiceProvider.GetRequiredService<Globals>();

        // Get all events (Admin only)
        app.MapGet("/events", async ([FromServices] EventsService eventsService) =>
        {
            var events = await eventsService.GetAllEventsAsync();
            if (events.Count == 0)
                return Results.NotFound();
            return Results.Ok(events);
        })
        .WithTags("Events")
        .AddEndpointFilter(globals.AdminAuth);

        // Get events by type (Admin only)
        app.MapGet("/events/type/{type}", async (EventType type, [FromServices] EventsService eventsService) =>
        {
            var events = await eventsService.GetAllEventsByTypeAsync(type);
            if (events.Count == 0)
                return Results.NotFound();
            return Results.Ok(events);
        })
        .WithTags("Events")
        .AddEndpointFilter(globals.AdminAuth);

        // Get events for a specific user (Admin only)
        app.MapGet("/events/personal/{userId}", async (int userId, [FromServices] EventsService eventsService) =>
        {
            var events = await eventsService.GetEventsByUserAsync(userId);
            if (events.Count == 0)
                return Results.NotFound();
            return Results.Ok(events);
        })
        .WithTags("Events")
        .AddEndpointFilter(globals.AdminAuth);

        // Get events for the authenticated user (Professor only)
        app.MapGet("/events/personal", async (
            HttpContext context,
            [FromServices] EventsService eventsService,
            [FromServices] UsersService usersService) =>
        {
            var user = await usersService.GetUserFromAuthAsync(context.Request)
                ?? throw new UnauthorizedAccessException();

            var events = await eventsService.GetEventsByUserAsync(user.Id);
            if (events.Count == 0)
                return Results.NotFound();
            return Results.Ok(events);
        })
        .WithTags("Events")
        .AddEndpointFilter(globals.ProfessorAuth);

        // Get a specific event (Professor only)
        app.MapGet("/events/{id}", async (int id, [FromServices] EventsService eventsService) =>
        {
            var user = await eventsService.GetEventAsync(id);
            if (user == null)
                return Results.NotFound();
            return Results.Ok(user);
        })
        .WithTags("Events")
        .AddEndpointFilter(globals.ProfessorAuth);

        // Create a new event (Professor only)
        app.MapPost("/events", async (
            [FromBody] EventCreateDto newEvent,
            HttpContext context,
            [FromServices] EventsService eventsService,
            [FromServices] UsersService usersService) =>
        {
            var user = await usersService.GetUserFromAuthAsync(context.Request)
                ?? throw new UnauthorizedAccessException();

            newEvent.UserId = user.Id;
            var newEventId = await eventsService.CreateEventAsync(newEvent);
            return Results.Created($"/events/{newEventId}", newEventId);
        })
        .WithTags("Events")
        .AddEndpointFilter(globals.ProfessorAuth);

        // Update an existing event (Professor only)
        app.MapPatch("/events/{id}", async (int id,
            HttpContext context,
            [FromBody] EventUpdateDto editedEvent,
            [FromServices] EventsService eventsService,
            [FromServices] UsersService usersService) =>
        {
            var user = await usersService.GetUserFromAuthAsync(context.Request)
                ?? throw new UnauthorizedAccessException();

            await eventsService.UpdateEventAsync(id, editedEvent, user);
        })
        .WithTags("Events")
        .AddEndpointFilter(globals.ProfessorAuth);

        // Delete an event (Professor only)
        app.MapDelete("/events/{id}", async (int id, [FromServices] EventsService eventsService) =>
        {
            await eventsService.DeleteEventAsync(id);
        })
        .WithTags("Events")
        .AddEndpointFilter(globals.ProfessorAuth);

        // Add a user to an event (Professor only)
        app.MapPost("/events/entry/{eventId}", async (
            int eventId,
            HttpContext context,
            [FromServices] BarcodeService barcodeService,
            [FromServices] EventsService eventsService) =>
        {
            var body = await context.Request.ReadRequestRawBodyAsync();
            try
            {
                var bcodeData = await barcodeService.ValidateBarcodeAsync(body.Trim());
                await eventsService.AddUserToEventAsync(bcodeData.Id, eventId);
            }
            catch (Exception e)
            {
                return Results.BadRequest(e.Message);
            }
            return Results.Created();
        })
        .WithTags("Events")
        .AddEndpointFilter(globals.ProfessorAuth);

        // Get event entry history (Professor only)
        app.MapGet("/events/entry/history", async (
            HttpContext context,
            [FromServices] EventsService eventsService,
            [FromServices] UsersService usersService) =>
        {
            var user = await usersService.GetUserFromAuthAsync(context.Request)
                ?? throw new UnauthorizedAccessException();

            var contentType = context.Request.ContentType;
            var timezone = context.Request.Headers["Timezone"].ToString();
            var culture = context.Request.Headers["Culture"].ToString();
            var startTs = long.Parse(context.Request.Headers["StartTimestamp"].ToString());
            var endTs = long.Parse(context.Request.Headers["EndTimestamp"].ToString());
            var history = await eventsService.GetEventsHistoryByUserAsync(user.Id, startTs, endTs, timezone, culture);
            switch (contentType)
            {
                case "text/csv":
                    var separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator == ","
                        ? ConvertJsonToCSV.Main.CSVSeperator.COMMA
                        : ConvertJsonToCSV.Main.CSVSeperator.SEMICOLON;
                    var historyCsv = ConvertJsonToCSV.Main.Convert(JsonSerializer.Serialize(history), separator);
                    return Results.Text(historyCsv);
                default:
                    return Results.Ok(history);
            }
        })
        .WithTags("Events")
        .AddEndpointFilter(globals.ProfessorAuth);

        app.MapGet("/me/event-history", async (
            HttpContext context,
            [FromServices] EventsService eventsService,
            [FromServices] UsersService usersService) =>
        {
            var studentUser = await usersService.GetUserFromAuthAsync(context.Request)
                ?? throw new UnauthorizedAccessException("User not authenticated.");

            // Ensure this endpoint is only for students
            if (studentUser.Type != UserType.Student)
            {
                return Results.Forbid();
            }

            // Timestamps and culture/timezone info from headers, similar to professor's endpoint
            var timezone = context.Request.Headers["Timezone"].ToString();
            var culture = context.Request.Headers["Culture"].ToString();

            if (!long.TryParse(context.Request.Headers["StartTimestamp"].ToString(), out long startTs) ||
                !long.TryParse(context.Request.Headers["EndTimestamp"].ToString(), out long endTs))
            {
                return Results.BadRequest("Invalid StartTimestamp or EndTimestamp header.");
            }

            var history = await eventsService.GetStudentOwnParticipationHistoryAsync(studentUser.Id, startTs, endTs, timezone, culture);

            if (!history.Any())
            {
                return Results.Ok(new List<HistoryDto>()); // Return empty list instead of 404 for no history
            }
            return Results.Ok(history);
        })
        .WithTags("Me") // New tag for "current user" specific endpoints
        .WithName("GetMyEventHistory")
        .AddEndpointFilter(globals.StudentAuth); // Secure for Students only
    }
}