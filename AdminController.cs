using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using iCAREWebExperiment.Models;

namespace iCAREWebExperiment.Controllers
{
    public class AdminController : Controller
    {
        private iCAREDBWebTestEntities db = new iCAREDBWebTestEntities();

        // GET: Admin/Manage
        [HttpGet]
        public ActionResult ManageUsers()
        {
            // Fetch users and join with their roles to get the role name
            string sql = @"
        SELECT u.ID, u.name, u.userName, u.email, r.roleName
        FROM iCAREUser u
        LEFT JOIN UserRole r ON u.roleID = r.ID";

            // Fetching the data and mapping to UserWithRoleViewModel
            var users = db.Database.SqlQuery<UserWithRoleViewModel>(sql).ToList();

            return View(users);  // Pass the users list to the view
        }

        [HttpGet]
        public ActionResult EditUser(string id) // Use 'int id' if ID is an integer
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = db.iCAREUser.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            ViewBag.Roles = db.UserRole.ToList(); // Populate roles for dropdown
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(iCAREUser user, string selectedRoleID)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(selectedRoleID))
                {
                    ModelState.AddModelError("selectedRoleID", "Role must be selected.");
                    ViewBag.Roles = db.UserRole.SqlQuery("SELECT * FROM UserRole").ToList();
                    return View(user);
                }

                string sqlUpdateUser = @"
            UPDATE iCAREUser
            SET userName = @userName, name = @name, email = @email, roleID = @roleID
            WHERE ID = @ID";

                db.Database.ExecuteSqlCommand(
                    sqlUpdateUser,
                    new SqlParameter("@userName", user.userName),
                    new SqlParameter("@name", user.name),
                    new SqlParameter("@email", user.email),
                    new SqlParameter("@roleID", selectedRoleID),
                    new SqlParameter("@ID", user.ID)
                );

                return RedirectToAction("ManageUsers");
            }

            ViewBag.Roles = db.UserRole.SqlQuery("SELECT * FROM UserRole").ToList();
            return View(user);
        }


        // POST: Admin/DeleteUser/{id}
        [HttpPost]
        public ActionResult DeleteUser(string id)
        {
            string sqlDeletePassword = "DELETE FROM UserPassword WHERE ID = @ID";
            db.Database.ExecuteSqlCommand(sqlDeletePassword, new SqlParameter("@ID", id));

            string sqlDeleteUser = "DELETE FROM iCAREUser WHERE ID = @ID";
            db.Database.ExecuteSqlCommand(sqlDeleteUser, new SqlParameter("@ID", id));

            return RedirectToAction("ManageUsers");
        }

        // GET: Admin/AddUser
        [HttpGet]
        public ActionResult AddUser()
        {
            // Fetch available roles to show in the dropdown
            ViewBag.Roles = db.UserRole.ToList();
            return View();
        }

        // POST: Admin/AddUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUser(iCAREUser iCAREUser, string password, string selectedRoleID)
        {
            if (ModelState.IsValid)
            {
                var passwordManager = new UserPassword();
                var encryptedPassword = passwordManager.EncryptPassword(password);

                // Use SQL to insert the new user
                string sqlUser = @"
                    INSERT INTO iCAREUser (ID, userName, name, email, registrationDate, roleID)
                    VALUES (@ID, @userName, @name, @Email, @registrationDate, @roleID)";

                var newUserId = Guid.NewGuid().ToString();  // Generate unique ID

                // SQL command to insert user password
                string sqlPassword = @"
                    INSERT INTO UserPassword (ID, userName, encryptedPassword, passwordExpiryTime, userAccountExpiryDate)
                    VALUES (@ID, @userName, @encryptedPassword, @passwordExpiryTime, @userAccountExpiryDate)";

                // Password and account expiry details
                int passwordExpiryTime = 90;
                DateTime userAccountExpiryDate = DateTime.Now.AddYears(1);

                // Execute the SQL commands using SqlCommand
                db.Database.ExecuteSqlCommand(
                    sqlUser,
                    new SqlParameter("@ID", newUserId),
                    new SqlParameter("@userName", iCAREUser.userName),
                    new SqlParameter("@name", iCAREUser.name),
                    new SqlParameter("@Email", iCAREUser.email),
                    new SqlParameter("@registrationDate", DateTime.Now),
                    new SqlParameter("@roleID", selectedRoleID)
                );

                db.Database.ExecuteSqlCommand(
                    sqlPassword,
                    new SqlParameter("@ID", newUserId),  // Use iCAREUser.ID as the foreign key for UserPassword
                    new SqlParameter("@userName", iCAREUser.userName),
                    new SqlParameter("@encryptedPassword", encryptedPassword),
                    new SqlParameter("@passwordExpiryTime", passwordExpiryTime),  // Insert password expiry time
                    new SqlParameter("@userAccountExpiryDate", userAccountExpiryDate)  // Insert account expiry date
                );

                return RedirectToAction("ManageUsers");
            }

            // Reload roles in case of validation failure
            ViewBag.Roles = db.UserRole.SqlQuery("SELECT * FROM UserRole").ToList();
            return View(iCAREUser);
        }
    }
}
