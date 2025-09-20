using System.ComponentModel.DataAnnotations;

namespace ScoreTracker.Api.Models.DTOs;

public record RegisterDto([Required] string Email, [Required] string FullName, [Required] string Password);

public record LoginDto([Required] string Email, [Required] string Password);

// Data we send back to the client after a successful login or for a /me check
public record UserDto(string Id, string FullName, string Email, IList<string> Roles);
