using Files_cloud_manager.Server.Domain.Interfaces;
using Files_cloud_manager.Models;
using Files_cloud_manager.Server.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Files_cloud_manager.Server.Controllers
{
    [Route("[controller]/[action]")]
    public class AuthenticationController : Controller
    {
        IUnitOfWork _unitOfWork;
        HashAlgorithm _hashAlgorithm;
        public AuthenticationController(HashAlgorithm hashAlgorithm, IUnitOfWork unitOfWork)
        {
            this._hashAlgorithm = hashAlgorithm;
            this._unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Login(string login, string password, string? ReturnUrl)
        {
            User? user = _unitOfWork.UserRepository.Find(login);

            if (!(user is null))
            {
                if (user.PasswordHash != _hashAlgorithm.ComputeHash(password.Select(e=>(byte)e).ToArray()))
                    return View();
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, user.Login),
                     new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role,user.Role.RoleName)
                };
                var claimsIdentity = new ClaimsIdentity(claims, "Cookies");

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                return Redirect(ReturnUrl is null ? "/" : ReturnUrl);
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout(string Void)
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult RegisterUser()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult RegisterUser(string login, string password, int roleId)
        {
            if (!(login.Count() > 3 && login.Count() < 30 && login.All(c => char.IsLetterOrDigit(c))))
            {
                return BadRequest("Логин должен содержать только буквы и цифры и быть длиной от 3 до 30");
            }
            if (!(password.Count() >= 3 && password.Count() < 30 && password.All(c => char.IsLetterOrDigit(c))))
            {
                return BadRequest("Пароль должен содержать только буквы и цифры и быть длиной от 3 до 30");
            }
            if (!(_unitOfWork.UserRepository.Find(login) is null))
            {
                return BadRequest("Логин занят");
            }
            _unitOfWork.UserRepository.Create(
                new User() {
                    Login = login,
                    PasswordHash = _hashAlgorithm.ComputeHash(password.Select(e=>(byte)e).ToArray()), 
                    RoleId = roleId 
                });
            _unitOfWork.Save();
            return View();
        }
    }
}
