﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PagedList;
using ToDoApp.Models;

namespace ToDoApp.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private readonly Logger Log = new Logger(typeof(TeamsController));

        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Index(int? i)
        {
            List<Team> teams;
            if(User.IsInRole("Administrator"))
            {
                teams = db.Teams.ToList().OrderBy(x => x.Title).ToList();
            }
            else
            {
                string currentUserId = User.Identity.GetUserId();
                List<UserToTeam> userTeams = db.UsersToTeams.ToList().FindAll(x => x.UserId == currentUserId);
                teams = db.Teams.ToList().FindAll(x => userTeams.Exists(y => y.TeamId == x.TeamId)).OrderBy(x => x.Title).ToList();
            }
            return View(teams.ToPagedList(i ?? 1, 10));
        }

        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Details(int id, int? i)
        {
            Team item = db.Teams.FirstOrDefault(x => x.TeamId == id);
            if(item == null)
                return RedirectToAction("Index");

            ViewBag.TeamId = item.TeamId;

            List<UserToTeam> userTeams = db.UsersToTeams.ToList().FindAll(x => x.TeamId == item.TeamId && x.UserId != item.UserId);
            if(User.IsInRole("Administrator"))
            {
                ViewBag.HasRights = true;
                ViewBag.TeamMembers = db.Users.ToList().FindAll(x => userTeams.Exists(y => y.UserId == x.Id)).OrderBy(x => x.UserName).ToPagedList(i ?? 1, 5);
                if(ViewBag.TeamMembers == null)
                    ViewBag.TeamMembers = new PagedList<ApplicationUser>(new List<ApplicationUser>(), 1, 0);
                return View(item);
            }

            string currentUserId = User.Identity.GetUserId();

            if(item.UserId == currentUserId)
                ViewBag.HasRights = true;
            else
                ViewBag.HasRights = false;

            userTeams = db.UsersToTeams.ToList().FindAll(x => x.TeamId == item.TeamId && x.UserId != item.UserId);
            if(item.UserId != currentUserId && !userTeams.Exists(x => x.UserId == currentUserId))
                return RedirectToAction("Index");

            ViewBag.TeamMembers = db.Users.ToList().FindAll(x => userTeams.Exists(y => y.UserId == x.Id)).OrderBy(x => x.UserName).ToPagedList(i ?? 1, 5);
            ;
            if(ViewBag.TeamMembers == null)
                ViewBag.TeamMembers = new PagedList<ApplicationUser>(new List<ApplicationUser>(), 1, 0);
            return View(item);
        }

        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Create()
        {
            Team team = new Team();
            return View(team);
        }

        [Authorize(Roles = "Administrator,Manager,User")]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(Team team)
        {
            if(ModelState.IsValid)
            {
                string currentUserId = User.Identity.GetUserId();
                ApplicationUser user = db.Users.FirstOrDefault(x => x.Id == currentUserId);
                UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));

                if(!UserManager.GetRoles(user.Id).Contains("Manager"))
                    UserManager.AddToRole(user.Id, "Manager");

                UserToTeam userToTeam = new UserToTeam() { TeamId = team.TeamId, UserId = currentUserId };
                team.LastUpdate = DateTime.Now;
                team.UserId = User.Identity.GetUserId();
                try
                {
                    db.Teams.Add(team);
                    db.UsersToTeams.Add(userToTeam);
                    db.SaveChanges();
                    return RedirectToAction("Details", new { id = team.TeamId });
                }

                catch(Exception ex)
                {
                    Log.Error("Failed to create team. Error: " + ex.Message);
                    ModelState.AddModelError("Title", "Title should be unique. The title you chose may have already been taken");
                }
            }
            return View(team);
        }

        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult Edit(int id)
        {
            Team team;
            string currentUserId = User.Identity.GetUserId();
            if(User.IsInRole("Administrator"))
                team = db.Teams.FirstOrDefault(x => x.TeamId == id);
            else
                team = db.Teams.FirstOrDefault(x => x.TeamId == id && x.UserId == currentUserId);
            if(team != null)
                return View(team);

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost]
        public ActionResult Edit(int id, Team team)
        {
            Team item = db.Teams.Find(id);
            string currentUserId = User.Identity.GetUserId();

            if(User.IsInRole("Administrator") || item.UserId == currentUserId)
            {
                if(ModelState.IsValid)
                {
                    if(TryUpdateModel(item))
                    {
                        item.Title = team.Title;
                        try
                        {
                            db.SaveChanges();
                            return RedirectToAction("Index");
                        }
                        catch(Exception ex)
                        {
                            Log.Error("Failed to edit team. Error: " + ex.Message);
                            ModelState.AddModelError("Title", "Title should be unique. The title you chose may have already been taken");
                        }
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
            string currentUserId = User.Identity.GetUserId();

            if(User.IsInRole("Administrator") || item.UserId == currentUserId)
            {
                try
                {
                    db.Teams.Remove(item);
                    db.SaveChanges();
                }
                catch(Exception ex)
                {
                    Log.Error("Failed to delete team. Error: " + ex.Message);
                }
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult AddMember(int teamId)
        {
            Team team;
            string currentUserId = User.Identity.GetUserId();
            if(User.IsInRole("Administrator"))
                team = db.Teams.FirstOrDefault(x => x.TeamId == teamId);
            else
                team = db.Teams.FirstOrDefault(x => x.TeamId == teamId && x.UserId == currentUserId);
            if(team == null)
                return RedirectToAction("Index");

            UserToTeam item = new UserToTeam();
            item.TeamId = teamId;
            List<UserToTeam> currentUsersOfTeam = db.UsersToTeams.ToList().FindAll(x => x.TeamId == teamId);
            ViewBag.Members = MembersToSelectList(db.Users.ToList().FindAll(x => x.Id != currentUserId && !currentUsersOfTeam.Exists(y => y.UserId == x.Id)));
            return View(item);
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost]
        public ActionResult AddMember(UserToTeam item)
        {
            Team team;
            string currentUserId = User.Identity.GetUserId();
            if(User.IsInRole("Administrator"))
                team = db.Teams.FirstOrDefault(x => x.TeamId == item.TeamId);
            else
                team = db.Teams.FirstOrDefault(x => x.TeamId == item.TeamId && x.UserId == currentUserId);
            if(team == null)
                return RedirectToAction("Index");

            if(ModelState.IsValid)
            {
                if(TryUpdateModel(item))
                {
                    try
                    {
                        db.UsersToTeams.Add(item);
                        db.SaveChanges();
                    }
                    catch(Exception ex)
                    {
                        Log.Error("Failed to add member to team. Error: " + ex.Message);
                    }
                    return RedirectToAction("Details", new { id = team.TeamId });
                }
            }
            List<UserToTeam> currentUsersOfTeam = db.UsersToTeams.ToList().FindAll(x => x.TeamId == team.TeamId);
            ViewBag.Members = MembersToSelectList(db.Users.ToList().FindAll(x => x.Id != currentUserId && !currentUsersOfTeam.Exists(y => y.UserId == x.Id)));

            return View(item);
        }

        [Authorize(Roles = "Administrator,Manager")]
        public ActionResult RemoveMember(int teamId, string memberId)
        {
            Team team;
            string currentUserId = User.Identity.GetUserId();

            if(User.IsInRole("Administrator"))
                team = db.Teams.FirstOrDefault(x => x.TeamId == teamId);
            else
                team = db.Teams.FirstOrDefault(x => x.TeamId == teamId && x.UserId == currentUserId);
            if(team == null)
                return RedirectToAction("Index");

            UserToTeam item = db.UsersToTeams.Find(teamId, memberId);
            try
            {
                db.UsersToTeams.Remove(item);
                db.SaveChanges();
            }
            catch(Exception ex)
            {
                Log.Error("Failed to remove member from team. Error: " + ex.Message);
            }
            return RedirectToAction("Details", new { id = teamId });
        }

        [NonAction]
        public IEnumerable<SelectListItem> MembersToSelectList(List<ApplicationUser> users)
        {
            List<SelectListItem> selectList = new List<SelectListItem>();

            foreach(var user in users)
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