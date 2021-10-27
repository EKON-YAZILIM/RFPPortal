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


                if (context.HttpContext.Session.GetInt32("UserID") == null)
                {
                    control = false;
                }

                if (!control)
                {
                    context.Result = new RedirectResult("../Public/Login");
                }
            }
            catch
            {
                context.Result = new RedirectResult("../Public/Login");
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


                if (context.HttpContext.Session.GetInt32("UserID") == null)
                {
                    control = false;
                }

                if (!control)
                {
                    context.Result = new RedirectResult("../Public/Login");
                }
            }
            catch
            {
                context.Result = new RedirectResult("../Public/Login");
            }

        }
    }

    /// <summary>
    ///  Authorization attribute for admin.
    ///  Only accepts requests from ip addresses in whitelist defined in appsettings.json
    /// </summary>
    public class AdminAuthorization : ActionFilterAttribute
    {
        public AdminAuthorization()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                bool control = false;

                string clientIp = Utility.IpHelper.GetClientIpAddress(context.HttpContext);

                if (Program._settings.IpWhitelist.Contains("*") || Program._settings.IpWhitelist.Contains(clientIp))
                {
                    control = true;
                }

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

                if (clientIp == "127.0.0.1" || clientIp == "localhost" || clientIp == "::1")
                {
                    control = true;
                }

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
}
