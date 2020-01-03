using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ToDoApp.Models;

namespace ToDoApp.Controllers
{
    public class TeamsController : Controller
    {

        private readonly ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Index()
        {
            if (User.IsInRole("Administrator"))
            {
                ViewBag.Teams = db.Teams.ToList();
                ViewBag.HasRights = true;
            }
            else
            {
                UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                string currentUserId = User.Identity.GetUserId();
                ApplicationUser user = db.Users.FirstOrDefault(x => x.Id == currentUserId);

                if (User.IsInRole("Manager") || UserManager.GetRoles(user.Id).Contains("Manager"))
                {
                    ViewBag.HasRights = true;
                }
                List<UserToTeam> userTeams = db.UsersToTeams.ToList().FindAll(x => x.UserId == User.Identity.GetUserId());
                ViewBag.Teams = db.Teams.ToList().FindAll(x => userTeams.Exists(y => y.TeamId == x.TeamId));
            }
            return View(ViewBag.Teams);
        }

        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Details(int id)
        {
            if (User.IsInRole("Administrator"))
            {
                Team item = db.Teams.FirstOrDefault(x => x.TeamId == id);
                if (item != null)
                    return View(item);
                else
                    return RedirectToAction("Index");
            }

            List<UserToTeam> userTeams = db.UsersToTeams.ToList().FindAll(x => x.UserId.ToString() == User.Identity.GetUserId());
            if (userTeams.Exists(x => x.TeamId == id))
            {
                Team item = db.Teams.FirstOrDefault(x => x.TeamId == id);
                return View(item);
            }
            else
                return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Create()
        {
            Team team = new Team();
            team.UserId = User.Identity.GetUserId();
            return View(team);
        }

        [Authorize(Roles = "Administrator,Manager,User")]
        [HttpPost]
        public ActionResult Create(Team team)
        {
            if (ModelState.IsValid)
            {
                string currentUserId = User.Identity.GetUserId();
                ApplicationUser user = db.Users.FirstOrDefault(x => x.Id == currentUserId);
                UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

                if (!UserManager.GetRoles(user.Id).Contains("Manager"))
                    UserManager.AddToRole(user.Id, "Manager");

                UserToTeam userToTeam = new UserToTeam() { TeamId = team.TeamId, UserId = currentUserId };
                team.LastUpdate = DateTime.Now;

                db.Teams.Add(team);
                db.UsersToTeams.Add(userToTeam);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(team);
        }

        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult Edit(int id)
        {
            Team team;
            if (User.IsInRole("Administrator"))
                team = db.Teams.FirstOrDefault(x => x.TeamId == id);
            else
                team = db.Teams.FirstOrDefault(x => x.TeamId == id && x.UserId == User.Identity.GetUserId());
            if (team != null)
                return View(team);

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost]
        public ActionResult Edit(int id, Team team)
        {
            Team item = db.Teams.Find(id);

            if (User.IsInRole("Administrator") || item.UserId == User.Identity.GetUserId())
            {
                if (ModelState.IsValid)
                {
                    if (TryUpdateModel(item))
                    {
                        item.Title = team.Title;
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }
                return View(team);
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Team item = db.Teams.Find(id);
            if (User.IsInRole("Administrator") || item.UserId == User.Identity.GetUserId())
            {
                db.Teams.Remove(item);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult AddMember(int teamId)
        {
            UserToTeam item = new UserToTeam();
            item.TeamId = teamId;
            List<UserToTeam> currentUsersOfTeam = db.UsersToTeams.ToList().FindAll(x => x.TeamId == teamId);
            ViewBag.Members = MembersToSelectList(db.Users.ToList().FindAll(x => x.Id != User.Identity.GetUserId() && !currentUsersOfTeam.Exists(y => y.UserId == x.Id)));
            return View(item);
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost]
        public ActionResult AddMember(UserToTeam item)
        {
            if (ModelState.IsValid)
            {
                if (TryUpdateModel(item))
                {
                    db.UsersToTeams.Add(item);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            return View(item);

        }

        [NonAction]
        public IEnumerable<SelectListItem> MembersToSelectList(List<ApplicationUser> users)
        {
            var selectList = new List<SelectListItem>();

            foreach (var user in users)
            {
                selectList.Add(new SelectListItem
                {
                    Value = user.Id.ToString(),
                    Text = user.UserName.ToString()
                });
            }
            return selectList;
        }

    }
}
