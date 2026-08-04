namespace Application.Contracts.Authentication;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAt,
    AuthenticatedUserDto User);
    
public sealed record AuthenticatedUserDto(
    Guid Id,
    string Name,
    string Email);