using Microsoft.Data.SqlClient;
using System.Net.Mail;
using System.Security.Policy;

namespace ShopSuppliesV3.Helpers
{
    public class EmailSendingHelpers
    {
        public static void SendEmail(string recipientEmail, string emailBody, string emailSubject)
        {
            MailMessage message = new();

            //Clear the message.To  and message.CC. to remove previous values
            message.To.Clear();
            message.CC.Clear();
            message.Bcc.Clear();

            //Add the approver email to the message.To
            SmtpClient smtp = new SmtpClient("smtp.beav.com");
            message.To.Add(recipientEmail); //GETEmailAdd must be executed First
            //message.CC.Add(storeEmail);//ADD STORE IN THE CC FOR REQUESTS
            message.From = new MailAddress("SSV3_bsda-no-reply@collins.com");
            message.Bcc.Add("MARICEL.MAGTIBAY@collins.com"); //Stores BCC
            message.Bcc.Add("christian.garcia2@collins.com"); //Stores BCC
            message.Subject = emailSubject;
            message.IsBodyHtml = true; //to make message body as html
            message.Body = emailBody;
            smtp.Send(message); //uncomment when deploy
        }

        //FOR SENDING THE REQUEST TO APPROVER
        public static string GetEmaiBody(string orderNumbers, string approverUsername, string requestorName, string htmlTable, int isAdvance, string forApprovalId) //isAdvance should only be 0 or 1
        {
            string emailBody = EmailTemplate();
            string dateRequested = DateTime.Now.AddDays(1).ToString("MMM-dd-yyyy");
            string advanceDate = DateTime.Now.AddDays(2).ToString("MMM-dd-yyyy");

            //CHANGE THIS BASED ON THE API TO BE CREATED
            //string approvedHref = "http://localhost:5066/Approvals/UpdateRequest?ON=" + orderNumbers + "&CU=" + approverUsername + "&YN=" + 1 + "&RD=" + dateRequested + "&AO=" + isAdvance + ""; //CHANGE THIS BASED ON API LINK (AO advanceOrder and will depend on isAdvance paramete)
            //string declinedHref = "http://localhost:5066/Approvals/UpdateRequest?ON=" + orderNumbers + "&CU=" + approverUsername + "&YN=" + 2 + "&RD=" + dateRequested + "&AO=" + isAdvance + ""; //CHANGE THIS BASED ON API LINK (AO advanceOrder and will depend on isAdvance paramete)
            string approvedHref = "http://ta5-12ls0d3/SSV3//Approvals/UpdateRequest?ON=" + orderNumbers + "&CU=" + approverUsername + "&YN=" + 1 + "&RD=" + dateRequested + "&AO=" + isAdvance + "&AID=" + forApprovalId + ""; //CHANGE THIS BASED ON API LINK (AO advanceOrder and will depend on isAdvance paramete)
            string declinedHref = "http://ta5-12ls0d3/SSV3//Approvals/UpdateRequest?ON=" + orderNumbers + "&CU=" + approverUsername + "&YN=" + 2 + "&RD=" + dateRequested + "&AO=" + isAdvance + "&AID=" + forApprovalId + ""; //CHANGE THIS BASED ON API LINK (AO advanceOrder and will depend on isAdvance paramete)
            string approveLink = "<a href=\"" + approvedHref + "\" style = \"color:green\" >APPROVE LINK</a>";
            string declinedLink = "<a href=\"" + declinedHref + "\" style = \"color:red\" >REJECT LINK</a>";

            //for the approval/declined link
            string message = "Please refer to this " + approveLink + " to approve or use this " + declinedLink + " to decline the request.";
            string startGreeting = "";

            if (isAdvance == 0)
            {
                startGreeting = "This request needs your approval to proceed. " +
                    "Kindly review the details below before proceeding. " +
                    $"Plase do note that the cut-off period for this order is, {dateRequested}, at 2:00 PM";
            }
            else if(isAdvance == 1)
            {
                startGreeting = "This request needs your approval to proceed. " +
                    "Kindly review the details below before proceeding. " +
                    $"Plase do note that this is an advance order and the cut-off period for this is on {advanceDate} at 2:00 PM";
            }


            //Change the title, department, and businessunit in the emailbody
            emailBody = emailBody.Replace("[TITLE]", "SHOP SUPPLIES V3 REQUEST FORM");
            emailBody = emailBody.Replace("[DEPARTMENT]", $"Requestor: {requestorName}");
            //emailBody = emailBody.Replace("[BUSINESSUNIT]", $"Date Requested: {dateRequested}");
            emailBody = emailBody.Replace("[BUSINESSUNIT]", $"Request Id: {forApprovalId}");
            emailBody = emailBody.Replace("[STARTGREETING]", startGreeting);
            emailBody = emailBody.Replace("[CONTENT]", htmlTable);
            emailBody = emailBody.Replace("[ENDGREETING]", message);
            emailBody = emailBody.Replace("[FOOTER]", "This is an automatically generated email. Please do not respond directly to this email.");

            return emailBody;
        }

