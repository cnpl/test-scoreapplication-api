using Microsoft.AspNetCore.Identity;

namespace ScoreTracker.Api.Models.Entities;

/// <summary>
/// Custom User class that extends the built-in IdentityUser.
/// You can add custom properties here, such as FullName, DateOfBirth, etc.
/// </summary>
public class ApplicationUser : IdentityUser
{
    // Example of a custom property
    public string? FullName { get; set; }
}
