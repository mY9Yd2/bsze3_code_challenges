using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Bsze3Blog.Models;

public partial class Bsze3BlogContext : DbContext
{
    public Bsze3BlogContext(DbContextOptions<Bsze3BlogContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BlogEditLog> BlogEditLogs { get; set; }

    public virtual DbSet<BlogEntry> BlogEntries { get; set; }

    public virtual DbSet<BlogView> BlogViews { get; set; }

    public virtual DbSet<FailedLoginAttempt> FailedLoginAttempts { get; set; }

    public virtual DbSet<LastLogin> LastLogins { get; set; }

    public virtual DbSet<OldPassword> OldPasswords { get; set; }

    public virtual DbSet<Password> Passwords { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserPersonalInformation> UserPersonalInformations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_hungarian_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<BlogEditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("blog_edit_logs", tb => tb.HasComment("A blogok szerkesztési naplója"));

            entity.HasIndex(e => e.BlogEntryId, "fk_blog_edit_logs__blog_entries");

            entity.HasIndex(e => e.EditedBy, "fk_blog_edit_logs__users");

            entity.Property(e => e.Id)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.BlogEntryId)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("blog_entry_id");
            entity.Property(e => e.EditedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasComment("A blog szerkesztésének, módosításának időpontja")
                .HasColumnType("timestamp")
                .HasColumnName("edited_at");
            entity.Property(e => e.EditedBy)
                .HasComment("A felhasználó, aki szerkesztette a blogot (FK ID)")
                .HasColumnType("int(10) unsigned")
                .HasColumnName("edited_by");

            entity.HasOne(d => d.BlogEntry).WithMany(p => p.BlogEditLogs)
                .HasForeignKey(d => d.BlogEntryId)
                .HasConstraintName("fk_blog_edit_logs__blog_entries");

            entity.HasOne(d => d.EditedByNavigation).WithMany(p => p.BlogEditLogs)
                .HasForeignKey(d => d.EditedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_blog_edit_logs__users");
        });

        modelBuilder.Entity<BlogEntry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("blog_entries", tb => tb.HasComment("Blog bejegyzések & alap adatai"));

            entity.HasIndex(e => e.Author, "fk_blog_entries__user_personal_informations");

            entity.Property(e => e.Id)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.Author)
                .HasComment("A blogot létrehozó felhasználó (FK ID)")
                .HasColumnType("int(10) unsigned")
                .HasColumnName("author");
            entity.Property(e => e.Content)
                .HasComment("A blog tartalma")
                .HasColumnType("text")
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasComment("A blog létehozásának időpontja")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasComment("A blog \"törlési\" időpontja")
                .HasColumnType("timestamp")
                .HasColumnName("deleted_at");
            entity.Property(e => e.NumberOfViews)
                .HasComment("Hányszor tekintették meg a blogot a felhasználók")
                .HasColumnType("int(10) unsigned")
                .HasColumnName("number_of_views");
            entity.Property(e => e.Title)
                .HasMaxLength(70)
                .HasComment("A blog címe")
                .HasColumnName("title");

