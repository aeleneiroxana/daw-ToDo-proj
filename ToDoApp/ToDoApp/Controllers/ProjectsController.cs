using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using PagedList;
using ToDoApp.Models;

namespace ToDoApp.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private readonly Logger Log = new Logger(typeof(ProjectsController));

        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Index(int? i)
        {
            List<Project> projects;
            if(User.IsInRole("Administrator"))
            {
                ViewBag.HasRights = true;
                projects = db.Projects.ToList().OrderBy(x => x.Title).ToList();
            }
            else
            {
                if(User.IsInRole("Manager"))
                    ViewBag.HasRights = true;
                else
                    ViewBag.HasRights = false;
                string currentUserId = User.Identity.GetUserId();
                List<Team> teams = db.Teams.ToList().FindAll(x => db.UsersToTeams.ToList().Exists(y => y.UserId == currentUserId && y.TeamId == x.TeamId));

                projects = db.Projects.ToList().FindAll(x => teams.Exists(y => y.TeamId == x.TeamId)).OrderBy(x => x.Title).ToList();
            }
            return View(projects.ToPagedList(i ?? 1, 10));
        }

        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Details(int id, int? i)
        {
            ViewBag.CurrentUserId = User.Identity.GetUserId();
            if(User.IsInRole("Administrator"))
            {
                ViewBag.HasRights = true;
                Project item = db.Projects.FirstOrDefault(x => x.ProjectId == id);
                if(item != null)
                {
                    ViewBag.ProjectTasks = item.Tasks.ToList().OrderBy(x => x.Title).ToPagedList(i ?? 1, 5);
                    return View(item);
                }
                else
                    return RedirectToAction("Index");
            }

            string currentUserId = User.Identity.GetUserId();

            List<Team> teams = db.Teams.ToList().FindAll(x => db.UsersToTeams.ToList().Exists(y => y.UserId == currentUserId && y.TeamId == x.TeamId));
            List<Project> projects = db.Projects.ToList().FindAll(x => teams.Exists(y => y.TeamId == x.TeamId));

            if(projects.Exists(x => x.ProjectId == id))
            {
                Project item = projects.FirstOrDefault(x => x.ProjectId == id);

                if(item.Team.UserId == currentUserId)
                    ViewBag.HasRights = true;
                else
                    ViewBag.HasRights = false;

                ViewBag.ProjectTasks = item.Tasks.ToList().OrderBy(x => x.Title).ToPagedList(i ?? 1, 5);
                return View(item);
            }
            else
                return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult Create()
        {
            Project project = new Project();
            string currentUserId = User.Identity.GetUserId();
            if(User.IsInRole("Administrator"))
            {
                ViewBag.Teams = TeamsToSelectList(db.Teams.ToList());
            }
            else
            {
                List<UserToTeam> userTeams = db.UsersToTeams.ToList().FindAll(x => x.UserId == currentUserId);
                ViewBag.Teams = TeamsToSelectList(db.Teams.ToList().FindAll(x => userTeams.Exists(y => y.TeamId == x.TeamId) && x.UserId == currentUserId));
            }
            return View(project);
        }

        [Authorize(Roles = "Administrator, Manager")]
        [HttpPost]
        public ActionResult Create(Project project)
        {
            string currentUserId = User.Identity.GetUserId();

            if(ModelState.IsValid)
            {
                project.LastUpdate = DateTime.Now;
                try
                {
                    db.Projects.Add(project);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch(Exception ex)
                {
                    Log.Error("Failed to create project. Error: " + ex.Message);
                    ModelState.AddModelError("Title", "Title should be unique. The title you chose may have already been taken");
                }
            }
            if(User.IsInRole("Administrator"))
            {
                ViewBag.Teams = TeamsToSelectList(db.Teams.ToList());
            }
            else
            {
                List<UserToTeam> userTeams = db.UsersToTeams.ToList().FindAll(x => x.UserId == currentUserId);
                ViewBag.Teams = TeamsToSelectList(db.Teams.ToList().FindAll(x => userTeams.Exists(y => y.TeamId == x.TeamId) && x.UserId == currentUserId));
            }
            return View(project);
        }

        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult Edit(int id)
        {
            Project project;
            string currentUserId = User.Identity.GetUserId();
            if(User.IsInRole("Administrator"))
            {
                project = db.Projects.FirstOrDefault(x => x.ProjectId == id);
            }
            else
            {
                project = db.Projects.FirstOrDefault(x => x.ProjectId == id && x.Team.UserId == currentUserId);
            }
            if(project != null)
                return View(project);

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult Edit(int id, Project project)
        {
            Project item = db.Projects.Find(id);
            string currentUserId = User.Identity.GetUserId();

            if(User.IsInRole("Administrator") || item.Team.UserId == currentUserId)
            {
                if(ModelState.IsValid)
                {
                    if(TryUpdateModel(item))
                    {
                        item.Description = project.Description;
                        item.Title = project.Title;
                        try
                        {
                            db.SaveChanges();
                            return RedirectToAction("Index");
                        }

                        catch(Exception ex)
                        {
                            Log.Error("Failed to edit project. Error: " + ex.Message);
                            ModelState.AddModelError("Title", "Title should be unique. The title you chose may have already been taken");
                        }
                    }
                }
                return View(project);
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult Delete(int id)
        {
            Project item = db.Projects.Find(id);
            string currentUserId = User.Identity.GetUserId();

            if(User.IsInRole("Administrator") || item.Team.UserId == currentUserId)
            {
                try
                {
                    db.Projects.Remove(item);
                    db.SaveChanges();
                }
                catch(Exception ex)
                {
                    Log.Error("Failed to delete project. Error: " + ex.Message);
                }
            }
            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> TeamsToSelectList(List<Team> teams)
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            foreach(var team in teams)
            {
                selectList.Add(new SelectListItem
                {
                    Value = team.TeamId.ToString(),
                    Text = team.Title.ToString()
                });
            }

            selectList = selectList.OrderBy(x => x.Text).ToList();
            return selectList;
        }
    }
}