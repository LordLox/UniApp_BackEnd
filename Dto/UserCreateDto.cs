// Data Transfer Object for creating a new user
public class UserCreateDto
{
    public string Name { get; set; } = string.Empty;
    public int Badge { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserType Type { get; set; }
}