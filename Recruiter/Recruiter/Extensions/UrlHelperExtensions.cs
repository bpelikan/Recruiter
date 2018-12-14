using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Recruiter.Controllers;

namespace Microsoft.AspNetCore.Mvc
{
    public static class UrlHelperExtensions
    {
        public static string EmailConfirmationLink(this IUrlHelper urlHelper, string userId, string code, string scheme)
        {
            return urlHelper.Action(
                action: nameof(AccountController.ConfirmEmail),
                controller: "Account",
                values: new { userId, code },
                protocol: scheme);
        }

        public static string ResetPasswordCallbackLink(this IUrlHelper urlHelper, string userId, string code, string scheme)
        {
            return urlHelper.Action(
                action: nameof(AccountController.ResetPassword),
                controller: "Account",
                values: new { userId, code },
                protocol: scheme);
        }

        public static string MyApplicationDetailsCallbackLink(this IUrlHelper urlHelper, string applicationId, string scheme)
        {
            return urlHelper.Action(
                action: nameof(MyApplicationController.MyApplicationDetails),
                controller: "MyApplication",
                values: new { applicationId },
                protocol: scheme);
        }

        public static string ConfirmAppointmentCallbackLink(this IUrlHelper urlHelper, string interviewAppointmentId, string scheme, string stageId = null, string applicationId = null)
        {
            return urlHelper.Action(
                action: nameof(MyApplicationController.ConfirmInterviewAppointmentFromLink),
                controller: "MyApplication",
                values: new { interviewAppointmentId, stageId, applicationId },
                protocol: scheme);
        }
    }
}
