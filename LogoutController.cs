using System.Web.Mvc;

public class LogoutController : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Index()
    {
        // Clear the session to log out the user
        Session.Clear();

        // Redirect to the Login page (in AccountController)
        return RedirectToAction("Login", "Account");
    }
}
