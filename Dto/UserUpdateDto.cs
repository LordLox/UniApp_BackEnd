// Data Transfer Object for updating an existing user
public class UserUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public UserType Type { get; set; }
}