using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Shared
{
    public static class Routes
    {
        // Routes for Account/Register
        public const string AccountRegisterControllerRoute = "api/Account/Register";
        // Account/Register controller method routes
        public const string SigninRoute = "signin";
        public const string SigupRoute = "signup";
        public const string ConfirmEmailRoute = "confirmemail";
        public const string ResendConfirmEmailRoute = "resendconfirmemail";
        public const string ResetPasswordRoute = "resetpassword";
        public const string ForgotPasswordRoute = "forgotpassword";
        
        public const string SignoutRoute = "signout";

        // Routes for Account/Manage
        public const string AccountManageControllerRoute = "api/AccountManage";
        // Method routes
        public const string ProfileUpdateRoute = "updateprofile";
        public const string ProfileViewRoute = "viewprofile";
        public const string ChangeUserNameRoute = "changeusername";
        public const string SetProfilePicRoute = "setprofilepic";
        public const string ChangeEmailRoute = "changeemail";
        public const string ChangePasswordRoute = "changepassword";
        public const string RemoveAccountRoute = "removeaccount";

        /// Doctor section routes
        // DrAccountantController
        public const string DrAccountantControllerRoute = "api/dr/accountant";
        // method routes
        public const string ViewAllPaymentsRoute = "viewallpayments";
        public const string GetPaymentRoute = "getpayment";
        public const string ToggleConfirmPaymentRoute = "toggleconfirmpayment";
        public const string DrViewPaymentRoute = "DrViewPayment";

        // DrAssistant Routes
        public const string DrAssistantControllerRoute = "api/dr/assistant";
        // method routes
        public const string DrSearchAppointmentsRoute = "searchappointments";
        public const string GetTodayAppointmentsRoute = "gettodayappointments";
        public const string GetAppointmentDetailRoute = "getAppointmentDetail";
        public const string RejectAppointmentRoute = "RejectAppointment";
        public const string UnRejectAppointmentRoute = "unrejectappointment";
        public const string GetAppointmentsCountRoute = "GetAppointmentsCount";
        // clinic detail management
        public const string AddDrPracTimeRoute = "AddDrPracTime";
        public const string UpdateDrPracTimeRoute = "UpdateDrPracTime";
        public const string DeleteDrPracTimeRoute = "DeleteDrPracTime";
        public const string GetDrPracTimesRoute = "GetDrPracTimesRoute";
        public const string AddDrHolidayRoute = "AddDrHoliday";
        public const string UpdateDrHolidayRoute = "UpdateDrHoliday";
        public const string DeleteDrHolidayRoute = "DeleteDrHoliday";
        public const string GetDrHolidaysRoute = "GetDrHolidays";

        // DrLabStaffController routes
        public const string DrLabStaffControllerRoute = "api/dr/labstaff";
        // method routes
        public const string GetReferredLabTestsRoute = "getreferredlabtests";
        public const string GetReferredLabTestRoute = "getreferredlabtest";
        public const string SubmitLabTestResultsRoute = "submitlabtestresults";
        public const string ViewLabTestsRoute = "ViewLabTestsRoute";
        public const string ViewLabTestRoute = "ViewLabTestRoute";

        // DrPharmacyController routes
        public const string DrPharmacyStaffControllerRoute = "api/dr/pharmacy";
        // method routes
        public const string GetAllDrugsRoute = "getalldrugs";
        public const string SearchDrugRoute = "searchdrug";
        public const string AddDrugRoute = "adddrug";
        public const string UpdateDrugRoute = "updatedrug";
        public const string DeleteDrugRoute = "deletedrug";
        public const string ViewPatDrugsRoute = "viewpatdrugs";
        public const string ViewPrescriptionRoute = "ViewPrescription";
        public const string ViewAllPrescriptionsRoute = "ViewAllPrescriptions";

        // DrXrayStaffController routes
        public const string DrXrayStaffControllerRoute = "api/dr/xraystaff";
        // method routes
        public const string SubmitXrayResultRoute = "submitxrayresult";
        public const string GetReferredXraysRoute = "getreferredxrays";
        public const string GetReferredXrayRoute = "getreferredxray";
        public const string ViewPatXraysRoute = "ViewPatXraysRoute";
        public const string ViewPatXrayRoute = "ViewPatXrayRoute";

        // DrCheckPatController routes
        public const string DrCheckPatControllerRoute = "api/dr/checkpat";
        // method routes
        public const string InitPatReportRoute = "initpatreport";
        public const string GeneratePatReportRoute = "generatepatreport";
        public const string ReOpenReportRoute = "reopenreport";
        public const string ReferLabTestRoute = "referlabtests";
        public const string RemoveLabTestRoute = "removelabtest";
        public const string ReferXrayRoute = "referxray";
        public const string RemoveXrayRoute = "removexray";
        public const string AddPatDrugRoute = "addpatdrug";
        public const string UpdatePatDrugRoute = "updatepatdrug";
        public const string RemovePatDrugRoute = "removepatdrug";
        public const string CancelReportRoute = "cancelreport";
        public const string GetPatMedHistoryRoute = "getpatmedhistory";
        public const string ViewPatReportRoute = "viewpatreport";
        public const string ChangeAppointStatusRoute = "ChangeAppointStatus";

        // DrReviewController routes
        public const string DrRevewsControllerRoute = "api/dr/reviews";
        // method routes
        public const string DrViewReviewsRoute = "drviewreviews";
        public const string ToggleReportReviewRoute = "togglereportreview";

        // DrDetailsController routes
        public const string DrDetailsControllerRoute = "api/dr/details";
        // method routes
        public const string AddOrUpdateDrDetailRoute = "AddOrUpdateDrDetail";
        public const string ViewDrDetailRoute = "ViewDrDetail";
        public const string ToggleDrVisibilityRoute = "ToggleDrVisibility";

        // dr staff manage routes
        public const string DrStaffManageControllerRoute = "api/dr/managestaff";
        // routes
        public const string AddDrStaffRoute = "AddDrStaff";
        public const string ViewAllStaffDetailsRoute = "ViewAllStaffDetails";
        public const string UpdateStaffDataRoute = "UpdateStaffData";
        public const string RemoveStaffRoute = "RemoveStaff";
        public const string SetStaffProfilePicRoute = "setStaffProfilePicRoute";

        // dr progress routes
        public const string DrProgressControllerRoute = "api/dr/drprogress";
        //method routes
        public const string ViewMonthlyProgressRoute = "ViewMonthlyProgress";
        public const string ViewYearlyProgressRoute = "ViewYearlyProgress";
        public const string ViewVisitsRoute = "viewvisits";

        // DrFeedbackController routes
        public const string DrFeedbackControllerRoute = "api/dr/feedbackcontroller";
        // method routes
        public const string GetMessagesRoute = "GetMessages";
        public const string GetMessageRoute = "GetMessage";
        public const string ToggleReadMessageRoute = "ToggleReadMessage";
        public const string RemoveMsgFromYourChatRoute = "RemoveMsgFromYourChat";
        public const string DoReplyRoute = "DoReply";
        public const string RemoveReplyRoute = "RemoveReply";

        /// <summary>
        /// Patient section
        /// </summary>
        // PatAppointmentController routes
        public const string PatAppointmentControllerRoute = "api/pat/appointment";
        // method routes
        public const string CreateAppointmentRoute = "create";
        public const string PatViewAllAppointmentsRoute = "ViewAllAppointments";
        public const string ViewAllPastAppointmentsAsyncRoute = "ViewAllPastAppointments";
        public const string ViewAppointmentDetailAsyncRoute = "ViewAppointmentDetail";
        public const string CancelAppointmentRoute = "CancelAppointment";
        public const string TrackAppointmentRoute = "TrackAppointment";

        // PatCommentController routes
        public const string PatReviewControllerRoute = "api/pat/review";
        // method routes
        public const string AddOrUpdatePatReviewRoute = "AddOrUpdatePatReview";
        public const string RemovePatReviewRoute = "PatRemoveReview";
        public const string PatGetAllReviewsRoute = "patgetallreviews";

        // PatFeedbackController routes
        public const string PatFeedbackControllerRoute = "api/pat/feedback";
        // method routes
        public const string PatGetConversationRoute = "PatGetConversation";
        public const string SendMessageRoute = "SendMessage";
        public const string RemoveMessageRoute = "RemoveMessage";
        public const string ToggleReadReplyRoute = "ToggleReadReply";

        // PatReportController routes
        public const string PatReportControllerRoute = "api/pat/report";
        // method routes
        public const string PatViewReportRoute = "patviewreport";
        public const string PatToggleReportVisibilityRoute = "patToggleReportVisiblility";
        public const string GetAllVisitedDrsRoute = "getallvisiteddrs";
        public const string PatViewPaymentRoute = "patViewPayment";

        // SearchDrController routes
        public const string SearchDrControllerRoute = "api/searchdr";
        // method routes
        public const string SearchDrByNameRoute = "byname";
        public const string SearchDrBySpecialtyRoute = "byspecialty";
        public const string GetDrDetailsRoute = "details";
        public const string GetTopTenDrsRoute = "toptendrs";
    }
}
