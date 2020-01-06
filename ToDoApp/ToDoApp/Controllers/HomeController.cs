﻿using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ToDoApp.Models;

namespace ToDoApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
                return RedirectToAction("Dashboard");

            return View();
        }

        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Dashboard()
        {
            string currentUserId = User.Identity.GetUserId();
            ViewBag.CurrentUserId = currentUserId;

            
            List<Task> tasks = db.Tasks.ToList().FindAll(x => x.AssignedUserId == currentUserId);
            ViewBag.InProgressTasks = tasks.FindAll(x => x.Status == Models.Enums.TaskStatus.InProgress);
            ViewBag.NotStartedTasks = tasks.FindAll(x => x.Status == Models.Enums.TaskStatus.NotStarted);
            ViewBag.CompletedTasks = tasks.FindAll(x => x.Status == Models.Enums.TaskStatus.Completed);
            return View();
        }

    }
}