        //FOR SENDING THE APPROVAL RESULT TO REQUESTOR
        public static string GetEmaiBody(int YN, string dateRequested , string approverName, string requestorName, string htmlTable, string forApprovalId)
        {
            string emailBody = EmailTemplate();
            string dateApproved = DateTime.Now.ToString("MMM-dd-yyyy");

            //FOR THE STATUS (YN) IF APPROVED OR NOT
            string message = "";
            string endGreeting = "";

            if(YN == 1)
            {
                message = $"Your request have been APPROVED by {approverName} on {dateApproved}";
                endGreeting = "You can now print this email and present to the store to claim your requests";
            }
            else
            {
                message = $"Your request have been DECLINED by {approverName} on {dateApproved}";
                endGreeting = $"For any concerns regarding approval of this request, please contact {approverName}.";
            }

            //Change the title, department, and businessunit in the emailbody
            emailBody = emailBody.Replace("[TITLE]", "SHOP SUPPLIES V3 REQUEST FORM");
            emailBody = emailBody.Replace("[DEPARTMENT]", $"Requestor: {requestorName}");
            //emailBody = emailBody.Replace("[BUSINESSUNIT]", $"Date Requested: {dateRequested}");
            emailBody = emailBody.Replace("[BUSINESSUNIT]", $"Request Id: {forApprovalId}");
            emailBody = emailBody.Replace("[STARTGREETING]", message);
            emailBody = emailBody.Replace("[CONTENT]", htmlTable);
            emailBody = emailBody.Replace("[ENDGREETING]", endGreeting);
            emailBody = emailBody.Replace("[FOOTER]", "This is an automatically generated email. Please do not respond directly to this email.");

            return emailBody;
        }

        //FOR SENDING THE REQUEST TO THE BUYER(PULL-OUT ITEMS)
        public static string GetEmaiBodyPullout(int requestId, string buyerUsername ,string requestorName, string itemNumber, string pulloutReason, decimal pulloutQty, int incomingId) //isAdvance should only be 0 or 1
        {
            string emailBody = EmailTemplate();
            string dateRequested = DateTime.Now.AddDays(1).ToString("MMM-dd-yyyy");

            //CHANGE THIS BASED ON THE API TO BE CREATED
            //string approvedHref = "http://localhost:5066/Approvals/UpdateRequest?ON=" + orderNumbers + "&CU=" + approverUsername + "&YN=" + 1 + "&RD=" + dateRequested + "&AO=" + isAdvance + ""; //CHANGE THIS BASED ON API LINK (AO advanceOrder and will depend on isAdvance paramete)
            //string declinedHref = "http://localhost:5066/Approvals/UpdateRequest?ON=" + orderNumbers + "&CU=" + approverUsername + "&YN=" + 2 + "&RD=" + dateRequested + "&AO=" + isAdvance + ""; //CHANGE THIS BASED ON API LINK (AO advanceOrder and will depend on isAdvance paramete)
            string approvedHref = "http://ta5-12ls0d3/SSV3//Approvals/PulloutApproval?IN=" + requestId + "&CU=" + buyerUsername + "&IN" + incomingId + "&YN=" + 1 +  ""; 
            string declinedHref = "http://ta5-12ls0d3/SSV3//Approvals/PulloutApproval?IN=" + requestId + "&CU=" + buyerUsername + "&IN" + incomingId + "&YN=" + 2 + "";  
            string approveLink = "<a href=\"" + approvedHref + "\" style = \"color:green\" >APPROVE LINK</a>";
            string declinedLink = "<a href=\"" + declinedHref + "\" style = \"color:red\" >REJECT LINK</a>";

            //for the approval/declined link
            string message = "Please refer to this " + approveLink + " to approve or use this " + declinedLink + " to decline the request.";
            string startGreeting = "Good day, please refer below for the item pull-out request details";
            string content = $"Item Number: {itemNumber} | Pullout Quantity: {pulloutQty} | Reason: {pulloutReason}";



            //Change the title, department, and businessunit in the emailbody
            emailBody = emailBody.Replace("[TITLE]", "SHOP SUPPLIES V3 PULL-OUT REQUEST FORM");
            emailBody = emailBody.Replace("[DEPARTMENT]", $"Item Number: {itemNumber}");
            //emailBody = emailBody.Replace("[BUSINESSUNIT]", $"Date Requested: {dateRequested}");
            emailBody = emailBody.Replace("[BUSINESSUNIT]", $"Requestor: {requestorName}");
            emailBody = emailBody.Replace("[STARTGREETING]", startGreeting);
            emailBody = emailBody.Replace("[CONTENT]", content);
            emailBody = emailBody.Replace("[ENDGREETING]", message);
            emailBody = emailBody.Replace("[FOOTER]", "This is an automatically generated email. Please do not respond directly to this email.");

            return emailBody;
        }

