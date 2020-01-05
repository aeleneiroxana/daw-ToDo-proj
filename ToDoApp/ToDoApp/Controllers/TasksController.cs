using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ToDoApp.Models;
using ToDoApp.Models.Enums;

namespace ToDoApp.Controllers
{
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Details(int id)
        {
            string currentUserId = User.Identity.GetUserId();
            ViewBag.CurrentUserId = currentUserId;
            Task item = db.Tasks.FirstOrDefault(x => x.TaskId == id);

            if (User.IsInRole("Administrator"))
            {
                ViewBag.HasTaskRights = true;
                ViewBag.HasRights = true;
                if (item != null)
                {
                    ViewBag.TaskComments = item.Comments.ToList();
                    ViewBag.NewComment = new Comment()
                    {
                        TaskId = item.TaskId,
                        UserId = currentUserId
                    };
                    return View(item);
                }
                else
                    return RedirectToAction("Index", "Projects");
            }
            else
            {
                ViewBag.HasRights = false;
                ViewBag.HasTaskRights = false;
            }
            if (item == null)
                return RedirectToAction("Index", "Projects");

            Project project = db.Projects.FirstOrDefault(x => x.ProjectId == item.ProjectId);
            if (project == null)
                return RedirectToAction("Index", "Projects");

            if(project.Team.UserId == currentUserId)
                ViewBag.HasTaskRights = true;

            ViewBag.CurrentUserId = currentUserId;

            List<UserToTeam> currentUsersOfTeam = db.UsersToTeams.ToList().FindAll(x => x.TeamId == project.TeamId);
            List<SelectListItem> members = MembersToSelectList(db.Users.ToList().FindAll(x => currentUsersOfTeam.Exists(y => y.UserId == x.Id))).ToList();

            if (!members.Exists(x => x.Value == currentUserId))
                return RedirectToAction("Index", "Projects");

            ViewBag.NewComment = new Comment()
            {
                TaskId = item.TaskId,
                UserId = currentUserId
            };

            ViewBag.TaskComments = item.Comments.ToList();
            return View(item);
        }

        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult Create(int projectId)
        {
            Project project;
            string currentUserId = User.Identity.GetUserId();
            if (User.IsInRole("Administrator"))
                project = db.Projects.FirstOrDefault(x => x.ProjectId == projectId);
            else
                project = db.Projects.FirstOrDefault(x => x.ProjectId == projectId && x.Team.UserId == currentUserId);
            if (project == null)
                return RedirectToAction("Index", "Projects");

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
        public ActionResult Create(Task item)
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
                    return RedirectToAction("Index", "Projects");

                if (TryUpdateModel(item))
                {
                    item.LastUpdate = DateTime.Now;
                    if (item.Status == TaskStatus.InProgress && item.StartDate == null)
                        item.StartDate = DateTime.Now;
                    if (item.Status == TaskStatus.Completed && item.EndDate == null)
                        item.EndDate = DateTime.Now;

                    db.Tasks.Add(item);
                    db.SaveChanges();
                    return RedirectToAction("Details", "Projects", new { id = item.ProjectId });
                }
            }

            return View(item);

        }

        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Edit(int id)
        {
            Task task = db.Tasks.FirstOrDefault(x => x.TaskId == id);
            if (task == null)
                return RedirectToAction("Index", "Projects");

            Project project = db.Projects.FirstOrDefault(x => x.ProjectId == task.ProjectId);
            if (project == null)
                return RedirectToAction("Index", "Projects");

            string currentUserId = User.Identity.GetUserId();
            if (User.IsInRole("Administrator") || project.Team.UserId == currentUserId)
                ViewBag.HasRights = true;
            else
            {
                if (task.AssignedUserId == currentUserId)
                    ViewBag.HasRights = false;
                else
                    return RedirectToAction("Index", "Projects");
            }
            List<UserToTeam> currentUsersOfTeam = db.UsersToTeams.ToList().FindAll(x => x.TeamId == project.TeamId);
            ViewBag.TeamMembers = MembersToSelectList(db.Users.ToList().FindAll(x => currentUsersOfTeam.Exists(y => y.UserId == x.Id)));

            if (task != null)
                return View(task);

            return RedirectToAction("Index", "Projects");
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Edit(int id, Task task)
        {

            Task item = db.Tasks.Find(id);
            string currentUserId = User.Identity.GetUserId();

            Project project = db.Projects.FirstOrDefault(x => x.ProjectId == item.ProjectId);

            if (item == null || project == null)
                return RedirectToAction("Index", "Projects");

            if (!(User.IsInRole("Administrator") || project.Team.UserId == currentUserId || item.AssignedUserId == currentUserId))
                return RedirectToAction("Index", "Projects");

            if (ModelState.IsValid)
            {
                if (item.Status == TaskStatus.NotStarted)
                {
                    if (task.Status == TaskStatus.InProgress && task.StartDate == null)
                        task.StartDate = DateTime.Now;
                }

                if (item.Status != TaskStatus.Completed)
                {
                    if (task.Status == TaskStatus.Completed && task.EndDate == null)
                        task.EndDate = DateTime.Now;
                }

                if (TryUpdateModel(item))
                {
                    item.AssignedUserId = task.AssignedUserId;
                    item.Description = task.Description;
                    item.EndDate = task.EndDate;
                    item.LastUpdate = DateTime.Now;
                    item.StartDate = task.StartDate;
                    item.Status = task.Status;
                    item.Title = task.Title;
                    db.SaveChanges();
                    return RedirectToAction("Details", "Projects", new { id = item.ProjectId });
                }
            }
            return View(task);

        }


        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult Delete(int taskId)
        {
            string currentUserId = User.Identity.GetUserId();

            Task task;
            if (User.IsInRole("Administrator"))
                task = db.Tasks.FirstOrDefault(x => x.TaskId == taskId);
            else
                task = db.Tasks.FirstOrDefault(x => x.TaskId == taskId && x.Project.Team.UserId == currentUserId);
            if (task == null)
                return RedirectToAction("Index", "Projects");

            int projectId = task.ProjectId;
            db.Tasks.Remove(task);
            db.SaveChanges();
            return RedirectToAction("Details", "Projects", new { id = projectId });
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
