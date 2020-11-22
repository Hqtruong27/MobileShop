namespace Models.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Models.DataModels;

    internal sealed class Configuration : DbMigrationsConfiguration<MobileShopContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(MobileShopContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
            UserAdminSapmple(context);
            RoleSample(context);
        }
        private void UserAdminSapmple(MobileShopContext context)
        {
            if (context.Users.Count() == 0)
            {
                context.Users.AddOrUpdate(new User { UserId = 1, UserName = "Admin", FullName = "Truong Hoang ", Email = "hqtruong27@gmail.com", Password = BCrypt.Net.BCrypt.HashPassword("111111"), Phone = "0963712001", Avatar = "", IsAdmin = true, Status = 1, mStatus = 1, isEmailVerified = true, ActiveCode = new Guid(), ResetPasswordCode = "", GroupId = null });
                context.SaveChanges();
            }
        }
        private void RoleSample(MobileShopContext context)
        {
            if (context.Roles.Count() == 0)
            {
                context.Roles.AddOrUpdate(new Role { RoleId = "VIEW", RoleName = "VIEW", Status = 1 });
                context.Roles.AddOrUpdate(new Role { RoleId = "CREATE", RoleName = "CREATE", Status = 1 });
                context.Roles.AddOrUpdate(new Role { RoleId = "UPDATE", RoleName = "UPDATE", Status = 1 });
                context.Roles.AddOrUpdate(new Role { RoleId = "DELETE", RoleName = "DELETE", Status = 1 });
                context.SaveChanges();
            }
        }
    }
}
