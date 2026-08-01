namespace Application.Contracts.Authentication;

public sealed record AuthenticatedUserDto(
    Guid Id,
    string Name,
    string Email);