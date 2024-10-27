using System;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using iCAREWebExperiment.Models;

namespace iCAREWebExperiment.Controllers
{
    public class AccountController : Controller
    {
        private iCAREDBWebTestEntities db = new iCAREDBWebTestEntities();

        // GET: Login page
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // POST: Handle Login logic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            // SQL query to fetch user and their associated password
            string sqlQuery = @"
            SELECT u.userName, u.roleID, p.encryptedPassword 
            FROM iCAREUser u
            JOIN UserPassword p ON u.ID = p.ID
            WHERE u.userName = @username";

            // Execute SQL command and retrieve user details
            var userRecord = db.Database.SqlQuery<LoginUserModel>(sqlQuery,
                new SqlParameter("@username", username)).FirstOrDefault();

            if (userRecord != null)
            {
                // Validate password using raw comparison of encrypted passwords
                var passwordManager = new UserPassword();
                string hashedEnteredPassword = passwordManager.EncryptPassword(password);

                if (hashedEnteredPassword == userRecord.encryptedPassword)
                {
                    // Store user information in session
                    Session["LoggedUser"] = userRecord.userName;
                    Session["RoleID"] = userRecord.roleID;

                    // Optionally retrieve the roleName from the database
                    string roleQuery = "SELECT roleName FROM UserRole WHERE ID = @roleID";
                    var role = db.Database.SqlQuery<string>(roleQuery,
                        new SqlParameter("@roleID", userRecord.roleID)).FirstOrDefault();

                    if (role != null)
                    {
                        Session["RoleName"] = role;
                    }

                    return RedirectToAction("Index", "Home");  // Redirect on successful login
                }
            }

            // If login fails
            ViewBag.Message = "Invalid username or password.";
            return View();
        }


        // GET: Register page
        [HttpGet]
        public ActionResult Register()
        {
            // Fetch the roles from the UserRole table and pass to the view
            ViewBag.Roles = db.UserRole.ToList();  // Ensure roleID and roleName are available
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(iCAREUser iCAREUser, string password, string selectedRoleID)
        {
            if (ModelState.IsValid)
            {
                // Generate a new GUID for the user ID (this will be used as the foreign key in UserPassword)
                var newUserId = Guid.NewGuid().ToString();

                // Encrypt the password (assuming you have a password manager for encryption)
                var passwordManager = new UserPassword();
                var encryptedPassword = passwordManager.EncryptPassword(password);

                // Convert DateTime to Unix timestamp (int) for passwordExpiryTime
                long passwordExpiryTime = ((DateTimeOffset)DateTime.Now.AddMonths(6)).ToUnixTimeSeconds();

                // Convert userAccountExpiryDate to string in 'YYYY-MM-DD' format for SQL DATE type
                string userAccountExpiryDate = DateTime.Now.AddYears(1).ToString("yyyy-MM-dd");

                // Insert into iCAREUser table with roleID as a string
                string sqlUser = "INSERT INTO iCAREUser (ID, userName, name, email, registrationDate, roleID) " +
                                 "VALUES (@ID, @userName, @name, @Email, @RegistrationDate, @roleID);";

                db.Database.ExecuteSqlCommand(
                    sqlUser,
                    new SqlParameter("@ID", newUserId),
                    new SqlParameter("@userName", iCAREUser.userName),
                    new SqlParameter("@name", iCAREUser.name),
                    new SqlParameter("@Email", iCAREUser.email),
                    new SqlParameter("@RegistrationDate", DateTime.Now.Date),
                    new SqlParameter("@roleID", selectedRoleID)  // Store selected role ID as string
                );

                // Insert into UserPassword table (use newUserId as the foreign key in ID)
                string sqlPassword = "INSERT INTO UserPassword (ID, userName, encryptedPassword, passwordExpiryTime, userAccountExpiryDate) " +
                                     "VALUES (@ID, @userName, @encryptedPassword, @passwordExpiryTime, @userAccountExpiryDate);";

                db.Database.ExecuteSqlCommand(
                    sqlPassword,
                    new SqlParameter("@ID", newUserId),  // Use iCAREUser.ID as the foreign key for UserPassword
                    new SqlParameter("@userName", iCAREUser.userName),
                    new SqlParameter("@encryptedPassword", encryptedPassword),
                    new SqlParameter("@passwordExpiryTime", passwordExpiryTime),  // Insert as Unix timestamp (INT)
                    new SqlParameter("@userAccountExpiryDate", userAccountExpiryDate)  // Insert as Date string
                );

                TempData["SuccessMessage"] = "Registration successful! Please log in.";
                return RedirectToAction("Login");
            }

            ViewBag.Roles = db.UserRole.ToList();  // Reload roles in case of validation error
            return View(iCAREUser);
        }
        
        // POST: Logout user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            // Clear the session to log out the user
            Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
