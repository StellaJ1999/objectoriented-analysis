using System.ComponentModel.DataAnnotations;

namespace Domain.Common.ValueObjects;

public sealed record EmailAddress
{
    private EmailAddress(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ValidationException("Email is required");

        Value = email.Trim();
    }

    public string Value { get; private set; }


    public static EmailAddress Create(string email)
        => new(email);
}
