using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ToDoApp.Models;


namespace ToDoApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            ViewBag.Users = db.Users.ToList();
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Details(int id)
        {
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Create()
        {
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(int id)
        {
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(int id)
        {
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
        }
    }
}
