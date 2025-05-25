using System.Text;
using AutoMapper;
using CheckPasswordStrength;
using Microsoft.EntityFrameworkCore;

public class UsersService(IDbContextFactory<ApplicationContext> contextFactory, IMapper mapper)
{
    private readonly IDbContextFactory<ApplicationContext> contextFactory = contextFactory;
    private readonly IMapper mapper = mapper;

    #region CRUD
    // Creates a new user
    public int CreateUser(UserCreateDto newUser, bool ignorePasswordStrength = false)
    {
        if (!ignorePasswordStrength)
        {
            var pwStrength = newUser.Password.PasswordStrength();
            if (pwStrength.Id < 2)
                throw new PasswordTooWeakException();
        }

        var user = mapper.Map<User>(newUser);

        user.PasswordSalt = CryptoExtensions.GenerateSalt();
        user.HashedPassword = CryptoExtensions.HMACSHA256(newUser.Password, user.PasswordSalt);

        var context = contextFactory.CreateDbContext();
        context.Users.Add(user);
        context.SaveChanges();
        return user.Id;
    }

    // Asynchronous wrapper for CreateUser method
    public async Task<int> CreateUserAsync(UserCreateDto newUser, bool ignorePasswordStrength = false)
    {
        return await Task.FromResult(CreateUser(newUser, ignorePasswordStrength));
    }

    // Retrieves all users
    public async Task<List<User>> GetAllUsersAsync()
    {
        var context = await contextFactory.CreateDbContextAsync();
        return await context.Users.ToListAsync();
    }

    // Retrieves a user by ID
    public async Task<User?> GetUserAsync(int id)
    {
        var context = await contextFactory.CreateDbContextAsync();
        return await context.Users.SingleOrDefaultAsync(x => x.Id == id);
    }

    // Retrieves a user by username
    public async Task<User?> GetUserAsync(string username)
    {
        var context = await contextFactory.CreateDbContextAsync();
        return await context.Users.SingleOrDefaultAsync(x => x.Username == username);
    }

    // Retrieves user info by ID
    public async Task<UserInfoDto?> GetUserInfoAsync(int id)
    {
        var context = await contextFactory.CreateDbContextAsync();
        var user = await context.Users.SingleOrDefaultAsync(x => x.Id == id);
        return mapper.Map<UserInfoDto?>(user);
    }

    // Retrieves user info by username
    public async Task<UserInfoDto?> GetUserInfoAsync(string username)
    {
        var context = await contextFactory.CreateDbContextAsync();
        var user = await context.Users.SingleOrDefaultAsync(x => x.Username == username);
        return mapper.Map<UserInfoDto?>(user);
    }

    // Retrieves a user by ID (non-async version)
    public User? GetUser(int id)
    {
        var context = contextFactory.CreateDbContext();
        return context.Users.SingleOrDefault(x => x.Id == id);
    }

    // Updates an existing user
    public async Task UpdateUserAsync(int id, UserUpdateDto editedUser)
    {
        var context = await contextFactory.CreateDbContextAsync();
        var user = await context.Users.SingleAsync(x => x.Id == id);
        mapper.Map(editedUser, user);
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }

    // Deletes a user
    public async Task DeleteUserAsync(int id)
    {
        var context = await contextFactory.CreateDbContextAsync();
        var user = await context.Users.SingleAsync(x => x.Id == id);
        context.Users.Remove(user);
        await context.SaveChangesAsync();
    }
    #endregion

    #region Authentication
    // Changes a user's password
    public async Task ChangePasswordAsync(int id, string newPassword, bool ignorePasswordStrength = false)
    {
        if (!ignorePasswordStrength)
        {
            var pwStrength = newPassword.PasswordStrength();
            if (pwStrength.Id < 2)
                throw new PasswordTooWeakException();
        }

        var salt = CryptoExtensions.GenerateSalt();
        var hashedPassword = CryptoExtensions.HMACSHA256(newPassword, salt);

        var context = await contextFactory.CreateDbContextAsync();
        var user = await context.Users.SingleAsync(x => x.Id == id);
        user.PasswordSalt = salt;
        user.HashedPassword = hashedPassword;
        context.Users.Update(user);
        await context.SaveChangesAsync();
    }

    // Authenticates a user
    public async Task<bool> Authenticate(AuthDto auth)
    {
        var context = await contextFactory.CreateDbContextAsync();
        var user = await context.Users.SingleOrDefaultAsync(x => x.Username == auth.Username)
            ?? throw new Exception($"User {auth.Username} not found");

        var hashCheck = CryptoExtensions.HMACSHA256(auth.Password, user.PasswordSalt);

        if (user.HashedPassword != hashCheck)
            return false;

        return true;
    }

    // Retrieves a user from HTTP request authentication
    public async Task<User?> GetUserFromAuthAsync(HttpRequest request)
    {
        var authDto = request.GetBasicAuthenticationHeader();
        var user = await GetUserAsync(authDto.Username);
        return user;
    }

    // Retrieves user info from HTTP request authentication
    public async Task<UserInfoDto?> GetUserInfoFromAuthAsync(HttpRequest request)
    {
        var authDto = request.GetBasicAuthenticationHeader();
        var userInfo = await GetUserInfoAsync(authDto.Username)
            ?? throw new Exception("User not found");
        userInfo.Password = authDto.Password;
        return userInfo;
    }
    #endregion

    #region Authorization
    // Checks if a user has a specific role
    public async Task<bool> Authorize(int userId, UserType userType)
    {
        var context = await contextFactory.CreateDbContextAsync();
        var user = await context.Users.SingleAsync(x => x.Id == userId);
        return user.Type == userType;
    }
    #endregion
}