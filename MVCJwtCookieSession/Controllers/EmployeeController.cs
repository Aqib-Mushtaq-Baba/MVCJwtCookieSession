using Microsoft.AspNetCore.Mvc;
using MVCJwtCookieSession.DbContext;
using MVCJwtCookieSession.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace MVCJwtCookieSession.Controllers
{
    

    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Utility to validate token from cookie/session
        private bool IsTokenValid()
        {
            string token = HttpContext.Request.Cookies["jwt_token"] ?? HttpContext.Session.GetString("jwt_token");
            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                return jwt.ValidTo > DateTime.UtcNow;
            }
            catch
            {
                return false;
            }
        }

        public IActionResult Index()
        {
            if (!IsTokenValid()) return RedirectToAction("Login", "Account");
            var employees = _context.Employees.ToList();
            return View(employees);
        }

        public IActionResult Create()
        {
            if (!IsTokenValid()) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        public IActionResult Create(Employee emp)
        {
            if (!IsTokenValid()) return RedirectToAction("Login", "Account");
            _context.Employees.Add(emp);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            if (!IsTokenValid()) return RedirectToAction("Login", "Account");
            var emp = _context.Employees.Find(id);
            return View(emp);
        }

        [HttpPost]
        public IActionResult Edit(Employee emp)
        {
            if (!IsTokenValid()) return RedirectToAction("Login", "Account");
            _context.Employees.Update(emp);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            if (!IsTokenValid()) return RedirectToAction("Login", "Account");
            var emp = _context.Employees.Find(id);
            _context.Employees.Remove(emp);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }

}
