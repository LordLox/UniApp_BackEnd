// This class holds global authentication filters for different user types
public class Globals
{
    // Authentication filter for admin users
    public BasicAuthFilter AdminAuth { get; set; } = null!;

    // Authentication filter for professor users
    public BasicAuthFilter ProfessorAuth { get; set; } = null!;

    // Authentication filter for student users
    public BasicAuthFilter StudentAuth { get; set; } = null!;

    // Authentication filter for all user types
    public BasicAuthFilter AllAuth { get; set; } = null!;
}