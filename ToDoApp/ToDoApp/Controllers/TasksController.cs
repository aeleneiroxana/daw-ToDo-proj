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
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tasks
        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Index()
        {
            if(User.IsInRole("Administrator"))
                ViewBag.Tasks = db.Tasks.ToList();
            else
            {

            }
            return View();
        }

        // GET: Tasks/Details/5
        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Details(int id)
        {
            if (User.IsInRole("Administrator"))
            {
                Task item = db.Tasks.FirstOrDefault(x => x.TaskId == id);
                if (item != null)
                    return View(item);
                else
                    return RedirectToAction("Index");
            }
            return View();
        }

        // GET: Tasks/Create
        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Create()
        {
            //Task task = new Task();
            //task.User = User.Identity.GetUserId();
            return View();
        }

        // POST: Tasks/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Tasks/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Tasks/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Tasks/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Tasks/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
