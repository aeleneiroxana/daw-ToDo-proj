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
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: Projects
        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Index()
        {
            if (User.IsInRole("Administrator"))
                ViewBag.Projects = db.Projects.ToList();
            else
            {
                List<UserToProject> userProjects = db.UsersToProjects.ToList().FindAll(x => x.UserId == User.Identity.GetUserId());
                ViewBag.Projects = db.Projects.ToList().FindAll(x => userProjects.Exists(y => y.ProjectId == x.ProjectId));
            }
            return View();
        }

        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Details(int id)
        {

            if (User.IsInRole("Administrator"))
            {
                Project item = db.Projects.FirstOrDefault(x => x.ProjectId == id);
                if (item != null)
                    return View(item);
                else
                    return RedirectToAction("Index");
            }

            List<UserToProject> userProjects = db.UsersToProjects.ToList().FindAll(x => x.UserId.ToString() == User.Identity.GetUserId());
            if (userProjects.Exists(x => x.ProjectId == id))
            {
                Project item = db.Projects.FirstOrDefault(x => x.ProjectId == id);
                return View(item);
            }
            else
                return RedirectToAction("Index");
        }

        // GET: Projects/Create
        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Create()
        {
            Project project = new Project();
            project.UserId = User.Identity.GetUserId();
            return View(project);
        }

        // POST: Projects/Create
        [Authorize(Roles = "Administrator,Manager,User")]
        [HttpPost]
        public ActionResult Create(Project project)
        {

            if (ModelState.IsValid)
            {
                ApplicationUser user = db.Users.FirstOrDefault(x => x.Id == User.Identity.GetUserId());
                UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

                if (!UserManager.GetRoles(user.Id).Contains("Manager"))
                    UserManager.AddToRole(user.Id, "Manager");

                db.Projects.Add(project);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult Edit(int id)
        {
            Project project;
            if (User.IsInRole("Administrator"))
                project = db.Projects.FirstOrDefault(x => x.ProjectId == id);
            else
                project = db.Projects.FirstOrDefault(x => x.ProjectId == id && x.UserId == User.Identity.GetUserId());
            if (project != null)
                return View(project);

            return RedirectToAction("Index");
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult Edit(int id, Project project)
        {
            Project item = db.Projects.Find(id);

            if (User.IsInRole("Administrator") || item.UserId == User.Identity.GetUserId())
            {
                if (ModelState.IsValid)
                {
                    if (TryUpdateModel(item))
                    {
                        item.Description = project.Description;
                        item.Title = project.Title;
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
                return View(project);
            }
            return RedirectToAction("Index");
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "Administrator,Manager")]
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Project item = db.Projects.Find(id);
            if (User.IsInRole("Administrator") || item.UserId == User.Identity.GetUserId())
            {
                db.Projects.Remove(item);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();

            var roles = from role in db.Roles select role;
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
