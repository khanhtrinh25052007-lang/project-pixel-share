using project_photo_sharing.Models;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace project_photo_sharing.Controllers
{
    public class HomeController : Controller
    {
        private PhotoSharingDbContext db = new PhotoSharingDbContext();

        private int? CurrentUserId
        {
            get { return Session["UserId"] as int?; }
        }

        public ActionResult Index()
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = CurrentUserId.Value;

            var posts = db.PhotoPosts
                          .Include(p => p.Author)
                          .Include(p => p.Comments.Select(c => c.User))
                          .Include(p => p.Likes)
                          .OrderByDescending(p => p.CreatedAt)
                          .ToList();

            var followedIds = db.Follows
                                .Where(f => f.FollowerId == userId)
                                .Select(f => f.FollowingId)
                                .ToList();

            foreach (var post in posts)
            {
                post.IsFollowedByCurrentUser = followedIds.Contains(post.UserId);
                post.LikeCount = post.Likes != null ? post.Likes.Count : 0;
                post.IsLikedByCurrentUser = post.Likes != null && post.Likes.Any(l => l.UserId == userId);
            }

            return View(posts);
        }

        public ActionResult MyProfile()
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = CurrentUserId.Value;

            var myPosts = db.PhotoPosts
                            .Where(p => p.UserId == userId)
                            .OrderByDescending(p => p.CreatedAt)
                            .ToList();

            ViewBag.PostCount = myPosts.Count;
            ViewBag.FollowerCount = db.Follows.Count(f => f.FollowingId == userId);
            ViewBag.FollowingCount = db.Follows.Count(f => f.FollowerId == userId);

            return View(myPosts);
        }

        [HttpPost]
        public ActionResult UploadPhoto(HttpPostedFileBase imageFile, string caption)
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (imageFile != null && imageFile.ContentLength > 0)
            {
                string uploadsFolder = Server.MapPath("~/Uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                imageFile.SaveAs(filePath);

                var newPost = new PhotoPost
                {
                    UserId = CurrentUserId.Value,
                    ImageUrl = "/Uploads/" + uniqueFileName,
                    Caption = caption ?? "",
                    CreatedAt = DateTime.Now
                };

                db.PhotoPosts.Add(newPost);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult EditPost(int postId, string newCaption, HttpPostedFileBase newImageFile)
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var post = db.PhotoPosts.Find(postId);
            if (post == null || post.UserId != CurrentUserId.Value)
            {
                return new HttpStatusCodeResult(403, "Bạn không có quyền sửa bài viết này!");
            }

            if (!string.IsNullOrWhiteSpace(newCaption))
            {
                post.Caption = newCaption.Trim();
            }

            if (newImageFile != null && newImageFile.ContentLength > 0)
            {
                if (post.ImageUrl.StartsWith("/Uploads/"))
                {
                    string oldPath = Server.MapPath("~" + post.ImageUrl);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(newImageFile.FileName);
                string newPath = Path.Combine(Server.MapPath("~/Uploads"), uniqueFileName);
                newImageFile.SaveAs(newPath);

                post.ImageUrl = "/Uploads/" + uniqueFileName;
            }

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DeletePost(int postId)
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var post = db.PhotoPosts
                         .Include(p => p.Comments)
                         .Include(p => p.Likes)
                         .FirstOrDefault(p => p.PostId == postId);

            if (post == null || post.UserId != CurrentUserId.Value)
            {
                return new HttpStatusCodeResult(403, "Bạn không có quyền xóa!");
            }

            db.Comments.RemoveRange(post.Comments);
            db.Likes.RemoveRange(post.Likes);

            if (post.ImageUrl.StartsWith("/Uploads/"))
            {
                string physicalPath = Server.MapPath("~" + post.ImageUrl);
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }

            db.PhotoPosts.Remove(post);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ToggleLike(int postId)
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = CurrentUserId.Value;
            var existingLike = db.Likes.FirstOrDefault(l => l.PostId == postId && l.UserId == userId);

            if (existingLike != null)
            {
                db.Likes.Remove(existingLike);
            }
            else
            {
                db.Likes.Add(new Like
                {
                    PostId = postId,
                    UserId = userId
                });
            }

            db.SaveChanges();
            return Redirect(Url.Action("Index", "Home") + "#post-" + postId);
        }

        [HttpPost]
        public ActionResult AddComment(int postId, string content)
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!string.IsNullOrWhiteSpace(content))
            {
                var cmt = new Comment
                {
                    PostId = postId,
                    UserId = CurrentUserId.Value,
                    Content = content.Trim(),
                    CreatedAt = DateTime.Now
                };
                db.Comments.Add(cmt);
                db.SaveChanges();
            }
            return Redirect(Url.Action("Index", "Home") + "#post-" + postId);
        }

        [HttpPost]
        public ActionResult ToggleFollow(int targetUserId)
        {
            if (CurrentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = CurrentUserId.Value;
            if (targetUserId != userId)
            {
                var existingFollow = db.Follows.FirstOrDefault(f => f.FollowerId == userId && f.FollowingId == targetUserId);
                if (existingFollow != null)
                {
                    db.Follows.Remove(existingFollow);
                }
                else
                {
                    db.Follows.Add(new Follow
                    {
                        FollowerId = userId,
                        FollowingId = targetUserId,
                        CreatedAt = DateTime.Now
                    });
                }
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}