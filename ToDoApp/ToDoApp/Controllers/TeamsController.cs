using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ToDoApp.Controllers
{
    public class TeamsController : Controller
    {
        // GET: Teams
        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Index()
        {
            return View();
        }
    }
}