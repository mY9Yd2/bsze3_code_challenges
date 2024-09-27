using System;
using System.Collections.Generic;

namespace Bsze3Blog.Models;

/// <summary>
/// Az utolsó sikeres bejelentkezések időpontjai a felhasználói fiókokba
/// </summary>
public partial class LastLogin
{
    public uint UserId { get; set; }

    /// <summary>
    /// Az utolsó bejelentkezés időpontja
    /// </summary>
    public DateTime LastLoginAt { get; set; }

    public virtual User User { get; set; } = null!;
}
