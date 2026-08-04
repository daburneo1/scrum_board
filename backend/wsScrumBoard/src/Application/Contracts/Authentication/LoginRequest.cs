namespace Application.Contracts.Authentication;

public sealed record LoginRequest(
    string Email,
    string Password);