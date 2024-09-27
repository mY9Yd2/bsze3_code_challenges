using System;
using System.Collections.Generic;

namespace Bsze3Blog.Models;

/// <summary>
/// A felhasználók jogosultsági szerepkörei
/// </summary>
public partial class Role
{
    public uint UserId { get; set; }

    /// <summary>
    /// Adminisztrátor-e a felhasználó
    /// </summary>
    public bool IsAdministrator { get; set; }

    /// <summary>
    /// Moderátor-e a felhasználó
    /// </summary>
    public bool IsModerator { get; set; }

    public virtual User User { get; set; } = null!;
}
