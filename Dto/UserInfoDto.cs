// Data Transfer Object for user information
public class UserInfoDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Badge { get; set; }
    public string Username { get; set; } = string.Empty;
    public UserType Type { get; set; }
    public string Password { get; set; } = string.Empty;  // Note: This should be handled carefully for security reasons
}