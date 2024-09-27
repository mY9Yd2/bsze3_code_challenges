using System;
using System.Collections.Generic;

namespace Bsze3Blog.Models;

/// <summary>
/// Sikertelen bejelentkezési kísérletek időpontjai a felhasználói fiókokba
/// </summary>
public partial class FailedLoginAttempt
{
    public uint Id { get; set; }

    public uint UserId { get; set; }

    /// <summary>
    /// A bejelentkezési kísérlet időpontja
    /// </summary>
    public DateTime AttemptAt { get; set; }

    public virtual User User { get; set; } = null!;
}
