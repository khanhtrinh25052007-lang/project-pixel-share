using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace project_photo_sharing.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    [Table("PhotoPosts")]
    public class PhotoPost
    {
        [Key]
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string ImageUrl { get; set; }
        public string Caption { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual User Author { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Like> Likes { get; set; }

        [NotMapped]
        public bool IsFollowedByCurrentUser { get; set; }

        [NotMapped]
        public bool IsLikedByCurrentUser { get; set; }

        [NotMapped]
        public int LikeCount { get; set; }
    }

    [Table("Comments")]
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey("PostId")]
        public virtual PhotoPost PhotoPost { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }

    [Table("Likes")]
    public class Like
    {
        [Key]
        public int LikeId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }

        [ForeignKey("PostId")]
        public virtual PhotoPost PhotoPost { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }

    [Table("Follows")]
    public class Follow
    {
        [Key]
        public int FollowId { get; set; }
        public int FollowerId { get; set; }
        public int FollowingId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PhotoSharingDbContext : DbContext
    {
        public PhotoSharingDbContext() : base("name=PhotoSharingDbContext")
        {
            Database.SetInitializer<PhotoSharingDbContext>(null);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<PhotoPost> PhotoPosts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Follow> Follows { get; set; }
    }
}