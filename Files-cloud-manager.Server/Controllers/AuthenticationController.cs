using Files_cloud_manager.Server.Domain.Interfaces;
using Files_cloud_manager.Server.Models;
using Files_cloud_manager.Server.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Files_cloud_manager.Server.Domain;
using System.Data;
using static Files_cloud_manager.Server.Domain.AppDBContext;
using System.ComponentModel.DataAnnotations;

namespace Files_cloud_manager.Server.Controllers
{
    [Route("[controller]/[action]")]
    public class AuthenticationController : Controller
    {
        IUnitOfWork _unitOfWork;
        IHashAlgorithmFactory _hashAlgFactory;
        public AuthenticationController(IHashAlgorithmFactory hashAlgFactory, IUnitOfWork unitOfWork)
        {
            _hashAlgFactory = hashAlgFactory;
            _unitOfWork = unitOfWork;
        }

        [RequireHttps]
        [HttpPost]
        public async Task<IActionResult> Login(string login, string password)
        {
            User? user = _unitOfWork.UserRepository.Find(login);

            if (!(user is null))
            {
                if (!user.PasswordHash.SequenceEqual(_hashAlgFactory.Create().ComputeHash(password.Select(e => (byte)e).ToArray())))
                    return BadRequest();
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, user.Login),
                     new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role,user.Role.RoleName)
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                return Ok();
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Ok();
        }



        [HttpPost]
        [RequireHttps]
        [Authorize(Roles = "Admin")]
        public IActionResult RegisterUser(string login, string password, string roleName)
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

            Role? role = _unitOfWork.RoleRepository.Get(e => e.RoleName == roleName).FirstOrDefault();

            if (role is null)
            {
                return BadRequest($"Нет роли с именем {roleName}");
            }

            _unitOfWork.UserRepository.Create(
                new User()
                {
                    Login = login,
                    PasswordHash = _hashAlgFactory.Create().ComputeHash(password.Select(e => (byte)e).ToArray()),
                    RoleId = role.Id,
                    UserFoldersPath = login
                });
            _unitOfWork.Save();
            return Ok();
        }
        [HttpPost]
        [RequireHttps]
        [Authorize]
        public IActionResult ChangePassword(string oldPassword, string password)
        {
            User? user = _unitOfWork.UserRepository.Find(GetUserId());
            if (user is null)
                return BadRequest("Пользователь не найден");
            if (!_hashAlgFactory.Create().ComputeHash(oldPassword.Select(e => (byte)e).ToArray()).SequenceEqual(user.PasswordHash))
            {
                return BadRequest("Старый пароль введён не верно");
            }

            if (!(password.Count() > 2 && password.Count() < 30 && password.All(c => char.IsLetterOrDigit(c))))
            {
                return BadRequest("Новый пароль должен содержать только буквы и цифры и быть длиной от 8 до 30");
            }

            user.PasswordHash = _hashAlgFactory.Create().ComputeHash(password.Select(e => (byte)e).ToArray());
            _unitOfWork.UserRepository.Update(user);
            _unitOfWork.Save();
            return Ok();
        }
        [HttpPost]
        [RequireHttps]
        [Authorize(Roles = "Admin")]
        public IActionResult ChangeUserPassword(string userLogin, string password)
        {
            User? user = _unitOfWork.UserRepository.Find(userLogin);
            if (user is null)
                return BadRequest("Пользователь не найден");
            user.PasswordHash = password.Select(e => (byte)e).ToArray();
            _unitOfWork.UserRepository.Update(user);
            _unitOfWork.Save();
            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteUser(string userLogin)
        {
            User? user = _unitOfWork.UserRepository.Find(userLogin);
            if (user is null)
                return BadRequest("Пользователь не найден");
            _unitOfWork.UserRepository.Delete(user);
            _unitOfWork.Save();
            return Ok();
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult ChangeRole(string userLogin, string roleName)
        {
            User? user = _unitOfWork.UserRepository.Find(userLogin);
            if (user is null)
            {
                return BadRequest("Запрошенный пользователь не существует.");
            }
            Role? newRole = _unitOfWork.RoleRepository.Get(e => e.RoleName == roleName).FirstOrDefault();
            if (newRole is null)
            {
                return BadRequest("Запрошена не существующая роль.");
            }
            user.Role = newRole;

            _unitOfWork.UserRepository.Update(user);
            _unitOfWork.Save();
            return Ok($"Пользователю установлена роль {newRole.RoleName}");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> RefreshCoockie()
        {
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(User.Identity));
            return Ok();
        }

        [NonAction]
        private int GetUserId()
        {
            return int.Parse(User.Claims.First(e => e.Type == ClaimTypes.NameIdentifier).Value);
        }

    }
}
