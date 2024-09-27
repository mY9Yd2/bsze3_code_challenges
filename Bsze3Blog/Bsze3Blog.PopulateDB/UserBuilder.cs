using Bogus;

using Bsze3Blog.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Bsze3Blog.PopulateDB;

internal class UserBuilder
{
    private Faker _f = new();
    private User _user = new();

    public UserBuilder() { }

    internal void Reset()
    {
        _f = new();
        _user = new User();
    }

    internal User GetUser()
    {
        User user = _user;
        Reset();
        return user;
    }

    internal void UserPart(uint userId)
    {
        _user.Id = userId;
        _user.CreatedAt = _f.Date.Between(
            new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Local),
            DateTime.Now
        );
        _user.UpdatedAt = _f.Date.Between(_user.CreatedAt, DateTime.Now);
    }

    internal void UserPersonalInformationPart()
    {
        _user.UserPersonalInformation ??= new();
        _user.UserPersonalInformation.UserId = _user.Id;
        _user.UserPersonalInformation.DisplayName = _f.Person.FullName;
        _user.UserPersonalInformation.Email = _f.Internet.Email(
            firstName: _f.Person.FirstName,
            lastName: _f.Person.LastName,
            provider: null,
            uniqueSuffix: _user.Id.ToString()
        );
        _user.UserPersonalInformation.TelephoneNumber = _f.Person.TelephoneNumber();
        _user.UserPersonalInformation.CreatedAt = _user.CreatedAt;
        _user.UserPersonalInformation.UpdatedAt = _f.Date.Between(
            _user.UserPersonalInformation.CreatedAt,
            DateTime.Now
        );
    }

    internal void PermissionPart()
    {
        _user.Permission ??= new();

        _user.Permission.UserId = _user.Id;
        _user.Permission.CanCreateBlog = _f.Random.Bool();
        _user.Permission.CanDeleteBlog = _f.Random.WeightedRandom(
            [true, false],
            [0.7f, 1.0f]
        );
        _user.Permission.CanEditBlog = _f.Random.WeightedRandom(
            [true, false],
            [3.0f, 0.1f]
        );
    }

    internal void RolePart()
    {
        _user.Role ??= new();

        _user.Role.UserId = _user.Id;
        _user.Role.IsAdministrator = _f.Random.WeightedRandom(
            [true, false],
            [0.1f, 2.0f]
        );
        _user.Role.IsModerator = _user.Role.IsAdministrator || _f.Random.WeightedRandom(
            [true, false],
            [0.1f, 1.0f]
        );
    }

    internal void PasswordsPart()
    {
        PasswordHasher<User> passwordHasher = new(Options.Create(new PasswordHasherOptions()
        {
            // https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html
            //IterationCount = 600_000 // Lassú
            IterationCount = 10 // Nem ajánlott, de teszthez elég lesz ez az érték
        }));
        List<Password> passwords = [];

        DateTime nextExpireAt = _user.CreatedAt;
        int years = 3;
        do
        {
            Password password = new();

            password.UserId = _user.Id;
            password.PasswordHash = passwordHasher.HashPassword(_user, _f.Internet.Password());
            password.ExpireAt = nextExpireAt;

            passwords.Add(password);
            nextExpireAt = nextExpireAt.AddYears(years);
        } while (nextExpireAt < DateTime.Now.AddYears(years));

        _user.Password = passwords[^1];
        _user.OldPasswords = passwords[..^1].ConvertAll(password => new OldPassword()
        {
            PasswordHash = password.PasswordHash,
            ExpiredAt = password.ExpireAt
        });
    }

    internal void LastLoginPart()
    {
        LastLogin? lastLogin = null;
        if (_f.Random.WeightedRandom([true, false], [1.0f, 0.1f]))
        {
            lastLogin = new();
            lastLogin.UserId = _user.Id;
            lastLogin.LastLoginAt = _f.Date.Between(_user.CreatedAt, DateTime.Now);
        }

        _user.LastLogin = lastLogin;
    }

    internal void FailedLoginAttemptsPart()
    {
        List<FailedLoginAttempt> failedLoginAttempts = [];

        for (int i = 0; i < _f.Random.Int(0, 10); i++)
        {
            FailedLoginAttempt failedLoginAttempt = new();

            failedLoginAttempt.UserId = _user.Id;
            failedLoginAttempt.AttemptAt = _f.Date.Between(_user.CreatedAt, DateTime.Now);

            failedLoginAttempts.Add(failedLoginAttempt);
        }

        _user.FailedLoginAttempts = [.. failedLoginAttempts.OrderBy(
            failedLoginAttempt => failedLoginAttempt.AttemptAt
        )];
    }

    internal void BlogEntriesPart()
    {
        _user.Permission ??= new();
        _user.UserPersonalInformation ??= new();

        if (_user.Permission.CanCreateBlog)
        {
            for (int i = 0; i < _f.Random.Int(0, 3); i++)
            {
                BlogEntry blogEntry = new();
                blogEntry.Title = _f.Lorem.Word();
                blogEntry.Content = _f.Lorem.Paragraph();
                blogEntry.Author = _user.Id;
                blogEntry.CreatedAt = _f.Date.Between(_user.CreatedAt, DateTime.Now);
                if (_f.Random.WeightedRandom([true, false], [0.1f, 1.0f]))
                    blogEntry.DeletedAt = _f.Date.Between(blogEntry.CreatedAt, DateTime.Now);

                _user.UserPersonalInformation.BlogEntries.Add(blogEntry);
            }
        }
    }
}
