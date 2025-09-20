using System.ComponentModel.DataAnnotations;

namespace ScoreTracker.Api.Models.DTOs;

public record ScoreDto(int Id, int Value, DateTime DateRecorded);

public record CreateScoreDto([Required] int Value);
