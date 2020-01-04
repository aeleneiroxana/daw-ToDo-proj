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
                    item.StartDate = null;
                    item.EndDate = null;
                    db.Tasks.Add(item);
                    db.SaveChanges();
                    return RedirectToAction("Details", "Projects", new { id = item.ProjectId });
                }
            }

            return RedirectToAction("Create", new { projectId = item.ProjectId });

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
