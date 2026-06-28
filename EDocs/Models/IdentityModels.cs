// Developed by: Mahmoud Shaheen (mahmoudshaheensy@gmail.com)
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data;
using System.Collections;

namespace edocs.Models
{
    public class UserRole : IdentityUserRole<int>
    {
    }

    public class UserClaim : IdentityUserClaim<int>
    {
    }

    public class UserLogin : IdentityUserLogin<int>
    {
    }

    public class Role : IdentityRole<int, UserRole>
    {
        public Role() { }
        public Role(string name) { Name = name; }
    }

    public class UserStore : UserStore<ApplicationUser, Role, int,
        UserLogin, UserRole, UserClaim>
    {
        public UserStore(ApplicationDbContext context) : base(context)
        {
        }
    }

    public class RoleStore : RoleStore<Role, int, UserRole>
    {
        public RoleStore(ApplicationDbContext context) : base(context)
        {
        }
    }



    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser<int, UserLogin, UserRole, UserClaim>
    {

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        public string USERTYPE { get; set; }

        public string FULLNAME { get; set; }
        public virtual ICollection<Documenti> DOCUMENTS { get; set; }

        public virtual ICollection<DocFile> DocFile { get; set; }


    }

    //Added For Add Role
  
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, Role, int, UserLogin, UserRole, UserClaim>
    {

        public ApplicationDbContext()
            : base("name=IdentityContext")
        {
        }

         protected override void OnModelCreating(DbModelBuilder modelBuilder)
         {
             base.OnModelCreating(modelBuilder); // MUST go first.
                         
             this.Configuration.LazyLoadingEnabled = false;
             this.Configuration.ProxyCreationEnabled = false;

             modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers");
             modelBuilder.Entity<edocs.Models.Category>().ToTable("Category");
             modelBuilder.Entity<edocs.Models.DocAttribute>().ToTable("DocAttribute");
             modelBuilder.Entity<edocs.Models.Documenti>().ToTable("Documenti");
             modelBuilder.Entity<edocs.Models.UserCategory>().ToTable("UserCategory");
             modelBuilder.Entity<Role>().ToTable("AspNetRoles");
             modelBuilder.Entity<edocs.Models.AttributeList>().ToTable("AttributeList");
          
         }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<edocs.Models.Category> Category { get; set; }

        public System.Data.Entity.DbSet<edocs.Models.DocAttribute> DocAttribute { get; set; }

        public System.Data.Entity.DbSet<edocs.Models.Documenti> Documenti { get; set; }

        public System.Data.Entity.DbSet<edocs.Models.DocAttributes> DocAttributes { get; set; }

        public System.Data.Entity.DbSet<edocs.Models.DocFile> DocFile { get; set; }

        public System.Data.Entity.DbSet<edocs.Models.DocContent> DocContent { get; set; }
        public System.Data.Entity.DbSet<edocs.Models.UserCategory> UserCategory { get; set; }
        public System.Data.Entity.DbSet<edocs.Models.AttributeList> AttributeList { get; set; }

        object placeHolderVariable;
        
        public IEnumerable ApplicationUsers { get; internal set; }

        public System.Data.Entity.DbSet<edocs.Models.DocMessage> DocMessage { get; set; }

    }
}