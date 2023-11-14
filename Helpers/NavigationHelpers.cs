using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ShopSuppliesV3.Data;
using ShopSuppliesV3.ViewModel;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;

namespace ShopSuppliesV3.Helpers
{
    public static class NavigationHelpers
    {

        public static string MakeActiveClass(this IUrlHelper urlHelper, string controller)
        {
            try
            {
                string result = "active";
                string controllerName = urlHelper.ActionContext.RouteData.Values["controller"].ToString();
                //string methodName = urlHelper.ActionContext.RouteData.Values["action"].ToString();
                if (string.IsNullOrEmpty(controllerName)) return null;
                if (controllerName.Equals(controller, StringComparison.OrdinalIgnoreCase))
                {
                    return result;
                    //if (methodName.Equals(action, StringComparison.OrdinalIgnoreCase))
                    //{
                    //    return result;
                    //}
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string MakeActiveClass(this IUrlHelper urlHelper, string controller, string method)
        {
            try
            {
                string result = "active";
                string controllerName = urlHelper.ActionContext.RouteData.Values["controller"].ToString();
                string methodName = urlHelper.ActionContext.RouteData.Values["action"].ToString();
                if (string.IsNullOrEmpty(controllerName)) return null;
                if (controllerName.Equals(controller, StringComparison.OrdinalIgnoreCase))
                {
                    if (methodName.Equals(method, StringComparison.OrdinalIgnoreCase))
                    {
                        return result;
                    }
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string ExpDateWriter(DateTime? ExpirationDate)
        {
            try
            {
                if (ExpirationDate == null)
                {
                    string result = "N/A";
                    return result;
                }
                else
                {
                    return DateTime.Parse(ExpirationDate.ToString()).ToString("MMM-dd-yyyy");
                }
            }
            catch
            {
                return DateTime.Parse(ExpirationDate.ToString()).ToString("MMM-dd-yyyy");
            }
        }

        //public static string SpanColorBasedOnExpDate(DateTime? ExpirationDate)
        //{
        //    try
        //    {
        //        if (ExpirationDate == null)
        //        {       
        //            return "text-muted";
        //        }
        //        else
        //        {
        //            return "text-danger";
        //        }
        //    }
        //    catch
        //    {
        //        return "text-danger";
        //    }
        //}

        public static string ExpDateChecker(DateTime? ExpirationDate)
        {
            try
            {
                if (ExpirationDate != null)
                {
                    //if(ExpirationDate == DateTime.Now )
                    //{
                    //    string result = "bg-danger";
                    //    return result;
                    //}

                    //FOR CONTINUATION
                    if (DateTime.Now.AddDays(+30) > ExpirationDate)
                    {
                        string result = "bg-danger";
                        return result;
                    }
                    else if (DateTime.Now.AddDays(+60) > ExpirationDate)
                    {
                        string result = "bg-warning";
                        return result;
                    }
                    else
                    {
                        string result = "bg-success";
                        return result;
                    }

                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        //Remove the sidebar when in the Store Url (Strore Controller)
        public static string MakeSidebarCollapse(this IUrlHelper urlHelper, string controller)
        {
            try
            {
                string result = "layout-top-nav";
                string controllerName = urlHelper.ActionContext.RouteData.Values["controller"].ToString();
                //string methodName = urlHelper.ActionContext.RouteData.Values["action"].ToString();
                if (string.IsNullOrEmpty(controllerName)) return null;
                if (controllerName.Equals(controller, StringComparison.OrdinalIgnoreCase))
                {
                    return result;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

    }

    public static class StoreIndicatorHelpers
    {
        public static string ColorRowBasedOnStocks(decimal remainingStocksToRequest, decimal safetyStocks)
        {
            try
            {
                if (remainingStocksToRequest < safetyStocks && remainingStocksToRequest != 0)
                {
                    return "bg-danger";
                }
                else if (remainingStocksToRequest == safetyStocks)
                {
                    return "bg-warning";
                }
                else if (remainingStocksToRequest > safetyStocks)
                {
                    return "bg-success";
                }
                else if (remainingStocksToRequest == 0)
                {
                    return "bg-secondary";
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }


    public static class RequestIndicatorHelpers
    {
        public static string DisableBtnBasedOnApproval(string approvalStatus)
        {
            try
            {
                if (approvalStatus.ToLower() == "approved")
                {
                    return "btn-primary";
                }
                else
                {
                    return "btn-secondary disabled";
                }
            }
            catch
            {
                return "btn-secondary disabled";
            }
        }
    }

    public static class TrackingHelpers
    {
        public static string ActivateTracker(string modelValue)
        {
            try
            {
                if (!string.IsNullOrEmpty(modelValue))
                {
                    return "active";
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }

        }
    }

    public static class MainNavNotif
    {
        //GET THE COUNT OF REQUEST FOR APPROVAL BY THE LOGIN APPROVER
        public static string ApprovalsCount(string currentUser)
        {
            try
            {
                string count = null;

                string conString = "Server=TAWBSDAD001P;Database=scmdb;User Id=ruTAWBSprod;Password=VeV1$A*!cROhEv";

                //Getting of Count of request for approvals
                using (SqlConnection con = new SqlConnection(conString))
                {
                    string query = $"SELECT COUNT([Id]) AS ForApprovalCount FROM [scmdb].[dbo].[SSv3_GAL_APPROVALSTBL] WHERE ApproverUserName = '{currentUser}' AND ForApprovalStatus = 'For Approval'";
                    con.Open();

                    //Selecting the email template
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                count = rdr["ForApprovalCount"].ToString();
                            }
                        }
                    }
                    con.Close();
                }

                return count;
            }
            catch
            {
                return null;
            }
        }

        public static string ApprovalNotification(string currentUser)
        {
            try
            {
                string newApproval = ApprovalsCount(currentUser);
                if(newApproval != "0")
                {
                    return "new";
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        //GET THE CURRENT STATUS OF THE LATEST USER'S REQUEST
        public static string RequestStatus(string currentUser)
        {
            try
            {
                string latestRequestStatus = null;

                string conString = "Server=TAWBSDAD001P;Database=scmdb;User Id=ruTAWBSprod;Password=VeV1$A*!cROhEv";

                //Getting of Count of request for approvals
                using (SqlConnection con = new SqlConnection(conString))
                {
                    string query = $"SELECT TOP(1) [RequestStatus] FROM [scmdb].[dbo].[SSv3_ITEM_REQUESTTBL] INNER JOIN [scmdb].[dbo].[SSv3_GAL_USERTBL] ON [scmdb].[dbo].[SSv3_ITEM_REQUESTTBL].RequestorUserId = [scmdb].[dbo].[SSv3_GAL_USERTBL].Id WHERE [scmdb].[dbo].[SSv3_GAL_USERTBL].UserName = '{currentUser}' ORDER BY [scmdb].[dbo].[SSv3_ITEM_REQUESTTBL].DateRequested DESC";
                    con.Open();

                    //Selecting the email template
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                latestRequestStatus = rdr["RequestStatus"].ToString();
                            }
                        }
                    }
                    con.Close();
                }

                if (latestRequestStatus == null)
                {
                    return null;
                }

                if (latestRequestStatus.ToLower() == "new")
                {
                    return null;
                }

                return latestRequestStatus;
            }
            catch
            {
                return null;
            }
        }

        //GET THE COUNT OF ITEMS IN THE CART
        public static string CartItemCount(string currentUser)
        {
            try
            {
                string itemCount = "0";

                string conString = "Server=TAWBSDAD001P;Database=scmdb;User Id=ruTAWBSprod;Password=VeV1$A*!cROhEv";

                //Getting of Count of request for approvals
                using (SqlConnection con = new SqlConnection(conString))
                {
                    string query = $"\r\n  SELECT COUNT([UserName]) AS [ItemCount] FROM [scmdb].[dbo].[SSv3_GAL_USERTBL] AS [User] INNER JOIN [scmdb].[dbo].[SSv3_ITEM_REQUESTTBL] AS [Requests] ON [Requests].RequestorUserId = [User].Id WHERE [Requests].RequestStatus = 'New' AND [Requests].isActive = 1 AND [User].UserName = '{currentUser}'";
                    con.Open();

                    //Selecting the email template
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                itemCount = rdr["ItemCount"].ToString();
                            }
                        }
                    }
                    con.Close();
                }

                return itemCount;
            }
            catch
            {
                return "0";
            }
        }

        //GIVES NOTIFICATION COUNT IN THE VIEW
        public static string RequesStatusNotif(string currentUser)
        {
            try
            {
                string status = RequestStatus(currentUser);
                if (!string.IsNullOrEmpty(status))
                {
                    return "1";
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        //GET THE COUNT OF ORDERS FROM APPROVALS TABLE THAT WAS APPROVE FOR ISSUANCE
        public static string IssuanceCount()
        {
            try
            {
                string issuanceCount = "";

                string conString = "Server=TAWBSDAD001P;Database=scmdb;User Id=ruTAWBSprod;Password=VeV1$A*!cROhEv";

                //Getting of Count of request for approvals
                using (SqlConnection con = new SqlConnection(conString))
                {
                    string query = $"SELECT COUNT(*) AS [IssuanceSum] FROM (SELECT COUNT([RequestorUserId]) AS [IssuanceCount], [RequestorUserId]  FROM [scmdb].[dbo].[SSv3_ITEM_REQUESTTBL] WHERE [RequestStatus] = 'Approved' AND [RequestCategory] != 'Pullout' GROUP BY [RequestorUserId]) AS [NewGroup]";
                    con.Open();

                    //Selecting the email template
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                issuanceCount = rdr["IssuanceSum"].ToString();
                            }
                        }
                    }
                    con.Close();
                }
                return issuanceCount;
            }
            catch
            {
                return null;
            }
        }
    }

    public class LayoutSelector
    {
        //TO  SEPARATE VIEWS FOR ADMIN AND USER
        public static string AccessType(string currentUser)
        {
            string accessType = null;
            string isApprover = null;

            try
            { string conString = "Server=TAWBSDAD001P;Database=scmdb;User Id=ruTAWBSprod;Password=VeV1$A*!cROhEv";

                //Getting of Count of request for approvals
                using (SqlConnection con = new SqlConnection(conString))
                {
                    string query = $"SELECT [AccessType], [isApprover] FROM [scmdb].[dbo].[SSv3_GAL_USERTBL] WHERE [UserName] = '{currentUser}'";
                    con.Open();

                    //Selecting the email template
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                accessType = rdr["AccessType"].ToString();
                                isApprover = rdr["isApprover"].ToString();
                            }
                        }
                    }
                    con.Close();
                }

                if (!string.IsNullOrEmpty(accessType)) //query has value
                {
                    if (accessType.ToLower() == "admin")
                    {
                        return "admin";
                    }
                    else if (isApprover.ToLower() == "true" || accessType.ToLower() == "buyer")
                    {
                        return "approver";
                    }
                }

                return "User"; //query is null or empty
            }
            catch
            {
                return "User"; //error
            }
        }
    }


    //FOR INCOMING STATUS HELPER
    public class IncomingHelpers
    {
        //COLOR CODING FOR PULL REQUEST IN HISTORY TABLE
        public static string PullRequestHelper(string status)
        {
            try
            {
                if (status.ToLower() == "for approval")
                {
                    return "bg-warning";
                }
                else if (status.ToLower() == "approved")
                {
                    return "bg-success";
                }
                else if (status.ToLower() == "declined")
                {
                    return "bg-danger";
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }

        }
    }



}
