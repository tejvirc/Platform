namespace Aristocrat.Monaco.Gaming.Lobby.Controllers;

using Microsoft.AspNetCore.Mvc;

public class AutomationController : Controller
{
    public IActionResult Index()
    {
        return Json(new { Text = "Lobby Automation Controller" });
    }
}
