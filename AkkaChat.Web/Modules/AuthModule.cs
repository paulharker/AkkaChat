using System;
using AkkaChat.Messages;
using AkkaChat.Services;
using AkkaChat.ViewModels;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.ModelBinding;
using Nancy.Security;

namespace AkkaChat.Modules
{
    /// <summary>
    /// Module responsible for handling authentication and account creation
    /// </summary>
    public class AuthModule : NancyModule
    {
        private readonly IMembershipService _membership;

        public AuthModule(IMembershipService membership)
        {
            _membership = membership;

            Get["/login"] = _ =>
            {
                //already logged in
                if (Context.CurrentUser.IsAuthenticated())
                    return Response.AsRedirect("~/");
                return View["login"];
            };

            //login attempt
            Post["/login"] = _ =>
            {
                var loginAttempt = this.Bind<LoginAttempt>();
                var validationError = "";
                var failedValidation = false;
                if (string.IsNullOrEmpty(loginAttempt.UserName))
                {
                    failedValidation = true;
                    validationError += string.Format("Must provide a username!<br>");
                }
                if (string.IsNullOrEmpty(loginAttempt.Password))
                {
                    failedValidation = true;
                    validationError += string.Format("Must provide a password!<br>");
                }

                if (failedValidation)
                {
                    ViewBag.ValidationError = validationError;
                    return View["login"];
                }

                AuthenticationSuccess success;
                if (_membership.TryAuthenticateUser(loginAttempt.UserName, loginAttempt.Password, out success))
                {
                    return this.LoginAndRedirect(success.CookieId, DateTime.Now.AddDays(30), "~/");
                }
                
                ViewBag.ValidationError = "Invalid username and password combination.";
                return View["login"];
            };

            Get["/register"] = _ =>
            {
                //already logged in
                if (Context.CurrentUser.IsAuthenticated())
                    return Response.AsRedirect("~/");
                return View["register"];
            };

            Post["/register", true] = async (x, ct) =>
            {
                var registerAttempt = this.Bind<NewUser>();
                var validationError = "";
                var failedValidation = false;
                if (string.IsNullOrEmpty(registerAttempt.UserName))
                {
                    failedValidation = true;
                    validationError += string.Format("Must provide a username!<br>");
                }
                else
                {
                    //check to see if a username is available
                    var userNameAvailable = await _membership.IsUserNameAvailable(registerAttempt.UserName);
                    if (!userNameAvailable)
                    {
                        validationError += string.Format("{0} is already taken. Please pick another username.<br>",
                            registerAttempt.UserName);
                        failedValidation = true;
                    }
                }
                if (string.IsNullOrEmpty(registerAttempt.Password))
                {
                    failedValidation = true;
                    validationError += string.Format("Must provide a password!<br>");
                }
                if (string.IsNullOrEmpty(registerAttempt.Email))
                {
                    failedValidation = true;
                    validationError += string.Format("Must provide an email!<br>");
                }

                if (failedValidation)
                {
                    ViewBag.ValidationError = validationError;
                    return View["register"];
                }

                var registerResult = await _membership.AddUser(registerAttempt.UserName, registerAttempt.Email, registerAttempt.Password);

                //success!
                if (!(registerResult is MissingUserIdentity))
                {
                    return this.LoginAndRedirect(registerResult.CookieId, DateTime.Now.AddDays(30), "~/");
                }
                else //failure!
                {
                    ViewBag.ValidationError = string.Format("Unable to register as {0} - server error.", registerAttempt.UserName);
                    return View["register"];
                }

            };

            Get["/logout"] = _ =>
            {
                return Context.CurrentUser.IsAuthenticated() ? 
                    this.LogoutAndRedirect("~/") 
                    : Response.AsRedirect("~/");
            };
        }
    }
}