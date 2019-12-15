using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ToDoApp.Models;

namespace ToDoApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: Users
        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Index()
        {
            List<ApplicationUser> users = db.Users.ToList();
            ViewBag.Users = users;
            return View();
        }
    }
}