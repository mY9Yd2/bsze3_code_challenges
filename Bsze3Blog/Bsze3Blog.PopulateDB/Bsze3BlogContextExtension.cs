using Bogus;

using Bsze3Blog.Models;

namespace Bsze3Blog.PopulateDB;

public static class Bsze3BlogContextExtension
{
    private static readonly Faker F = new();
    private static readonly UserBuilder UserBuilder = new();

    public static User GenerateUser(this Bsze3BlogContext _, uint userId)
    {
        UserBuilder.UserPart(userId);
        UserBuilder.UserPersonalInformationPart();
        UserBuilder.PermissionPart();
        UserBuilder.RolePart();
        UserBuilder.PasswordsPart();
        UserBuilder.LastLoginPart();
        UserBuilder.FailedLoginAttemptsPart();
        UserBuilder.BlogEntriesPart();

        return UserBuilder.GetUser();
    }

    public static IEnumerable<BlogView> GenerateBlogView(this Bsze3BlogContext ctx, BlogEntry blogEntry)
    {
        // Azok a felhasználók tekinthették meg a blogot, akik:
        List<User> validUsers = [.. ctx.Users.Where(user =>
            // - Hamarabb jöttek létre, mint a blog
            user.CreatedAt < blogEntry.CreatedAt
            // - És a blog törlésénél hamarabb jöttek létre
            && (blogEntry.DeletedAt == null || user.CreatedAt < blogEntry.DeletedAt)
            // - És az inaktív felh. közül azok, akik még aktívak voltak, mikor a blog létrejött
            && (user.UserStatus != "Inactive" || user.UpdatedAt > blogEntry.CreatedAt)
            // - És az inaktív felh. közül azok, akik még a blog törlése előtt aktívak voltak
            && (user.UserStatus != "Inactive" || blogEntry.DeletedAt == null || user.UpdatedAt < blogEntry.DeletedAt)
        )];

        if (validUsers.Count == 0) yield break;

        for (int i = 0; i < F.Random.Int(0, 5); i++)
        {
            User randomUser = F.PickRandom(validUsers);

            BlogView blogView = new();
            blogView.BlogEntryId = blogEntry.Id;
            blogView.ViewedBy = randomUser.Id;

            // TODO: Itt is ki kéne logikázni, hogy mi legyen a vége, a felhasználó adatai alapján
            DateTime end = blogEntry.DeletedAt ?? DateTime.Now;
            blogView.ViewedAt = F.Date.Between(blogEntry.CreatedAt, end);

            yield return blogView;
        }
    }

    public static IEnumerable<BlogEditLog> GenerateBlogEditLog(this Bsze3BlogContext ctx)
    {
        // Azok a blog & felhasználó párosok kellenek, ahol
        // a felh. még aktív és van blog szerkesztési jogosultsága.
        // És a blog még nem lett törölve.
        // (Nem valami jó megoldás/szűrés, de mostanra megteszi)
        var blogInfo = ctx.Users
            .Where(user =>
                user.Permission != null
                && user.Permission.CanEditBlog
                && user.UserStatus == "Active")
            .Join(ctx.BlogEntries,
                user => user.Id,
                blogEntry => blogEntry.Author,
                (user, blogEntry) => new { User = user, BlogEntry = blogEntry })
            .Where(user_blogs => user_blogs.BlogEntry.DeletedAt == null).ToList();

        foreach (var info in blogInfo)
        {
            BlogEditLog blogEditLog = new();
            blogEditLog.BlogEntryId = info.BlogEntry.Id;
            blogEditLog.EditedBy = info.User.Id;
            blogEditLog.EditedAt = DateTime.Now;
            yield return blogEditLog;
        }
    }
}
