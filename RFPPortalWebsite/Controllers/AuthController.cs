﻿using Helpers.Models.SharedModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RFPPortalWebsite.Contexts;
using RFPPortalWebsite.Methods;
using RFPPortalWebsite.Models.DbModels;
using RFPPortalWebsite.Models.ViewModels;
using RFPPortalWebsite.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static RFPPortalWebsite.Models.Constants.Enums;

namespace RFPPortalWebsite.Controllers
{
    /// <summary>
    ///  AuthController contains user authorization and registration methods.
    ///  Public users are automatically registered to the system after filling the bidding form. "RegisterPublic" method is triggered after posting bidding form.
    ///  Internal users should be registered from third party admin(DevxDao) with "RegisterInternal" method before they transferred to the portal.
    ///  If user already have a registration in the portal "GetUserAuthKey" method can be used to get user's AuthKey
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        /// <summary>
        ///  Public user registration method
        ///  This method can be accessed by every user
        /// </summary>
        /// <param name="registerInput">Registration information of the user</param>
        /// <returns>AjaxResponse object with registration result</returns>
        [HttpPost("RegisterUser", Name = "RegisterUser")]
        public AjaxResponse RegisterUser([FromBody] RegisterModel registerInput)
        {
            try
            {
                //Validations
                using (rfpdb_context db = new rfpdb_context())
                {
                    //Email already exists control
                    if (db.Users.Count(x => x.Email == registerInput.Email) > 0)
                    {
                        return new AjaxResponse() { Success = false, Message = "Email already exists." };
                    }

                    //Username already exists control
                    if (db.Users.Count(x => x.UserName == registerInput.UserName) > 0)
                    {
                        return new AjaxResponse() { Success = false, Message = "Username already exists." };
                    }
                }
                User usr = AuthMethods.UserRegister(registerInput);
                if (usr.UserId > 0)
                {
                    return new AjaxResponse() { Success = true, Message = "User registration succesful.", Content = new User{ Email = usr.Email  } };
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError);
            }

            return new AjaxResponse() { Success = false, Message = "Unexpected error" };
        }

        /// <summary>
        ///  Returns user information from user's email and password
        ///  This method can be accessed by every user
        /// </summary>
        /// <param name="email">User's email and password</param>
        /// <returns>AjaxResponse object with user object</returns>
        [HttpPost("GetUserInfo", Name = "GetUserInfo")]
        public AjaxResponse GetUserInfo(string email, string pass)
        {
            try
            {
                User user = Methods.AuthMethods.GetUserInfo(email, pass);
                if(user.UserId > 0)
                {
                    return new AjaxResponse() { Success = true, Message = "User found.", Content = new { User = user } };
                }
                else
                {
                    return new AjaxResponse() { Success = false, Message = "User not found." };
                }
            }
            catch (Exception ex)
            {
                Program.monitizer.AddException(ex, LogTypes.ApplicationError);
                return new AjaxResponse() { Success = false, Message = "Unexpected error" };
            }
        }


    }
}
