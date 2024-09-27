using System;
using System.Collections.Generic;

namespace Bsze3Blog.Models;

/// <summary>
/// Felhasználói fiókok &amp; aktivitási állapota
/// </summary>
public partial class User
{
    public uint Id { get; set; }

    /// <summary>
    /// A felhasználó jelenlegi állapota
    /// </summary>
    public string UserStatus { get; set; } = null!;

    /// <summary>
    /// A felhasználó létrehozásának időpontja
    /// </summary>
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<BlogEditLog> BlogEditLogs { get; set; } = new List<BlogEditLog>();

    public virtual ICollection<BlogView> BlogViews { get; set; } = new List<BlogView>();

    public virtual ICollection<FailedLoginAttempt> FailedLoginAttempts { get; set; } = new List<FailedLoginAttempt>();

    public virtual LastLogin? LastLogin { get; set; }

    public virtual ICollection<OldPassword> OldPasswords { get; set; } = new List<OldPassword>();

    public virtual Password? Password { get; set; }

    public virtual Permission? Permission { get; set; }

    public virtual Role? Role { get; set; }

    public virtual UserPersonalInformation? UserPersonalInformation { get; set; }
}
