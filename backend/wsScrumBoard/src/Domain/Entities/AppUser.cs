using Domain.Common;

namespace Domain.Entities;

public sealed class AppUser : Entity
{
    private AppUser()
    {
    }

    public AppUser(
        string name,
        string email,
        string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "El nombre del usuario es obligatorio.",
                nameof(name));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException(
                "El correo electrónico es obligatorio.",
                nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException(
                "El hash de la contraseña es obligatorio.",
                nameof(passwordHash));
        }

        Name = name.Trim();
        Email = email.Trim();
        NormalizedEmail = Email.ToUpperInvariant();
        PasswordHash = passwordHash;
    }

    public string Name { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string NormalizedEmail { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public ICollection<BoardTask> AssignedTasks { get; private set; } =
        new List<BoardTask>();
}
