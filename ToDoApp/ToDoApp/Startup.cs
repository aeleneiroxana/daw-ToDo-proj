using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using ToDoApp.Models;

[assembly: OwinStartupAttribute(typeof(ToDoApp.Startup))]

namespace ToDoApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            CreateAdminUserAndApplicationRoles();
        }

        private void CreateAdminUserAndApplicationRoles()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            context.Configuration.LazyLoadingEnabled = true;

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            if(!roleManager.RoleExists("Administrator"))
            {
                IdentityRole role = new IdentityRole
                {
                    Name = "Administrator"
                };
                roleManager.Create(role);

                ApplicationUser user = new ApplicationUser
                {
                    UserName = "admin@admin.com",
                    Email = "admin@admin.com"
                };
                IdentityResult adminCreated = UserManager.Create(user, "Administrator1!");
                if(adminCreated.Succeeded)
                {
                    UserManager.AddToRole(user.Id, "Administrator");
                }
            }
            if(!roleManager.RoleExists("Manager"))
            {
                IdentityRole role = new IdentityRole
                {
                    Name = "Manager"
                };
                roleManager.Create(role);
            }
            if(!roleManager.RoleExists("User"))
            {
                IdentityRole role = new IdentityRole
                {
                    Name = "User"
                };
                roleManager.Create(role);
            }
        }
    }
}