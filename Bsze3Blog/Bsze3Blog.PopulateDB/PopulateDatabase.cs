using System.Reflection;

using Bsze3Blog.Models;

using Microsoft.EntityFrameworkCore;

namespace Bsze3Blog.PopulateDB;

internal interface ICanDeleteUsers
{
    void DeleteSomeUsers();
}

internal interface ICanPopulateBlogsAndCanDelete : ICanDeleteUsers
{
    ICanPopulateBlogsAndCanDelete BlogViews();
    ICanPopulateBlogsAndCanDelete BlogEditLogs();
}

internal interface ICanResetDatabase
{
    ICanPopulateUsers Reset();
}

internal interface ICanPopulateUsers
{
    ICanPopulateBlogsAndCanDelete Users(int numberOfUsers);
}

internal class PopulateDatabase : ICanPopulateBlogsAndCanDelete, ICanPopulateUsers, ICanResetDatabase
{
    private readonly Bsze3BlogContext _ctx;
    private uint _userId;

    private PopulateDatabase(Bsze3BlogContext ctx) => _ctx = ctx;

    internal static ICanResetDatabase Use(Bsze3BlogContext ctx) => new PopulateDatabase(ctx);

    public ICanPopulateUsers Reset()
    {
        // Az alkalmazás mellett levő create_database.sql-t futtatjuk.
        string path = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? throw new Exception("Unable to find the path");
        string sql = File.ReadAllText(Path.Join(path, "create_database.sql"))
            .Replace("DELIMITER //", string.Empty)
            .Replace("DELIMITER ;", string.Empty)
            .Replace("//", string.Empty);

        try
        {
            _ctx.Database.ExecuteSqlRaw(sql);
        }
        catch (MySqlConnector.MySqlException ex)
        {
            Console.WriteLine(ex.Message);
            Environment.Exit(1);
        }

        _userId = 0;

        return this;
    }

    public ICanPopulateBlogsAndCanDelete Users(int numberOfUsers)
    {
        for (int usersCount = 0; usersCount < numberOfUsers; usersCount++)
        {
            if (usersCount % 25 == 0 && usersCount != 0)
                Console.WriteLine($"{usersCount} users so far...");

            User user = _ctx.GenerateUser(++_userId);
            _ctx.Add(user);

            try
            {
                _ctx.SaveChanges();
            }
            catch (DbUpdateException exception)
            {
                // TODO: Incorrect datetime value (DST)
                // ---> MySqlConnector.MySqlException (0x80004005): Incorrect datetime value: '2017-03-26 02:07:27.725261'
                // https://stackoverflow.com/questions/35602939/mysql-1292-incorrect-datetime-value#35964177
                Console.WriteLine($"\n(DbUpdateException) Regenerate the user...\n{exception.InnerException?.Message}\n");
                _ctx.Entry(user).State = EntityState.Detached;
                usersCount--;
            }
        }
        Console.WriteLine($"\n{_ctx.Users.Count()} users added");
        return this;
    }

    public ICanPopulateBlogsAndCanDelete BlogViews()
    {
        Console.WriteLine($"\nThere are {_ctx.BlogEntries.Count()} blogs");
        foreach (BlogEntry blogEntry in _ctx.BlogEntries.ToList())
        {
            foreach (BlogView blogView in _ctx.GenerateBlogView(blogEntry))
            {
                _ctx.Add(blogView);
                try
                {
                    _ctx.SaveChanges();
                    blogEntry.NumberOfViews++;
                }
                catch (DbUpdateException exception)
                {
                    // TODO: Incorrect datetime value (DST)
                    Console.WriteLine($"\n(DbUpdateException) Regenerate blog view...\n{exception.InnerException?.Message}\n");
                    _ctx.Entry(blogView).State = EntityState.Detached;
                }
            }
        }
        Console.WriteLine($"\n{_ctx.BlogViews.Count()} blog views added");
        return this;
    }

    public ICanPopulateBlogsAndCanDelete BlogEditLogs()
    {
        foreach (BlogEditLog blogEditLog in _ctx.GenerateBlogEditLog())
        {
            _ctx.Add(blogEditLog);
            try
            {
                _ctx.SaveChanges();
            }
            catch (DbUpdateException exception)
            {
                // TODO: Incorrect datetime value (DST)
                Console.WriteLine($"\n(DbUpdateException) Regenerate blog edit log...\n{exception.InnerException?.Message}\n");
                _ctx.Entry(blogEditLog).State = EntityState.Detached;
            }
        }
        Console.WriteLine($"\n{_ctx.BlogEditLogs.Count()} blog edit logs added");
        return this;
    }

    public void DeleteSomeUsers()
    {
        // Random kiválasztott felhasználókat töröltre módosítunk
        // TODO: törölni is, amit kell
        int amount = (int)(_ctx.Users.Count() * 0.25);
        _ctx.Users.OrderBy(_ => EF.Functions.Random())
            .Take(amount)
            .ExecuteUpdate(setters =>
                setters.SetProperty(user => user.UserStatus, "Deleted"));
        Console.WriteLine($"\n{amount} users \"deleted\"");
    }
}
