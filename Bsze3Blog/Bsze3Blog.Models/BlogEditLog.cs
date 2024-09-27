using System;
using System.Collections.Generic;

namespace Bsze3Blog.Models;

/// <summary>
/// A blogok szerkesztési naplója
/// </summary>
public partial class BlogEditLog
{
    public uint Id { get; set; }

    public uint BlogEntryId { get; set; }

    /// <summary>
    /// A felhasználó, aki szerkesztette a blogot (FK ID)
    /// </summary>
    public uint? EditedBy { get; set; }

    /// <summary>
    /// A blog szerkesztésének, módosításának időpontja
    /// </summary>
    public DateTime EditedAt { get; set; }

    public virtual BlogEntry BlogEntry { get; set; } = null!;

    public virtual User? EditedByNavigation { get; set; }
}
