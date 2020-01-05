using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ToDoApp.Models;

namespace ToDoApp.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private readonly Logger Log = new Logger(typeof(CommentsController));

        [Authorize(Roles = "Administrator,Manager,User")]
        [HttpPost]
        public ActionResult Create(Comment comment)
        {

            if (ModelState.IsValid)
            {
                comment.LastUpdate = DateTime.Now;
                comment.DateAdded = DateTime.Now;

                db.Comments.Add(comment);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to create comment. Error: " + ex.Message);
                }
                return RedirectToAction("Details", "Tasks", new { id = comment.TaskId });
            }
            return RedirectToAction("Details", "Tasks", new { id = comment.TaskId });
        }

        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Edit(int id)
        {
            Comment comment = db.Comments.FirstOrDefault(x => x.CommentId == id);
            if (comment == null)
                return RedirectToAction("Index", "Projects");


            string currentUserId = User.Identity.GetUserId();
            if (User.IsInRole("Administrator") || comment.UserId == currentUserId)
                return View(comment);

            return RedirectToAction("Index", "Projects");
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Edit(int id, Comment comment)
        {

            Comment item = db.Comments.Find(id);
            string currentUserId = User.Identity.GetUserId();

            if (item == null)
                return RedirectToAction("Index", "Projects");

            if (!(User.IsInRole("Administrator") || item.UserId == currentUserId))
                return RedirectToAction("Index", "Projects");

            if (ModelState.IsValid)
            {

                if (TryUpdateModel(item))
                {
                    item.Content = comment.Content;
                    item.LastUpdate = DateTime.Now;
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Failed to edit comment. Error: " + ex.Message);
                    }
                    return RedirectToAction("Details", "Tasks", new { id = item.TaskId });
                }
            }
            return View(comment);

        }


        [Authorize(Roles = "Administrator,Manager,User")]
        public ActionResult Delete(int commentId)
        {
            Comment item = db.Comments.Find(commentId);
            if (item == null)
                return RedirectToAction("Index", "Projects");

            string currentUserId = User.Identity.GetUserId();

            if (!(User.IsInRole("Administrator") || item.UserId == currentUserId))
                return RedirectToAction("Index", "Projects");

            int taskId = item.TaskId;
            try
            {
                db.Comments.Remove(item);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to delete comment. Error: " + ex.Message);
            }
            return RedirectToAction("Details", "Tasks", new { id = taskId });
        }

    }
}
