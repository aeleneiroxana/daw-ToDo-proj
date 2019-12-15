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
    public class ProjectsController : Controller
    {
        private readonly ProjectDBContext db = new ProjectDBContext();

        private readonly ApplicationDbContext userContextdb = new ApplicationDbContext();
        // GET: Projects
        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Index()
        {
            ViewBag.proj = db.Projects.ToList();
            return View();
        }

        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Details(int id)
        {
            Project item = db.Projects.FirstOrDefault(x => x.ProjectId == id);
            if (item != null)
                return View(item);
            else
                return RedirectToAction("Index");
        }

        // GET: Projects/Create
        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        [Authorize(Roles = "Administrator,Manager,User")]
        [HttpPost]
        public ActionResult Create(Project project)
        {

            if (ModelState.IsValid)
            {
                ApplicationUser user = userContextdb.Users.FirstOrDefault(x => x.Id == User.Identity.GetUserId());
                UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContextdb));

                if (!UserManager.GetRoles(user.Id).Contains("Manager"))
                    UserManager.AddToRole(user.Id, "Manager");

                //TODO: link project with manager, not team

            }
            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [Authorize(Roles = "Administrator,Manager")]
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

        // GET: Projects/Delete/5
        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Projects/Delete/5
        [HttpPost]
        [Authorize(Roles = "Administrator,Manager")]
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

        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();

            var roles = from role in userContextdb.Roles select role;
            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }
    }
}
