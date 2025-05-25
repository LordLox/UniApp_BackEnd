using System.Globalization;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

public class EventsService(IDbContextFactory<ApplicationContext> contextFactory, IMapper mapper)
{
    private readonly IDbContextFactory<ApplicationContext> contextFactory = contextFactory;
    private readonly IMapper mapper = mapper;

    #region CRUD
    // Creates a new event
    public async Task<int> CreateEventAsync(EventCreateDto newEvent)
    {
        var evt = mapper.Map<Event>(newEvent);

        var context = await contextFactory.CreateDbContextAsync();
        await context.Events.AddAsync(evt);
        await context.SaveChangesAsync();
        return evt.Id;
    }

    // Retrieves all events
    public async Task<List<EventSummaryDto>> GetAllEventsAsync()
    {
        var context = await contextFactory.CreateDbContextAsync();
        return await context.Events
                            .Include(e => e.User)
                            .Select(e => new EventSummaryDto
                            {
                                Id = e.Id,
                                Name = e.Name,
                                Type = e.Type,
                                UserId = e.UserId,
                                UserName = e.User.Name,
                                Participants = e.EventsUsers.Select(eu => new ParticipantSummaryDto
                                {
                                    Id = eu.UserId,
                                    UserName = eu.User.Username, // Student's username
                                    Name = eu.User.Name,   // Student's full name
                                    Badge = eu.User.Badge,   // Student's badge
                                    EntryDate = eu.EntryDate
                                }).ToList(),
                            })
                            .ToListAsync();
    }

    // Retrieves all events of a specific type
    public async Task<List<EventSummaryDto>> GetAllEventsByTypeAsync(EventType type)
    {
        var context = await contextFactory.CreateDbContextAsync();
        return await context.Events
                            .Where(x => x.Type == type)
                            .Include(e => e.User)
                            .Select(e => new EventSummaryDto
                            {
                                Id = e.Id,
                                Name = e.Name,
                                Type = e.Type,
                                UserId = e.UserId,
                                UserName = e.User.Name,
                                Participants = e.EventsUsers.Select(eu => new ParticipantSummaryDto
                                {
                                    Id = eu.UserId,
                                    UserName = eu.User.Username, // Student's username
                                    Name = eu.User.Name,   // Student's full name
                                    Badge = eu.User.Badge,   // Student's badge
                                    EntryDate = eu.EntryDate
                                }).ToList(),
                            })
                            .ToListAsync();
    }

    // Retrieves a specific event by ID
    public async Task<EventSummaryDto?> GetEventAsync(int id)
    {
        var context = await contextFactory.CreateDbContextAsync();
        return await context.Events
                            .Include(e => e.User)
                            .Select(e => new EventSummaryDto
                            {
                                Id = e.Id,
                                Name = e.Name,
                                Type = e.Type,
                                UserId = e.UserId,
                                UserName = e.User.Name,
                                Participants = e.EventsUsers.Select(eu => new ParticipantSummaryDto
                                {
                                    Id = eu.UserId,
                                    UserName = eu.User.Username, // Student's username
                                    Name = eu.User.Name,   // Student's full name
                                    Badge = eu.User.Badge,   // Student's badge
                                    EntryDate = eu.EntryDate
                                }).ToList(),
                            })
                            .SingleOrDefaultAsync(x => x.Id == id);
    }

    // Retrieves all events for a specific user
    public async Task<List<EventSummaryDto>> GetEventsByUserAsync(int userId)
    {
        var context = await contextFactory.CreateDbContextAsync();
        return await context.Events
                            .Where(x => x.UserId == userId)
                            .Include(e => e.User)
                            .Select(e => new EventSummaryDto
                            {
                                Id = e.Id,
                                Name = e.Name,
                                Type = e.Type,
                                UserId = e.UserId,
                                UserName = e.User.Name,
                                Participants = e.EventsUsers.Select(eu => new ParticipantSummaryDto
                                {
                                    Id = eu.UserId,
                                    UserName = eu.User.Username, // Student's username
                                    Name = eu.User.Name,   // Student's full name
                                    Badge = eu.User.Badge,   // Student's badge
                                    EntryDate = eu.EntryDate
                                }).ToList(),
                            })
                            .ToListAsync();
    }

