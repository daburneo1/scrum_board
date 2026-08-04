namespace Application.Contracts.Boards;

public sealed record UserOptionDto(
    Guid Id,
    string Name,
    string Email);