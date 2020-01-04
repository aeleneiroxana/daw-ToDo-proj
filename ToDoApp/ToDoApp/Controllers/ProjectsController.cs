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
                    ViewBag.HasRights = true;
                else
                    ViewBag.HasRights = false;
                string currentUserId = User.Identity.GetUserId();
                //List<UserToProject> userProjects = db.UsersToProjects.ToList().FindAll(x => x.UserId == currentUserId);
                List<Team> teams = db.Teams.ToList().FindAll(x => db.UsersToTeams.ToList().Exists(y => y.UserId == currentUserId && y.TeamId == x.TeamId));

                ViewBag.Projects = db.Projects.ToList().FindAll(x => teams.Exists(y => y.TeamId == x.TeamId));
            }
            return View();
        }

        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Details(int id)
        {

            if (User.IsInRole("Administrator"))
            {
                ViewBag.HasRights = true;
                Project item = db.Projects.FirstOrDefault(x => x.ProjectId == id);
                if (item != null)
                {
                    ViewBag.ProjectTasks = item.Tasks.ToList();
                    return View(item);
                }
                else
                    return RedirectToAction("Index");
            }

            string currentUserId = User.Identity.GetUserId();

           
            List<Team> teams = db.Teams.ToList().FindAll(x => db.UsersToTeams.ToList().Exists(y => y.UserId == currentUserId && y.TeamId == x.TeamId));
            List<Project> projects = db.Projects.ToList().FindAll(x => teams.Exists(y => y.TeamId == x.TeamId));

            if (projects.Exists(x => x.ProjectId == id))
            {
                Project item = projects.FirstOrDefault(x => x.ProjectId == id);

                if (item.Team.UserId == currentUserId)
                    ViewBag.HasRights = true;
                else
                    ViewBag.HasRights = false;


                ViewBag.ProjectTasks = item.Tasks.ToList();
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
            if (User.IsInRole("Administrator"))
            {
                ViewBag.Teams = TeamsToSelectList(db.Teams.ToList());
            }
            else
            {
                List<UserToTeam> userTeams = db.UsersToTeams.ToList().FindAll(x => x.UserId == currentUserId);
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

                //UserToProject userToProject = new UserToProject() { ProjectId = project.ProjectId, UserId = currentUserId };

                project.LastUpdate = DateTime.Now;

                db.Projects.Add(project);
                //db.UsersToProjects.Add(userToProject);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult Edit(int id)
        {
            Project project;
            string currentUserId = User.Identity.GetUserId();
            if (User.IsInRole("Administrator"))
            {
                project = db.Projects.FirstOrDefault(x => x.ProjectId == id);
                ViewBag.Teams = TeamsToSelectList(db.Teams.ToList());
            }
            else
            {
                project = db.Projects.FirstOrDefault(x => x.ProjectId == id && x.Team.UserId == currentUserId);
                List<UserToTeam> userTeams = db.UsersToTeams.ToList().FindAll(x => x.UserId == currentUserId);
                ViewBag.Teams = TeamsToSelectList(db.Teams.ToList().FindAll(x => userTeams.Exists(y => y.TeamId == x.TeamId) && x.UserId == currentUserId));
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
            string currentUserId = User.Identity.GetUserId();

            if (User.IsInRole("Administrator") || item.Team.UserId == currentUserId)
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
                return RedirectToAction("Edit", new { id });
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult Delete(int id)
        {
            Project item = db.Projects.Find(id);
            string currentUserId = User.Identity.GetUserId();

            if (User.IsInRole("Administrator") || item.Team.UserId == currentUserId)
            {
                db.Projects.Remove(item);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult AddTask(int projectId)
        {
            Project project;
            string currentUserId = User.Identity.GetUserId();
            if (User.IsInRole("Administrator"))
                project = db.Projects.FirstOrDefault(x => x.ProjectId == projectId);
            else
                project = db.Projects.FirstOrDefault(x => x.ProjectId == projectId && x.Team.UserId == currentUserId);
            if (project == null)
                return RedirectToAction("Index");

            Task task = new Task()
            {
                ProjectId = projectId
            };

            List<UserToTeam> currentUsersOfTeam = db.UsersToTeams.ToList().FindAll(x => x.TeamId == project.TeamId);
            ViewBag.TeamMembers = MembersToSelectList(db.Users.ToList().FindAll(x => currentUsersOfTeam.Exists(y => y.UserId == x.Id)));

            return View(task);
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost]
        public ActionResult AddTask(Task item)
        {
            if (ModelState.IsValid)
            {
                Project project;
                string currentUserId = User.Identity.GetUserId();
                if (User.IsInRole("Administrator"))
                    project = db.Projects.FirstOrDefault(x => x.ProjectId == item.ProjectId);
                else
                    project = db.Projects.FirstOrDefault(x => x.ProjectId == item.ProjectId && x.Team.UserId == currentUserId);
                if (project == null)
                    return RedirectToAction("Index");

                if (TryUpdateModel(item))
                {
                    item.LastUpdate = DateTime.Now;
                    item.StartDate = null;
                    item.EndDate = null;
                    db.Tasks.Add(item);
                    db.SaveChanges();
                    return RedirectToAction("Details", new { id = item.ProjectId });
                }
            }

            return RedirectToAction("AddTask", new { projectId = item.ProjectId });

        }

        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult RemoveTask(int taskId)
        {
            string currentUserId = User.Identity.GetUserId();

            Task task;
            if (User.IsInRole("Administrator"))
                task = db.Tasks.FirstOrDefault(x => x.TaskId == taskId);
            else
                task = db.Tasks.FirstOrDefault(x => x.TaskId == taskId && x.Project.Team.UserId == currentUserId);
            if (task == null)
                return RedirectToAction("Index");

            int projectId = task.ProjectId;
            db.Tasks.Remove(task);
            db.SaveChanges();
            return RedirectToAction("Details", new { id = projectId });
        }

        [NonAction]
        public IEnumerable<SelectListItem> TeamsToSelectList(List<Team> teams)
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            foreach (var team in teams)
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

        [NonAction]
        public IEnumerable<SelectListItem> MembersToSelectList(List<ApplicationUser> users)
        {
            List<SelectListItem> selectList = new List<SelectListItem>
            {
                new SelectListItem { Value = null, Text = "None" }
            };

            List<SelectListItem> partialSelectList = new List<SelectListItem>();

            foreach (var user in users)
            {
                partialSelectList.Add(new SelectListItem
                {
                    Value = user.Id.ToString(),
                    Text = user.UserName.ToString()
                });
            }

            selectList = selectList.Concat(partialSelectList.OrderBy(x => x.Text)).ToList();

            return selectList;
        }
    }
}