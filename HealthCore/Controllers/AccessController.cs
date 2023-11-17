using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using HealthCore.Models;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace HealthCore.Controllers
{
    public class AccessController : Controller
    {
        public HealthCoreContext db1;
        public AccessController(HealthCoreContext context)
        {
            db1 = context;
        }
        public IActionResult Login()
        {
            ClaimsPrincipal claimUser = HttpContext.User;

            if (claimUser.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");


            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(VMLogin modelLogin)
        {
            List<User> ListUser = new List<User>();
            ListUser = db1.User.ToList(); 

            User us = new User();
            us = ListUser.FirstOrDefault(g => g.Name == modelLogin.Login & g.Password == md5.hashPassword(modelLogin.PassWord));

            //определяем роли аутенцифицированного пользователя
                               
            
            if (us!= null)
            {
            List<UserRole> usrol = new List<UserRole>();
            usrol = db1.UserRole.Include(h=>h.Role).Where(r=>r.UserId == us.Id).ToList();   

                List<Claim> claims = new List<Claim>() {
                    new Claim(ClaimTypes.NameIdentifier, modelLogin.Login),
                    new Claim(ClaimTypes.Name, modelLogin.Login),

                    new Claim("OtherProperties","Example Role")                    

                };

                foreach (var u in usrol)
                {
                    claims.Add(new Claim(ClaimTypes.Role, u.RoleId.ToString()));
                }


                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                AuthenticationProperties properties = new AuthenticationProperties()
                {

                    AllowRefresh = true,
                    //IsPersistent = modelLogin.KeepLoggedIn
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), properties);
                

                return RedirectToAction("Index", "Home");
            }



            ViewData["ValidateMessage"] = "Некорректные данные!!!";
            return View();
        }
    }
}
