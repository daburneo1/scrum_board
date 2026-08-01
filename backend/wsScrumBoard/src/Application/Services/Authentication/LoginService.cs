using Application.Common.Exceptions;
using Application.Contracts.Authentication;
using Application.Ports.Persistence;
using Application.Ports.Security;

namespace Application.Services.Authentication;

public sealed class LoginService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public LoginService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException(
                "Email and password are required.");
        }

        var normalizedEmail = request.Email
            .Trim()
            .ToUpperInvariant();

        var user = await _userRepository.FindByNormalizedEmailAsync(
            normalizedEmail,
            cancellationToken);

        if (user is null ||
            !_passwordService.VerifyPassword(
                user,
                user.PasswordHash,
                request.Password))
        {
            throw new UnauthorizedException(
                "Invalid email or password.");
        }

        var expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(60);

        return new LoginResponse(
            _tokenService.GenerateToken(user),
            "Bearer",
            expiresAtUtc,
            new AuthenticatedUserDto(
                user.Id,
                user.Name,
                user.Email));
    }
}