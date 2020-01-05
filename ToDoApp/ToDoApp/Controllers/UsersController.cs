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
        private readonly Logger Log = new Logger(typeof(UsersController));

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
                    item.UserName = user.UserName;
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Failed to edit user. Error: " + ex.Message);
                    }
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
            try
            {
                if (!UserManager.GetRoles(user.Id).Contains(role.Name))
                    UserManager.AddToRole(user.Id, role.Name);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to add role to user. Error: " + ex.Message);
            }
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
                    try
                    {
                        UserManager.RemoveFromRole(user.Id, role.Name);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Failed to remove role from user. Error: " + ex.Message);
                    }
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

            List<Team> teams = db.Teams.ToList().FindAll(x => x.UserId == userId);

            if (teams.Count == 0)
            {
                try
                {
                    UserManager.RemoveFromRole(userId, "Manager");
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to remove manager from team. Error: " + ex.Message);
                }
                return RedirectToAction("Index");
            }

            IEnumerable<SelectListItem> allUsers = MembersToSelectList(db.Users.ToList().FindAll(x => x.Id != userId));
            TeamsNewManager item = new TeamsNewManager()
            {
                formerManagerId = userId,
                newManagerId = allUsers.First().Value
            };
            ViewBag.AllUsers = allUsers;
            return View(item);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult ChangeManager(TeamsNewManager item)
        {
            ApplicationUser user = db.Users.Find(item.formerManagerId);
            ApplicationUser newManager = db.Users.Find(item.newManagerId);
            UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

            if (user == null || newManager == null || !UserManager.GetRoles(user.Id).Contains("Manager"))
                return RedirectToAction("Index");

            List<Team> teams = db.Teams.ToList().FindAll(x => x.UserId == user.Id);
            foreach (Team team in teams)
            {
                Team actualTeam = db.Teams.Find(team.TeamId);
                if (TryUpdateModel(actualTeam))
                {
                    actualTeam.UserId = item.newManagerId;
                    db.SaveChanges();
                }
                if (!db.UsersToTeams.ToList().Exists(x => x.UserId == item.newManagerId && x.TeamId == team.TeamId))
                {
                    UserToTeam newItem = new UserToTeam()
                    {
                        UserId = newManager.Id,
                        TeamId = team.TeamId
                    };
                    db.UsersToTeams.Add(newItem);
                    db.SaveChanges();
                }

            }
            try
            {
                if (!UserManager.GetRoles(newManager.Id).Contains("Manager"))
                    UserManager.AddToRole(newManager.Id, "Manager");

                UserManager.RemoveFromRole(user.Id, "Manager");
            }
            catch (Exception ex)
            {
                Log.Error("Failed to process changing teams manager. Error: " + ex.Message);
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete]
        public ActionResult Delete(string id)
        {
            ApplicationUser item = db.Users.Find(id);
            try
            {
                db.Users.Remove(item);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to delete user. Error: " + ex.Message);
            }
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
            List<SelectListItem> selectList = new List<SelectListItem>();

            foreach (var user in users)
            {
                selectList.Add(new SelectListItem
                {
                    Value = user.Id.ToString(),
                    Text = user.UserName.ToString()
                });
            }

            selectList = selectList.OrderBy(x => x.Text).ToList();

            return selectList;
        }
    }
}
