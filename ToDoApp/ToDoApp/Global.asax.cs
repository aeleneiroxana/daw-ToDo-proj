using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ToDoApp.Models;

namespace ToDoApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<ApplicationDbContext>());
            //Database.SetInitializer(new DropCreateDatabaseAlways<ApplicationDbContext>());
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<UsersToTeamsDBContext>());
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<TeamDBContext>());
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<CommentDBContext>());
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<TaskDBContext>());
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<UsersToProjectsDBContext>());
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<ProjectDBContext>());

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
