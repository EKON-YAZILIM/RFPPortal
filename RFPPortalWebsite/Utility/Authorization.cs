﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RFPPortalWebsite.Utility
{
    /// <summary>
    ///  Authorization attribute for public users
    /// </summary>
    public class PublicUserAuthorization : ActionFilterAttribute
    {
        public PublicUserAuthorization()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                bool control = true;

                //Check if user logged in
                if (context.HttpContext.Session.GetInt32("UserID") == null)
                {
                    control = false;
                }

                //Unauthorized request
                if (!control)
                {
                    context.Result = new RedirectResult("../Home/Unauthorized");
                }
            }
            catch
            {
                context.Result = new RedirectResult("../Home/Unauthorized");
            }

        }
    }

    /// <summary>
    ///  Authorization attribute for internal users (VAs)
    /// </summary>
    public class InternalUserAuthorization : ActionFilterAttribute
    {
        public InternalUserAuthorization()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                bool control = true;

                //Check if user logged in
                if (context.HttpContext.Session.GetInt32("UserID") == null)
                {
                    control = false;
                }

                //Check if user type is internal
                if (context.HttpContext.Session.GetString("UserType") == null || context.HttpContext.Session.GetString("UserType") != Models.Constants.Enums.UserIdentityType.Internal.ToString())
                {
                    control = false;
                }

                //Unauthorized request
                if (!control)
                {
                    context.Result = new RedirectResult("../Home/Unauthorized");
                }
            }
            catch
            {
                context.Result = new RedirectResult("../Home/Unauthorized");
            }

        }
    }

    /// <summary>
    ///  Authorization attribute for third party admin.
    ///  Only accepts requests from ip addresses in whitelist defined in appsettings.json
    /// </summary>
    public class IpWhitelistAuthorization : ActionFilterAttribute
    {
        public IpWhitelistAuthorization()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                bool control = false;

                string clientIp = Utility.IpHelper.GetClientIpAddress(context.HttpContext);

                //Check if client request ip is in whitelist
                if (Program._settings.IpWhitelist.Contains("*") || Program._settings.IpWhitelist.Contains(clientIp))
                {
                    control = true;
                }

                //Unauthorized request
                if (!control)
                {
                    Program.monitizer.AddApplicationLog(Models.Constants.Enums.LogTypes.ApplicationLog, "Unauthorized IpWhitelist request. Client Ip: " + clientIp);
                    context.Result = new RedirectResult("../Home/Unauthorized");
                }
            }
            catch
            {
                context.Result = new RedirectResult("../Home/Unauthorized");
            }

        }
    }

    /// <summary>
    ///  Authorization attribute for local machine. 
    ///  Only accepts requests from "localhost", "127.0.0.1" and "::1"
    /// </summary>
    public class LocalMachineAuthorization : ActionFilterAttribute
    {
        public LocalMachineAuthorization()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                bool control = false;

                string clientIp = Utility.IpHelper.GetClientIpAddress(context.HttpContext);
                
                //Check if client request coming from local machine
                if (clientIp == "127.0.0.1" || clientIp == "localhost" || clientIp == "::1")
                {
                    control = true;
                }

                //Unauthorized request
                if (!control)
                {
                    Program.monitizer.AddApplicationLog(Models.Constants.Enums.LogTypes.ApplicationLog, "Unauthorized local machine request. Client Ip: " + clientIp);
                    context.Result = new RedirectResult("../Home/Unauthorized");
                }
            }
            catch
            {
                context.Result = new RedirectResult("../Home/Unauthorized");
            }

        }
    }
}
