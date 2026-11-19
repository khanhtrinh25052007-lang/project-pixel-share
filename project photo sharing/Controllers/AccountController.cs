using project_photo_sharing.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace project_photo_sharing.Controllers
{
    public class AccountController : Controller
    {
        private PhotoSharingDbContext db = new PhotoSharingDbContext();

        // GET: Account/Login
        public ActionResult Login()
        {
            if (Session["UserId"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == password);
            if (user != null)
            {
                Session["UserId"] = user.UserId;
                Session["Username"] = user.Username;
                Session["AvatarUrl"] = string.IsNullOrEmpty(user.AvatarUrl) ? "/images/default-avatar.png" : user.AvatarUrl;

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Email hoặc mật khẩu không chính xác!";
            return View();
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            if (Session["UserId"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        public ActionResult Register(User model, HttpPostedFileBase AvatarFile)
        {
            if (db.Users.Any(u => u.Email == model.Email))
            {
                ViewBag.Error = "Email này đã được sử dụng!";
                return View();
            }

            string avatarPath = "/images/default-avatar.png";
            if (AvatarFile != null && AvatarFile.ContentLength > 0)
            {
                string uploadsFolder = Server.MapPath("~/Uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(AvatarFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                AvatarFile.SaveAs(filePath);

                avatarPath = "/Uploads/" + uniqueFileName;
            }

            model.AvatarUrl = avatarPath;
            model.CreatedAt = DateTime.Now;

            db.Users.Add(model);
            db.SaveChanges();

            return RedirectToAction("Login");
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Account");
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