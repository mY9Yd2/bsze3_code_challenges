using System;
using System.Collections.Generic;

namespace Bsze3Blog.Models;

/// <summary>
/// A blogok megtekintés és látogatási adatai, időpontjai
/// </summary>
public partial class BlogView
{
    public uint Id { get; set; }

    public uint BlogEntryId { get; set; }

    /// <summary>
    /// A felhasználó, aki megtekintette a blogot (FK ID)
    /// </summary>
    public uint? ViewedBy { get; set; }

    /// <summary>
    /// Az időpont, amikor a felhasználó megtekintette a blogot
    /// </summary>
    public DateTime ViewedAt { get; set; }

    public virtual BlogEntry BlogEntry { get; set; } = null!;

    public virtual User? ViewedByNavigation { get; set; }
}
