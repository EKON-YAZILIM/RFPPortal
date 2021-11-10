﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PagedList.Core;
using RFPPortalWebsite.Contexts;
using RFPPortalWebsite.Methods;
using RFPPortalWebsite.Models.DbModels;
using RFPPortalWebsite.Models.SharedModels;
using RFPPortalWebsite.Models.ViewModels;
using RFPPortalWebsite.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static RFPPortalWebsite.Models.Constants.Enums;

namespace RFPPortalWebsite.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Rfps");
        }

        public IActionResult Unauthorized()
        {
            return View();
        }

        [Route("Rfps")]
        [Route("Rfps/{Page}")]
        public IActionResult Rfps(int Page = 1)
        {
            PagedList.Core.IPagedList<Rfp> model = new PagedList<Rfp>(null, 1, 1);

            if (HttpContext.Session.GetString("UserType") == Models.Constants.Enums.UserIdentityType.Internal.ToString() || HttpContext.Session.GetString("UserType") == Models.Constants.Enums.UserIdentityType.Admin.ToString())
            {
                model = Methods.RfpMethods.GetRfpsByStatusPaged(null, Page, 5);
            }
            else
            {
                model = Methods.RfpMethods.GetRfpsByStatusPaged(Models.Constants.Enums.RfpStatusTypes.Public.ToString(), Page, 5);
            }

            ViewBag.PageTitle = "DEVxDAO - Request for Proposals Portal";
            return View(model);
        }

        [AdminUserAuthorization]
        [Route("Rfp-Form")]
        public IActionResult Rfp_Form()
        {
            ViewBag.PageTitle = "RFP Form";
            return View();
        }

        [AdminUserAuthorization]
        [Route("SubmitForm")]
        public IActionResult SubmitForm(Rfp model)
        {
            model.UserId = (int)HttpContext.Session.GetInt32("UserId");
            model.CreateDate = DateTime.Now;
            model.Status = Models.Constants.Enums.RfpStatusTypes.Internal.ToString();

            Rfp usr = Methods.RfpMethods.SubmitRfpForm(model);
            if (usr.RfpID > 0)
            {
                return Json(new SimpleResponse() { Success = true, Message = "Proposal submitted successfully." });
            }

            return Json(new SimpleResponse() { Success = false, Message = "Submission failed." });
        }

        [Route("RFP-Detail/{BidID}")]
        public IActionResult RFP_Detail(int BidID)
        {
            RfpController cont = new RfpController();
            Models.ViewModels.RfpDetailModel model = new Models.ViewModels.RfpDetailModel();
            try
            {
                model.RfpDeatil = cont.GetRfpById(BidID);
                model.BidList = cont.GetRfpBidsByRfpId(BidID);

                if (model.RfpDeatil.Status == Models.Constants.Enums.RfpStatusTypes.Internal.ToString() && HttpContext.Session.GetString("UserType") == Models.Constants.Enums.UserIdentityType.Public.ToString())
                {
                    return RedirectToAction("Unauthorized");
                }
            }
            catch (Exception)
            {
                return View(new List<Rfp>());
            }
            ViewBag.PageTitle = "RFP Detail";
            return View(model);
        }

        [UserAuthorization]
        [Route("My-Bids")]
        public IActionResult My_Bids()
        {
            ViewBag.PageTitle = "My Bids";

            List<MyBidsModel> model = Methods.BidMethods.GetUserBids(Convert.ToInt32(HttpContext.Session.GetInt32("UserId")));

            return View(model);
        }


        #region Login & Register Methods

        /// <summary>
        ///  User login function
        /// </summary>
        /// <param name="email">User's email or username</param>
        /// <param name="password">User's password</param>
        /// <param name="usercode">Captcha code (Needed after 3 failed requests)</param>
        /// <returns></returns>
        [Route("SignIn")]
        public IActionResult SignIn(string email, string pass)
        {
            SimpleResponse res = Methods.AuthMethods.UserSignIn(email, pass);
            if (res.Success)
            {
                var usr = res.Content as User;
                HttpContext.Session.SetInt32("UserId", usr.UserId);
                HttpContext.Session.SetString("UserType", usr.UserType);
                HttpContext.Session.SetString("NameSurname", usr.NameSurname);
                HttpContext.Session.SetString("Email", usr.Email);
                HttpContext.Session.SetString("Username", usr.UserName);
                return Json(res);
            }

            return Json(res);
        }

        /// <summary>
        ///  Public user registration method
        ///  This method can be accessed by every user
        /// </summary>
        /// <param name="registerInput">Registration information of the user</param>
        /// <returns>AjaxResponse object with registration result</returns>
        [HttpPost("RegisterUser", Name = "RegisterUser")]
        public SimpleResponse RegisterUser([FromBody] RegisterModel registerInput)
        {
            try
            {
                //Validations
                using (rfpdb_context db = new rfpdb_context())
                {
                    //Email already exists control
                    if (db.Users.Count(x => x.Email == registerInput.Email) > 0)
                    {
                        return new SimpleResponse() { Success = false, Message = "Email already exists." };
                    }

                    //Username already exists control
                    if (db.Users.Count(x => x.UserName == registerInput.UserName) > 0)
                    {
                        return new SimpleResponse() { Success = false, Message = "Username already exists." };
                    }
                }

                User usr = AuthMethods.UserRegister(registerInput);

                if (usr.UserId > 0)
                {

                    if (this.Request != null)
                    {
                        //Create encrypted activation key for email approval
                        string enc = Encryption.EncryptString(registerInput.Email + "|" + DateTime.Now.ToString());

                        var baseUrl = $"{this.Request.Scheme}://{this.Request.Host.Value.ToString()}{this.Request.PathBase.Value.ToString()}";

                        //Set email title and content
                        string emailTitle = "Welcome to RFP Portal";
                        string emailContent = "Thank you for your registration. Please use the link below to activate your account. <br><br> <a href='" + baseUrl + "/RegisterCompleteView?str=" + enc + "'>Click here to complete the registration.</a>";

                        //Send email
                        EmailHelper.SendEmail(emailTitle, emailContent, new List<string>() { usr.Email }, new List<string>(), new List<string>());
                    }

                    return new SimpleResponse() { Success = true, Message = "User registration succesful.Please verify your account from your email.", Content = new User { Email = usr.Email } };
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError);
            }

            return new SimpleResponse() { Success = false, Message = "Unexpected error" };
        }

        /// <summary>
        /// Completes user registration from activation link in the confirmation email
        /// </summary>
        /// <param name="str">Encrypted user information in the registration email</param>
        /// <returns></returns>
        [Route("RegisterCompleteView")]
        public IActionResult RegisterCompleteView(string str)
        {
            SimpleResponse resp = new SimpleResponse();

            User usr = AuthMethods.RegisterComplete(str);

            if (usr.UserId > 0)
            {
                TempData["toastr-type"] = "success";
                TempData["toastr-message"] = "User activation successful.";
            }
            else
            {
                TempData["toastr-type"] = "error";
                TempData["toastr-message"] = "Invalid user activation request.";
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// User logout function
        /// </summary>
        /// <returns></returns>
        [Route("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Sends password reset email to user's email
        /// </summary>
        /// <param name="email">User's email</param>
        /// <param name="usercode">Captcha code (Needed after 3 failed requests)</param>
        /// <returns></returns>
        [Route("ResetPassword")]
        [HttpPost]
        public JsonResult ResetPassword(string email)
        {

            try
            {
                //Validations
                using (rfpdb_context db = new rfpdb_context())
                {
                    //Email does not exist
                    if (db.Users.Count(x => x.Email == email) == 0)
                    {
                        return base.Json(new SimpleResponse { Success = false, Message = "Please enter an e-mail address registered in the system." });
                    }
                }

                if (this.Request != null)
                {
                    //Create encrypted activation key for email approval
                    string enc = Encryption.EncryptString(email + "|" + DateTime.Now.ToString());

                    var baseUrl = $"{this.Request.Scheme}://{this.Request.Host.Value.ToString()}{this.Request.PathBase.Value.ToString()}";

                    //Set email title and content
                    string emailTitle = "RFP Portal Password Renewal";
                    string emailContent = "We got your password reset request. Please use the link below to set a new password for  your account. <br><br> <a href='" + baseUrl + "/ResetPasswordView?str=" + enc + "'>Click here to reset your password.</a>";

                    //Send email
                    EmailHelper.SendEmail(emailTitle, emailContent, new List<string>() { email }, new List<string>(), new List<string>());

                }

                return base.Json(new SimpleResponse { Success = true, Message = "Password reset link sent to your email." });

            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
                return base.Json(new SimpleResponse { Success = false, Message = "An unexpected error has occurred. Please try again later" });
            }
        }

        /// <summary>
        /// Opens password reset model from 
        /// </summary>
        /// <param name="str">Encrypted user information in the password reset email</param>
        /// <returns></returns>
        [Route("ResetPasswordView")]

        public ActionResult ResetPasswordView(string str)
        {
            try
            {
                //Set password change token into session
                HttpContext.Session.SetString("passwordchangetoken", str);

                //Decrypt information
                string decryptedToken = Utility.Encryption.DecryptString(str);

                //Check if format is valid
                if (decryptedToken.Split('|').Length > 1)
                {
                    //Check if password renewal expired
                    DateTime emaildate = Convert.ToDateTime(decryptedToken.Split('|')[1]);
                    if (emaildate.AddMinutes(60) < DateTime.Now)
                    {
                        TempData["message"] = "Password reset request expired. Please submit a new request.";
                    }
                    else
                    {
                        //Set user's email
                        HttpContext.Session.SetString("passwordchangeemail", decryptedToken.Split('|')[0]);
                        TempData["action"] = "resetpassword";
                    }
                }
                else
                {
                    TempData["message"] = "Invalid password reset request.";
                }
            }
            catch (Exception ex)
            {
                TempData["message"] = "An error occurred during the process. Please try again later. ";

                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Sets user new password
        /// </summary>
        /// <param name="newpass">New password</param>
        /// <param name="newpassagain">New password confirmation</param>
        /// <param name="usercode">Captcha code (Needed after 3 failed requests)</param>
        /// <returns></returns>
        [Route("ResetPasswordComplete")]
        [HttpPost]
        public JsonResult ResetPasswordComplete(string newpass, string newpassagain)
        {
            try
            {
                //Password match control
                if (newpass != newpassagain)
                {
                    return base.Json(new SimpleResponse { Success = false, Message = "Passwords entered are not compatible." });
                }

                //Password strength control
                if (!Regex.IsMatch(newpass, @"^(?=.{8,})(?=.*[a-z])(?=.*[A-Z])"))
                {
                    return base.Json(new SimpleResponse { Success = false, Message = "The password must contain at least 8 characters and contain 1 digit 1 lowercase 1 uppercase." });
                }


                SimpleResponse resetResponse = AuthMethods.ResetPasswordComplete(newpass, HttpContext.Session.GetString("passwordchangetoken"));

                if (resetResponse.Success)
                {
                    return base.Json(new SimpleResponse { Success = true, Message = "Your password has been updated. You can sign in with your new password." });
                }
                else
                {
                    if (resetResponse.Message == "Renew expired")
                    {
                        HttpContext.Session.SetString("passwordchangeemail", "true");

                        return base.Json(new SimpleResponse { Success = false, Message = "This password renewal request has expired." });
                    }
                    else
                    {
                        return base.Json(new SimpleResponse { Success = false, Message = "An unexpected error has occurred. Please try again later" });
                    }
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError, true);
                return base.Json(new SimpleResponse { Success = false, Message = "An error occurred while proccesing your request." });
            }
        }


        [Route("CheckDevxDaoUser")]
        public IActionResult CheckDevxDaoUser(string email)
        {
            DxDUserModel model = AuthMethods.CheckDxDUser(email);

            return Json(model);
        }
        #endregion

    }
}
