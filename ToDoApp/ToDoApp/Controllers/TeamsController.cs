using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
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
                if (User.IsInRole("Manager"))
                    ViewBag.HasRights = true;
                else
                    ViewBag.HasRights = false;
                List<UserToTeam> userTeams = db.UsersToTeams.ToList().FindAll(x => x.UserId == User.Identity.GetUserId());
                ViewBag.Teams = db.Teams.ToList().FindAll(x => userTeams.Exists(y => y.TeamId == x.TeamId));
            }
            return View(ViewBag.Teams);
        }

        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Details(int id)
        {
            Team item = db.Teams.FirstOrDefault(x => x.TeamId == id);
            if (item == null)
                return RedirectToAction("Index");

            ViewBag.TeamId = item.TeamId;

            List<UserToTeam> userTeams = db.UsersToTeams.ToList().FindAll(x => x.TeamId == item.TeamId && x.UserId != item.UserId);
            if (User.IsInRole("Administrator"))
            {
                ViewBag.HasRights = true;
                ViewBag.TeamMembers = (List<ApplicationUser>)db.Users.ToList().FindAll(x => userTeams.Exists(y => y.UserId == x.Id));
                if (ViewBag.TeamMembers == null)
                    ViewBag.TeamMembers = new List<ApplicationUser>();
                return View(item);
            }

            if (item.UserId == User.Identity.GetUserId())
                ViewBag.HasRights = true;
            else
                ViewBag.HasRights = false;

            if (userTeams.Exists(x => x.TeamId == id))
            {
                userTeams = db.UsersToTeams.ToList().FindAll(x => x.TeamId == item.TeamId && x.UserId != item.UserId);
                ViewBag.TeamMembers = (List<ApplicationUser>)db.Users.ToList().FindAll(x => userTeams.Exists(y => y.UserId == x.Id));
                if (ViewBag.TeamMembers == null)
                    ViewBag.TeamMembers = new List<ApplicationUser>();
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
            Team team;
            if (User.IsInRole("Administrator"))
                team = db.Teams.FirstOrDefault(x => x.TeamId == teamId);
            else
                team = db.Teams.FirstOrDefault(x => x.TeamId == teamId && x.UserId == User.Identity.GetUserId());
            if (team == null)
                return RedirectToAction("Index");

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
                Team team;
                if (User.IsInRole("Administrator"))
                    team = db.Teams.FirstOrDefault(x => x.TeamId == item.TeamId);
                else
                    team = db.Teams.FirstOrDefault(x => x.TeamId == item.TeamId && x.UserId == User.Identity.GetUserId());
                if (team == null)
                    return RedirectToAction("Index");

                if (TryUpdateModel(item))
                {
                    db.UsersToTeams.Add(item);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            return View(item);

        }


        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult RemoveMember(int teamId, string memberId)
        {
            Team team;
            if (User.IsInRole("Administrator"))
                team = db.Teams.FirstOrDefault(x => x.TeamId == teamId);
            else
                team = db.Teams.FirstOrDefault(x => x.TeamId == teamId && x.UserId == User.Identity.GetUserId());
            if (team == null)
                return RedirectToAction("Index");

            UserToTeam item = db.UsersToTeams.Find(teamId, memberId);
            db.UsersToTeams.Remove(item);
            db.SaveChanges();
            return RedirectToAction("Details", new { id = teamId });
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
