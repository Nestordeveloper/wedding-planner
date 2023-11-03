using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using wedding_planner.Models;

namespace wedding_planner.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private MyContext _context;
    public HomeController(ILogger<HomeController> logger, MyContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    [Route("")]
    public IActionResult Index()
    {
        return View();
    }

    // REGISTRO
    [HttpGet]
    [Route("register")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [Route("users/register")]
    public IActionResult ProcessRegister(User newUser)
    {
        try
        {
            if (ModelState.IsValid)
            {
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                _context.Add(newUser);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "¡Registro exitoso! Ingrese sus credenciales en el Log in para iniciar sesión.";

                return RedirectToAction("Index");
            }
            else
            {
                return View("Index");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al registrar el usuario: " + ex.Message);
            return View("Index");
        }
    }



    //LOGIN
    [HttpGet]
    [Route("login")]
    public IActionResult Login()
    {
        return View("Index");
    }

    [HttpPost]
    [Route("users/login")]
    public IActionResult ProcessLogin(LoginUser userSubmission)
    {
        if (ModelState.IsValid)
        {
            User? userInDb = _context.Users.FirstOrDefault(u => u.Email == userSubmission.EmailLog);

            if (userInDb == null)
            {
                ModelState.AddModelError("EmailLog", "This user does not exist");
                ModelState.AddModelError("PasswordLog", "This user does not exist");
                Console.WriteLine("Usuario no registrado error");
                return View("Index");
            }

            PasswordHasher<LoginUser> hasher = new PasswordHasher<LoginUser>();
            var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.PasswordLog);

            if (result == PasswordVerificationResult.Success)
            {
                HttpContext.Session.SetString("Email", userInDb.Email);
                HttpContext.Session.SetString("FirstName", userInDb.FirstName);
                HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                Console.WriteLine("Ingresando a Weddings");
                return RedirectToAction("Weddings");
            }
            else
            {
                ModelState.AddModelError("EmailLog", "Invalid Email/Password");
                ModelState.AddModelError("PasswordLog", "Invalid Email/Password");
                Console.WriteLine("Credenciales invalidas");
                return View("Index");
            }
        }
        else
        {
            return View("Index");
        }
    }

    //LOGOUT
    [HttpPost]
    [Route("logout")]
    public IActionResult ProcessLogout(string logout)
    {
        if (logout == "logout")
        {
            HttpContext.Session.Clear();
            return View("Index");
        }
        return RedirectToAction("Weddings");
    }

    //WEDDINGS
    [SessionCheck]
    [HttpGet]
    [Route("users/weddings")]
    public IActionResult Weddings()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login");
        }

        var weddingViewModels = new List<WeddingViewModel>();

        var weddings = _context.Weddings.Include(w => w.Invitados).ToList();

        foreach (var wedding in weddings)
        {
            bool isInvited = wedding.Invitados.Any(inv => inv.UserId == userId);
            weddingViewModels.Add(new WeddingViewModel { Wedding = wedding, IsInvited = isInvited });
        }

        return View(weddingViewModels);
    }



    // VISTA WEDDING PRINCIPAL TABLA
    [HttpGet]
    [Route("wedding/new")]
    public IActionResult NewWedding()
    {
        return View("NewWedding");
    }


    // WEDDING CREATE POST METHOD
    [HttpPost]
    [Route("wedding/create")]
    public IActionResult CreateWedding(Wedding newWedding)
    {
        if (ModelState.IsValid)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId != null)
            {
                newWedding.UserId = userId.Value;
                _context.Weddings.Add(newWedding);
                _context.SaveChanges();
                int createdWeddingId = newWedding.WeddingId;
                return RedirectToAction("EditWedding", new { WeddingId = createdWeddingId });
            }
        }

        return View("NewWedding");
    }

    //WEDDING DELETE METHOD
    [HttpPost]
    [Route("wedding/delete/{WeddingId}")]
    public IActionResult DeleteWedding(int WeddingId)
    {
        Wedding? wedding = _context.Weddings.FirstOrDefault(wedi => wedi.WeddingId == WeddingId);
        _context.Weddings.Remove(wedding);
        _context.SaveChanges();
        return RedirectToAction("Weddings");
    }

    // WEDDING VIEW GET METHOD
    [HttpGet]
    [Route("wedding/edit/{WeddingId}")]
    public IActionResult EditWedding(int WeddingId)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        var availableUsers = _context.Users.ToList();

        ViewBag.AvailableUsers = availableUsers;
        Wedding wedding = _context.Weddings.FirstOrDefault(wedi => wedi.WeddingId == WeddingId);

        if (wedding == null)
        {
            return RedirectToAction("Weddings");
        }
        wedding.Invitados = _context.Invitations
            .Where(invitation => invitation.WeddingId == wedding.WeddingId)
            .Include(invitation => invitation.User)
            .ToList();

        return View(wedding);
    }


    //WEDDING INVITATIONS
    [HttpPost]
    [Route("wedding/addguest")]
    public IActionResult AddGuest(int WeddingId, int UserId)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (ModelState.IsValid)
        {
            var invitation = new Invitation
            {
                WeddingId = WeddingId,
                UserId = UserId,
                Attendance = AttendanceStatus.Pendiente
            };

            _context.Invitations.Add(invitation);
            _context.SaveChanges();
            return RedirectToAction("EditWedding", new { WeddingId = WeddingId });
        }
        else
        {
            return View("EditWedding");
        }
    }

    //ATTENDANCE INVITATION CHANGE
    [HttpPost]
    [Route("wedding/changeattendance")]
    public IActionResult ChangeAttendanceStatus(int WeddingId, int InvitationId, int userProvidedStatus)
    {
        var invitation = _context.Invitations
            .Include(i => i.Wedding)
            .FirstOrDefault(i => i.InvitationId == InvitationId);

        if (invitation != null)
        {
            if (userProvidedStatus == 0)
            {
                invitation.Attendance = AttendanceStatus.Aceptada;
            }
            else if (userProvidedStatus == 1)
            {
                invitation.Attendance = AttendanceStatus.Rechazada;
            }

            _context.SaveChanges();
        }
        return RedirectToAction("Weddings");
    }

}

public class SessionCheckAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        string? email = context.HttpContext.Session.GetString("Email");

        if (email == null)
        {
            context.Result = new RedirectToActionResult("Login", "Home", null);
        }
    }
}
