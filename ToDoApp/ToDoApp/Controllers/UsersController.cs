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
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            List<ApplicationUser> users = db.Users.ToList();
            return View(users);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            if (user != null)
                return View(user);

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult Edit(string id, ApplicationUser user)
        {
            ApplicationUser item = db.Users.Find(id);
            if (ModelState.IsValid)
            {
                if (TryUpdateModel(item))
                {
                    item.Email = user.Email;
                    item.PhoneNumber = user.PhoneNumber;
                    item.UserName = user.UserName;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(user);

        }

        [Authorize(Roles = "Administrator")]
        public ActionResult ChangeRole(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            if (user == null)
                return RedirectToAction("Index");

            ViewBag.CurrentRoles = UserRolesToSelectList(user.Roles.ToList());
            ViewBag.AllRoles = RolesToSelectList(db.Roles.ToList());
            return View(user);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult AddRole(string userId, string roleId)
        {
            ApplicationUser user = db.Users.Find(userId);
            IdentityRole role = db.Roles.Find(roleId);
            if (user == null || role == null)
                return RedirectToAction("Index");

            UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            if (!UserManager.GetRoles(user.Id).Contains(role.Name))
                UserManager.AddToRole(user.Id, role.Name);
            return RedirectToAction("ChangeRole", new { id = userId });
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult RemoveRole(string userId, string roleId)
        {
            ApplicationUser user = db.Users.Find(userId);
            IdentityRole role = db.Roles.Find(roleId);
            if (user == null || role == null)
                return RedirectToAction("Index");

            UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            if (UserManager.GetRoles(user.Id).Contains(role.Name))
                if (role.Name == "Manager")
                    return RedirectToAction("ChangeManager", new { userId });
                else
                    UserManager.RemoveFromRole(user.Id, role.Name);

            return RedirectToAction("ChangeRole", new { id = userId });
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult ChangeManager(string userId)
        {
            ApplicationUser user = db.Users.Find(userId);
            if (user == null)
                return RedirectToAction("Index");
            UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            if (!UserManager.GetRoles(user.Id).Contains("Manager"))
                return RedirectToAction("Index");

            List<Team> userTeams = db.Teams.ToList().FindAll(x => x.UserId == userId);
            Dictionary<int, List<SelectListItem>> teamsMembers = new Dictionary<int, List<SelectListItem>>();
            Dictionary<int, string> teamsNames = new Dictionary<int, string>();
            foreach (Team team in userTeams)
            {
                teamsNames.Add(team.TeamId, team.Title);
                List<UserToTeam> currentUsersOfTeam = db.UsersToTeams.ToList().FindAll(x => x.TeamId == team.TeamId && x.UserId != userId);
                if (currentUsersOfTeam.Count > 0)
                {
                    teamsMembers.Add(team.TeamId, MembersToSelectList(db.Users.ToList().FindAll(x => currentUsersOfTeam.Exists(y => y.UserId == x.Id))).ToList());
                }
            }
            if (teamsMembers.Count > 0)
            {
                ViewBag.TeamsMembers = teamsMembers;
                ViewBag.TeamsNames = teamsNames;
                Dictionary<int, string> newManagers = new Dictionary<int, string>();
                foreach (KeyValuePair<int, List<SelectListItem>> item in teamsMembers)
                    newManagers.Add(item.Key, item.Value.First().Value);

                return View(newManagers);
            }
            UserManager.RemoveFromRole(user.Id, "Manager");
            return RedirectToAction("ChangeRole", new { id = userId });
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult ChangeManager(Dictionary<int, string> newManagers)
        {
            List<Team> teams = db.Teams.ToList().FindAll(x => newManagers.ContainsKey(x.TeamId));
            if (teams.Count == 0)
                return RedirectToAction("Index");

            string currentManagerId = teams.First().UserId;
            UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            if (!UserManager.GetRoles(currentManagerId).Contains("Manager"))
                return RedirectToAction("Index");

            if (!teams.All(x => x.UserId == currentManagerId))
                return RedirectToAction("Index");

            foreach (KeyValuePair<int, string> newManager in newManagers)
            {
                Team item = db.Teams.Find(newManager.Key);
                if (TryUpdateModel(item))
                {
                    item.UserId = newManager.Value;
                    item.LastUpdate = DateTime.Now;
                    db.SaveChanges();
                }

                ApplicationUser user = db.Users.Find(newManager.Value);
                if (!UserManager.GetRoles(user.Id).Contains("Manager"))
                    UserManager.AddToRole(currentManagerId, "Manager");
            }

            UserManager.RemoveFromRole(currentManagerId, "Manager");
            return RedirectToAction("Index");

        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(string id)
        {
            ApplicationUser item = db.Users.Find(id);
            db.Users.Remove(item);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> UserRolesToSelectList(List<IdentityUserRole> roles)
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.RoleId.ToString(),
                    Text = db.Roles.Find(role.RoleId).Name
                });
            }

            selectList = selectList.OrderBy(x => x.Text).ToList();

            return selectList;
        }

        [NonAction]
        public IEnumerable<SelectListItem> RolesToSelectList(List<IdentityRole> roles)
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name
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