    // Updates an existing event
    public async Task UpdateEventAsync(int id, EventUpdateDto editedEvent, User user)
    {
        var context = await contextFactory.CreateDbContextAsync();
        var evt = await context.Events.SingleAsync(x => x.Id == id);
        if (evt.UserId != user.Id && user.Type != UserType.Admin)
            throw new UnauthorizedAccessException();
        mapper.Map(editedEvent, evt);
        context.Events.Update(evt);
        await context.SaveChangesAsync();
    }

    // Deletes an event
    public async Task DeleteEventAsync(int id)
    {
        var context = await contextFactory.CreateDbContextAsync();
        var evt = await context.Events.SingleAsync(x => x.Id == id);
        context.Events.Remove(evt);
        await context.SaveChangesAsync();
    }

    // Adds a user to an event
    public async Task AddUserToEventAsync(int userId, int eventId)
    {
        var context = await contextFactory.CreateDbContextAsync();
        await context.EventsUsers.AddAsync(
            new EventUser
            {
                EventId = eventId,
                UserId = userId,
                EntryDate = DateTime.UtcNow
            });
        await context.SaveChangesAsync();
    }

    // Professor's view of history for THEIR events
    public async Task<List<HistoryDto>> GetEventsHistoryByUserAsync(int userId, long dayTsStart, long dayTsEnd, string timezone = "UTC", string culture = "en-US")
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timezone);
        var ct = new CultureInfo(culture);
        var context = await contextFactory.CreateDbContextAsync();

        var query = from e in context.EventsUsers
                    join e2 in context.Events on e.EventId equals e2.Id
                    join u in context.Users on e.UserId equals u.Id
                    where e2.UserId == userId
                       && e.EntryDate >= DateTimeOffset.FromUnixTimeSeconds(dayTsStart)
                       && e.EntryDate <= DateTimeOffset.FromUnixTimeSeconds(dayTsEnd)
                    select new HistoryDto
                    {
                        EventEntryDate = TimeZoneInfo.ConvertTimeFromUtc(e.EntryDate, tz).ToString(ct),
                        EventName = e2.Name,
                        UserBirthName = u.Name,
                        UserName = u.Username,
                        UserBadge = u.Badge
                    };

        var events = await query.ToListAsync();

        return events;
    }

    // Student to get THEIR OWN participation history
    public async Task<List<HistoryDto>> GetStudentOwnParticipationHistoryAsync(int studentUserId, long dayTsStart, long dayTsEnd, string timezone = "UTC", string culture = "en-US")
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timezone); // Handle potential invalid timezone string
        var ct = new CultureInfo(culture); // Handle potential invalid culture string
        var context = await contextFactory.CreateDbContextAsync();

        var startDateUtc = DateTimeOffset.FromUnixTimeSeconds(dayTsStart).UtcDateTime;
        var endDateUtc = DateTimeOffset.FromUnixTimeSeconds(dayTsEnd).UtcDateTime;

        var query = from eu_join in context.EventsUsers // eu_join for EventUser (this is the student's participation)
                    join e_join in context.Events on eu_join.EventId equals e_join.Id // e_join for Event details
                    join u_student in context.Users on eu_join.UserId equals u_student.Id // u_student is the participating student (themselves)
                    // Optionally, join with the professor who created the event if needed in DTO
                    // join u_professor in context.Users on e_join.UserId equals u_professor.Id 
                    where eu_join.UserId == studentUserId // Filter by the student themselves
                       && eu_join.EntryDate >= startDateUtc
                       && eu_join.EntryDate <= endDateUtc
                    select new HistoryDto // HistoryDto structure: EventName, UserBirthName (Student's Name), UserName (Student's Username), UserBadge, EventEntryDate
                    {
                        EventEntryDate = TimeZoneInfo.ConvertTimeFromUtc(eu_join.EntryDate, tz).ToString(ct.DateTimeFormat),
                        EventName = e_join.Name,
                        UserBirthName = u_student.Name,    // This is the student's own full name
                        UserName = u_student.Username, // This is the student's own username
                        UserBadge = u_student.Badge,      // This is the student's own badge
                        // If you need professor's name: ProfessorName = u_professor.Name (requires the join above)
                        // If you need event type: EventType = e_join.Type (add to HistoryDto or a new StudentHistoryDto)
                    };
        return await query.ToListAsync();
    }
    #endregion
}