            entity.HasOne(d => d.AuthorNavigation).WithMany(p => p.BlogEntries)
                .HasForeignKey(d => d.Author)
                .HasConstraintName("fk_blog_entries__user_personal_informations");
        });

        modelBuilder.Entity<BlogView>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("blog_views", tb => tb.HasComment("A blogok megtekintés és látogatási adatai, időpontjai"));

            entity.HasIndex(e => e.BlogEntryId, "fk_blog_views__blog_entries");

            entity.HasIndex(e => e.ViewedBy, "fk_blog_views__users");

            entity.Property(e => e.Id)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.BlogEntryId)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("blog_entry_id");
            entity.Property(e => e.ViewedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasComment("Az időpont, amikor a felhasználó megtekintette a blogot")
                .HasColumnType("timestamp")
                .HasColumnName("viewed_at");
            entity.Property(e => e.ViewedBy)
                .HasComment("A felhasználó, aki megtekintette a blogot (FK ID)")
                .HasColumnType("int(10) unsigned")
                .HasColumnName("viewed_by");

            entity.HasOne(d => d.BlogEntry).WithMany(p => p.BlogViews)
                .HasForeignKey(d => d.BlogEntryId)
                .HasConstraintName("fk_blog_views__blog_entries");

            entity.HasOne(d => d.ViewedByNavigation).WithMany(p => p.BlogViews)
                .HasForeignKey(d => d.ViewedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_blog_views__users");
        });

        modelBuilder.Entity<FailedLoginAttempt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("failed_login_attempts", tb => tb.HasComment("Sikertelen bejelentkezési kísérletek időpontjai a felhasználói fiókokba"));

            entity.HasIndex(e => e.UserId, "fk_failed_login_attempts__users");

            entity.Property(e => e.Id)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.AttemptAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasComment("A bejelentkezési kísérlet időpontja")
                .HasColumnType("timestamp")
                .HasColumnName("attempt_at");
            entity.Property(e => e.UserId)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.FailedLoginAttempts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_failed_login_attempts__users");
        });

        modelBuilder.Entity<LastLogin>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("last_logins", tb => tb.HasComment("Az utolsó sikeres bejelentkezések időpontjai a felhasználói fiókokba"));

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnType("int(10) unsigned")
                .HasColumnName("user_id");
            entity.Property(e => e.LastLoginAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasComment("Az utolsó bejelentkezés időpontja")
                .HasColumnType("timestamp")
                .HasColumnName("last_login_at");

            entity.HasOne(d => d.User).WithOne(p => p.LastLogin)
                .HasForeignKey<LastLogin>(d => d.UserId)
                .HasConstraintName("fk_last_logins__users");
        });

        modelBuilder.Entity<OldPassword>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("old_passwords", tb => tb.HasComment("A felhasználók lejárt, érvényét vesztett jelszavai"));

            entity.HasIndex(e => e.UserId, "fk_old_passwords__users");

            entity.Property(e => e.Id)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.ExpiredAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasComment("A jelszó lejárati dátuma, ami után már nem érvényes")
                .HasColumnType("timestamp")
                .HasColumnName("expired_at");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(84)
                .HasComment("A felhasználó hashelt jelszava")
                .HasColumnName("password_hash");
            entity.Property(e => e.UserId)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.OldPasswords)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_old_passwords__users");
        });

        modelBuilder.Entity<Password>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("passwords", tb => tb.HasComment("A felhasználók jelenlegi jelszavai"));

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnType("int(10) unsigned")
                .HasColumnName("user_id");
            entity.Property(e => e.ExpireAt)
                .HasDefaultValueSql("(current_timestamp() + interval 3 year)")
                .HasComment("A jelszó lejárati dátuma, ami után már nem érvényes")
                .HasColumnType("timestamp")
                .HasColumnName("expire_at");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(84)
                .HasComment("A felhasználó hashelt jelszava")
                .HasColumnName("password_hash");

            entity.HasOne(d => d.User).WithOne(p => p.Password)
                .HasForeignKey<Password>(d => d.UserId)
                .HasConstraintName("fk_passwords__users");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("permissions", tb => tb.HasComment("A felhasználók jogosultságai"));

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnType("int(10) unsigned")
                .HasColumnName("user_id");
            entity.Property(e => e.CanCreateBlog)
                .HasComment("A felhasználónak van-e jogosultsága létrehozni egy blogot")
                .HasColumnName("can_create_blog");
            entity.Property(e => e.CanDeleteBlog)
                .HasComment("A felhasználónak van-e jogosultsága törölni egy blogot")
                .HasColumnName("can_delete_blog");
            entity.Property(e => e.CanEditBlog)
                .HasComment("A felhasználónak van-e jogosultsága szerkeszteni egy blogot")
                .HasColumnName("can_edit_blog");

            entity.HasOne(d => d.User).WithOne(p => p.Permission)
                .HasForeignKey<Permission>(d => d.UserId)
                .HasConstraintName("fk_permissions__users");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("roles", tb => tb.HasComment("A felhasználók jogosultsági szerepkörei"));

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnType("int(10) unsigned")
                .HasColumnName("user_id");
            entity.Property(e => e.IsAdministrator)
                .HasComment("Adminisztrátor-e a felhasználó")
                .HasColumnName("is_administrator");
            entity.Property(e => e.IsModerator)
                .HasComment("Moderátor-e a felhasználó")
                .HasColumnName("is_moderator");

            entity.HasOne(d => d.User).WithOne(p => p.Role)
                .HasForeignKey<Role>(d => d.UserId)
                .HasConstraintName("fk_roles__users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users", tb => tb.HasComment("Felhasználói fiókok & aktivitási állapota"));

            entity.Property(e => e.Id)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasComment("A felhasználó létrehozásának időpontja")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserStatus)
                .HasDefaultValueSql("'Inactive'")
                .HasComment("A felhasználó jelenlegi állapota")
                .HasColumnType("enum('Active','Inactive','Deleted')")
                .HasColumnName("user_status");
        });

        modelBuilder.Entity<UserPersonalInformation>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("user_personal_informations", tb => tb.HasComment("A felhasználók személyes adatai"));

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnType("int(10) unsigned")
                .HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasComment("A felhasználó személyes adatainak a létrehozásának időpontja")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(70)
                .HasComment("A felhasználó neve, amit mások is láthatnak")
                .HasColumnName("display_name");
            entity.Property(e => e.Email)
                .HasMaxLength(40)
                .HasComment("A felhasználó emailcíme")
                .HasColumnName("email");
            entity.Property(e => e.TelephoneNumber)
                .HasMaxLength(14)
                .IsFixedLength()
                .HasComment("A felhasználó telefonszáma 00-00-000-0000 formátumban")
                .HasColumnName("telephone_number");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.User).WithOne(p => p.UserPersonalInformation)
                .HasForeignKey<UserPersonalInformation>(d => d.UserId)
                .HasConstraintName("fk_user_personal_informations__users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
