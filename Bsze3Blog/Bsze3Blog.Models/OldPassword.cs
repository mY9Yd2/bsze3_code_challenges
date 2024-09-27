using System;
using System.Collections.Generic;

namespace Bsze3Blog.Models;

/// <summary>
/// A felhasználók lejárt, érvényét vesztett jelszavai
/// </summary>
public partial class OldPassword
{
    public uint Id { get; set; }

    /// <summary>
    /// A felhasználó hashelt jelszava
    /// </summary>
    public string PasswordHash { get; set; } = null!;

    /// <summary>
    /// A jelszó lejárati dátuma, ami után már nem érvényes
    /// </summary>
    public DateTime ExpiredAt { get; set; }

    public uint UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
