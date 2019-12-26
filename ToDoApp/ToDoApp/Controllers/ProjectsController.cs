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

        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Index()
        {
            if (User.IsInRole("Administrator"))
            {
                ViewBag.HasRights = true;
                ViewBag.Projects = db.Projects.ToList();
            }
            else
            {
                if (User.IsInRole("Manager"))
                {
                    ViewBag.HasRights = true;
                }
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
                {
                    return View(item);
                }
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

        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult Create()
        {
            Project project = new Project();
            if (User.IsInRole("Administrator"))
            {
                ViewBag.Teams = TeamsToSelectList(db.Teams.ToList());
            }
            else
            {
                List<UserToTeam> userTeams = db.UsersToTeams.ToList().FindAll(x => x.UserId == User.Identity.GetUserId());
                ViewBag.Teams = TeamsToSelectList(db.Teams.ToList().FindAll(x => userTeams.Exists(y => y.TeamId == x.TeamId)));
            }
            return View(project);
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost]
        public ActionResult Create(Project project)
        {

            if (ModelState.IsValid)
            {
                string currentUserId = User.Identity.GetUserId();

                UserToProject userToProject = new UserToProject() { ProjectId = project.ProjectId, UserId = currentUserId };

                project.LastUpdate = DateTime.Now;

                db.Projects.Add(project);
                db.UsersToProjects.Add(userToProject);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult Edit(int id)
        {
            Project project;
            if (User.IsInRole("Administrator"))
            {
                project = db.Projects.FirstOrDefault(x => x.ProjectId == id);
                ViewBag.Teams = TeamsToSelectList(db.Teams.ToList());
            }
            else
            {
                project = db.Projects.FirstOrDefault(x => x.ProjectId == id && x.Team.UserId == User.Identity.GetUserId());
                List<UserToTeam> userTeams = db.UsersToTeams.ToList().FindAll(x => x.UserId == User.Identity.GetUserId());
                ViewBag.Teams = TeamsToSelectList(db.Teams.ToList().FindAll(x => userTeams.Exists(y => y.TeamId == x.TeamId)));
            }
            if (project != null)
                return View(project);

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult Edit(int id, Project project)
        {
            Project item = db.Projects.Find(id);

            if (User.IsInRole("Administrator") || item.Team.UserId == User.Identity.GetUserId())
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

        [Authorize(Roles = "Administrator,Manager")]
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Project item = db.Projects.Find(id);
            if (User.IsInRole("Administrator") || item.Team.UserId == User.Identity.GetUserId())
            {
                db.Projects.Remove(item);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> TeamsToSelectList(List<Team> teams)
        {
            var selectList = new List<SelectListItem>();

            foreach (var team in teams)
            {
                selectList.Add(new SelectListItem
                {
                    Value = team.TeamId.ToString(),
                    Text = team.Title.ToString()
                });
            }
            return selectList;
        }
    }
}
