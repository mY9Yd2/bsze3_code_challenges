using System;
using System.Collections.Generic;

namespace Bsze3Blog.Models;

/// <summary>
/// A felhasználók jogosultságai
/// </summary>
public partial class Permission
{
    public uint UserId { get; set; }

    /// <summary>
    /// A felhasználónak van-e jogosultsága létrehozni egy blogot
    /// </summary>
    public bool CanCreateBlog { get; set; }

    /// <summary>
    /// A felhasználónak van-e jogosultsága törölni egy blogot
    /// </summary>
    public bool CanDeleteBlog { get; set; }

    /// <summary>
    /// A felhasználónak van-e jogosultsága szerkeszteni egy blogot
    /// </summary>
    public bool CanEditBlog { get; set; }

    public virtual User User { get; set; } = null!;
}
