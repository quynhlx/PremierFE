using debt_fe.Models;
using debt_fe.SignInManager;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace debt_fe.Controllers
{
    public class AccountController : Controller
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private PremierSignInManager _signInManager;
        private PremierUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(PremierUserManager userManager, PremierSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public PremierSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<PremierSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public PremierUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().Get<PremierUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    if (string.IsNullOrEmpty(returnUrl))
                    {
                        return RedirectToAction("Index", "Document");
                    }
                    return Redirect(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    var user = UserManager.FindByName(model.Username);
                    if (! await SignInManager.SendTwoFactorCodeAsync("Phone Code", user.Id))
                    {
                        ModelState.AddModelError("", "Send Two Factor Code Failed.");
                        return View(model);
                    }
                    return RedirectToAction("VerifyCode", new { Provider = "Phone Code",  ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid Username or Password.");
                    return View(model);
            }
        }

        //
        // POST: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl)
        {
            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync("Phone Code"))
            {
                return Json(new { msg = "Request too long ,Please login again", code = -2 }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { msg = "The authentication code had sent to your phone", code = 1 }, JsonRequestBehavior.AllowGet);
        }
        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = "Phone Code", ReturnUrl = returnUrl, RememberMe = rememberMe });
        }
        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignIn(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    if(string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return RedirectToAction("Index", "Document");
                    }
                    return Redirect(model.ReturnUrl);
                case SignInStatus.Failure:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }







        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
        //
        // POST: /Account/LogOff
        [HttpPost]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }


    }
}