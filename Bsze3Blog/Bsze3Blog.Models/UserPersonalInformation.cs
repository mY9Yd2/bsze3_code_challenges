using System;
using System.Collections.Generic;

namespace Bsze3Blog.Models;

/// <summary>
/// A felhasználók személyes adatai
/// </summary>
public partial class UserPersonalInformation
{
    public uint UserId { get; set; }

    /// <summary>
    /// A felhasználó neve, amit mások is láthatnak
    /// </summary>
    public string DisplayName { get; set; } = null!;

    /// <summary>
    /// A felhasználó emailcíme
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// A felhasználó telefonszáma 00-00-000-0000 formátumban
    /// </summary>
    public string? TelephoneNumber { get; set; }

    /// <summary>
    /// A felhasználó személyes adatainak a létrehozásának időpontja
    /// </summary>
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<BlogEntry> BlogEntries { get; set; } = new List<BlogEntry>();

    public virtual User User { get; set; } = null!;
}
