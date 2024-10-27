//using System.Web.Mvc;

//namespace iCAREWebExperiment.Controllers
//{
//    public class HomeController : Controller
//    {
//        // GET: Home
//        public ActionResult Index()
//        {
//            // Check if the user is logged in
//            if (Session["LoggedUser"] == null)
//            {
//                return RedirectToAction("Login", "Account");
//            }

//            // Retrieve the roleID from the session
//            var roleID = Session["RoleID"]?.ToString();
//            var roleName = Session["RoleName"]?.ToString();

//            if (string.IsNullOrEmpty(roleID) || string.IsNullOrEmpty(roleName))
//            {
//                ViewBag.Message = "Your role is not defined. Please contact the administrator.";
//                return View();
//            }

//            // Redirect based on roleID
//            switch (roleID)
//            {
//                case "1": // Admin
//                    return RedirectToAction("ManageUsers", "Admin");

//                case "2": // Doctor
//                    ViewBag.Username = Session["LoggedUser"];
//                    ViewBag.Role = "Doctor";
//                    return View("Index");  // Ensure you have a DoctorHome.cshtml view

//                case "3": // Nurse
//                    ViewBag.Username = Session["LoggedUser"];
//                    ViewBag.Role = "Nurse";
//                    return View("Index");  // Ensure you have a NurseHome.cshtml view

//                default:
//                    ViewBag.Message = "Your role is not recognized. Please contact the administrator.";
//                    return View();
//            }
//        }
//    }
//}

using System.Web.Mvc;

namespace iCAREWebExperiment.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            // Check if the user is logged in
            if (Session["LoggedUser"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Retrieve role details from session
            var roleID = Session["RoleID"]?.ToString();
            var roleName = Session["RoleName"]?.ToString();

            // If no role is defined
            if (string.IsNullOrEmpty(roleID) || string.IsNullOrEmpty(roleName))
            {
                ViewBag.Message = "Your role is not defined. Please contact the administrator.";
                return View();
            }

            // Pass role information to the view
            ViewBag.RoleID = roleID;
            ViewBag.RoleName = roleName;

            return View(); // Render the Index.cshtml view for all roles
        }
    }
}
