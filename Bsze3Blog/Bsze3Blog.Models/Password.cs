using System;
using System.Collections.Generic;

namespace Bsze3Blog.Models;

/// <summary>
/// A felhasználók jelenlegi jelszavai
/// </summary>
public partial class Password
{
    public uint UserId { get; set; }

    /// <summary>
    /// A felhasználó hashelt jelszava
    /// </summary>
    public string PasswordHash { get; set; } = null!;

    /// <summary>
    /// A jelszó lejárati dátuma, ami után már nem érvényes
    /// </summary>
    public DateTime ExpireAt { get; set; }

    public virtual User User { get; set; } = null!;
}
