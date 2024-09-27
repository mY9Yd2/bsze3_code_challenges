using System;
using System.Collections.Generic;

namespace Bsze3Blog.Models;

/// <summary>
/// Blog bejegyzések &amp; alap adatai
/// </summary>
public partial class BlogEntry
{
    public uint Id { get; set; }

    /// <summary>
    /// A blog címe
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// A blog tartalma
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// A blogot létrehozó felhasználó (FK ID)
    /// </summary>
    public uint Author { get; set; }

    /// <summary>
    /// Hányszor tekintették meg a blogot a felhasználók
    /// </summary>
    public uint NumberOfViews { get; set; }

    /// <summary>
    /// A blog létehozásának időpontja
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// A blog &quot;törlési&quot; időpontja
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    public virtual UserPersonalInformation AuthorNavigation { get; set; } = null!;

    public virtual ICollection<BlogEditLog> BlogEditLogs { get; set; } = new List<BlogEditLog>();

    public virtual ICollection<BlogView> BlogViews { get; set; } = new List<BlogView>();
}
