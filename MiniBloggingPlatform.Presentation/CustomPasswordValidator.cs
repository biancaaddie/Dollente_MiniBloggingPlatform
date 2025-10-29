using Microsoft.AspNetCore.Identity;
using MiniBloggingPlatform.Infrastructure.Models;

namespace MiniBloggingPlatform.Presentation;

public class CustomPasswordValidator : IPasswordValidator<ApplicationUser>
{
    public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string password)
    {
        var errors = new List<IdentityError>();

        if (password == null)
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordRequired",
                Description = "Password is required."
            });
            return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
        }

        int uppercaseCount = password.Count(char.IsUpper);
        int digitCount = password.Count(char.IsDigit);
        int symbolCount = password.Count(c => !char.IsLetterOrDigit(c));

        if (uppercaseCount < 2)
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordRequiresUpper",
                Description = "Password must contain at least 2 uppercase letters."
            });
        }

        if (digitCount < 3)
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordRequiresDigit",
                Description = "Password must contain at least 3 digits."
            });
        }

        if (symbolCount < 3)
        {
            errors.Add(new IdentityError
            {
                Code = "PasswordRequiresNonAlphanumeric",
                Description = "Password must contain at least 3 special characters."
            });
        }

        return Task.FromResult(errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success);
    }
}