        //FOR SENDING THE APPROVAL RESULT TO THE REQUESTOR
        public static string GetEmaiBodyPullout(int YN, string approverName, string itemNumber, string pulloutQty, string pulloutReason ) //isAdvance should only be 0 or 1
        {
            string emailBody = EmailTemplate();
            string dateApproved = DateTime.Now.ToString("MMM-dd-yyyy");

            //FOR THE STATUS (YN) IF APPROVED OR NOT
            string message = "";
            string endGreeting = "";

            if (YN == 1)
            {
                message = $"Your pull-out request for item {itemNumber} have been APPROVED by {approverName} on {dateApproved}";
                endGreeting = $"Please double check on the system if the item is successfully subtracted from the inventory.";
            }
            else
            {
                message = $"Your pull-out request for item {itemNumber} have been DECLINED by {approverName} on {dateApproved}";
                endGreeting = $"For any concerns regarding approval of this request, please contact {approverName}.";
            }

            string startGreeting = "Good day, please refer below for the item pull-out request details";
            string content = $"Item Number: {itemNumber} | Pullout Quantity: {pulloutQty} | Reason: {pulloutReason}";



            //Change the title, department, and businessunit in the emailbody
            emailBody = emailBody.Replace("[TITLE]", "SHOP SUPPLIES V3 REQUEST FORM");
            emailBody = emailBody.Replace("[DEPARTMENT]", $"Item Number: {itemNumber}");
            //emailBody = emailBody.Replace("[BUSINESSUNIT]", $"Date Requested: {dateRequested}");
            emailBody = emailBody.Replace("[BUSINESSUNIT]", $"Approver: {approverName}");
            emailBody = emailBody.Replace("[STARTGREETING]", message);
            emailBody = emailBody.Replace("[CONTENT]", content);
            emailBody = emailBody.Replace("[ENDGREETING]", endGreeting);
            emailBody = emailBody.Replace("[FOOTER]", "This is an automatically generated email. Please do not respond directly to this email.");

            return emailBody;
        }


        //FOR GETTING THE EMAIL TEMPLATE
        public static string EmailTemplate()
        {
            string EmailBody = "";
            string EmailConnectionString = "Data Source=TA5-GNRL6Q2\\ALCI;Database=alcidb;User Id=alciuser;Password=_*bs06";

            //Getting of email Template and updating the variable emailbody content
            using (SqlConnection con = new SqlConnection(EmailConnectionString))
            {
                string select_email_template = "SELECT * FROM [ALCIDB].[dbo].[EmailTemplate] WHERE TemplateName = 'Default'";
                con.Open();

                //Selecting the email template
                using (SqlCommand cmd = new SqlCommand(select_email_template, con))
                {
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            EmailBody = rdr["TemplateContent"].ToString();
                        }
                    }
                }
                con.Close();
            }

            return EmailBody;
        }

    }
}
