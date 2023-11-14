using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSuppliesV3.Models;
using ShopSuppliesV3.Data;
using System.Diagnostics.Eventing.Reader;
using ShopSuppliesV3.ViewModel;
using ShopSuppliesV3.Helpers;
using System.Data;
using System.Globalization;
using Humanizer;
using System.Security.Cryptography;

namespace ShopSuppliesV3.Controllers
{
    public class ApprovalsController : Controller
    {
        private readonly ShopSuppliesV3Context _context;

        public ApprovalsController(ShopSuppliesV3Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {

            //APPROVER VIEW (APPROVER OR BUYER)
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

            //If USER IS NOT IN THE USERTABLE OR AN APPROVER ERROR
            if (userQuery == null)
            {
                return RedirectToAction("Error404", "Errors");
            }

            try
            {
                if(userQuery.isApprover == true) //THE CURRENT USER IS AN APPROVER
                {
                    var query = from approval in _context.SSv3_GAL_APPROVALSTBL
                                where approval.ApproverUserName == currentUser && approval.ForApprovalStatus.ToLower() == "for approval"
                                join user in _context.SSv3_GAL_USERTBL on approval.RequestorUsername equals user.UserName
                                where user.isActive == 1
                                orderby approval.Id descending
                                select new Approvals
                                {
                                    Id = approval.Id,
                                    OrderNumbers = approval.OrderNumbers,
                                    RequestorName = user.Name,
                                    RequestedDate = approval.RequestedDate,
                                    IsAdvanceOder = approval.IsAdvanceOder,
                                };
                    var queryList = await query.ToListAsync();

                    TempData["AccessType"] = "Approver";
                    return View(queryList);
                }
                else if(userQuery.AccessType.ToLower() == "buyer") //THE CURRENT USER IS A BUYER
                {
                    var query = from request in _context.SSv3_ITEM_REQUESTTBL 
                                where request.isActive == 1 && request.RequestCategory.ToLower() == "pullout" 
                                && request.RequestStatus.ToLower() == "for approval" 
                                && request.ApproverUserId == userQuery.Id
                                join requestor in _context.SSv3_GAL_USERTBL on request.RequestorUserId equals requestor.Id where requestor.isActive==1
                                join approver in _context.SSv3_GAL_USERTBL on request.ApproverUserId equals approver.Id where approver.isActive == 1
                                join history in _context.SSv3_GAL_INCOMING_HISTORYTBL on request.OrderNumber equals history.OrderNumber
                                join incoming in _context.SSv3_GAL_INCOMMINGTBL on history.IncomingId equals incoming.Id where incoming.isActive == 1
                                select new Approvals
                                {
                                    Id = request.Id,
                                    OrderNumber = request.OrderNumber,
                                    RequestorName = requestor.Name,
                                    RequestedDate = request.DateRequested,
                                    IncomingId = incoming.Id
                                    
                                };
                    var queryList = await query.ToListAsync();

                    TempData["AccessType"] = "Buyer";
                    return View(queryList);
                }
                else
                {
                    return RedirectToAction("Error404", "Errors");
                }

            }
            catch
            {
                return RedirectToAction("ApprovalsIndexError", "Errors");
            }
        }

        public async Task<IActionResult> ApprovalHistory()
        {
            //APPROVER VIEW (APPROVER OR BUYER)
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

            //If USER IS NOT IN THE USERTABLE OR AN APPROVER ERROR
            if (userQuery == null)
            {
                return RedirectToAction("Error404", "Errors");
            }

            try
            {
                if (userQuery.isApprover == true) //THE CURRENT USER IS AN APPROVER
                {
                    var query = from approval in _context.SSv3_GAL_APPROVALSTBL
                                where approval.ApproverUserName == currentUser && approval.ForApprovalStatus.ToLower() != "for approval"
                                join user in _context.SSv3_GAL_USERTBL on approval.RequestorUsername equals user.UserName
                                where user.isActive == 1
                                orderby approval.Id descending
                                select new Approvals
                                {
                                    Id = approval.Id,
                                    OrderNumbers = approval.OrderNumbers,
                                    RequestorName = user.Name,
                                    RequestedDate = approval.RequestedDate,
                                    IsAdvanceOder = approval.IsAdvanceOder,
                                };
                    var queryList = await query.ToListAsync();

                    TempData["AccessType"] = "Approver";
                    return View(queryList);
                }
                else if (userQuery.AccessType.ToLower() == "buyer") //THE CURRENT USER IS A BUYER
                {
                    var query = from request in _context.SSv3_ITEM_REQUESTTBL
                                where request.isActive == 1 && request.RequestCategory.ToLower() == "pullout"
                                && request.RequestStatus.ToLower() != "for approval"
                                && request.ApproverUserId == userQuery.Id
                                join requestor in _context.SSv3_GAL_USERTBL on request.RequestorUserId equals requestor.Id
                                where requestor.isActive == 1
                                join approver in _context.SSv3_GAL_USERTBL on request.ApproverUserId equals approver.Id
                                where approver.isActive == 1
                                join history in _context.SSv3_GAL_INCOMING_HISTORYTBL on request.OrderNumber equals history.OrderNumber
                                join incoming in _context.SSv3_GAL_INCOMMINGTBL on history.IncomingId equals incoming.Id
                                where incoming.isActive == 1
                                select new Approvals
                                {
                                    Id = request.Id,
                                    OrderNumber = request.OrderNumber,
                                    RequestorName = requestor.Name,
                                    RequestedDate = request.DateRequested,
                                    IncomingId = incoming.Id

                                };
                    var queryList = await query.ToListAsync();

                    TempData["AccessType"] = "Buyer";
                    return View(queryList);
                }
                else
                {
                    return RedirectToAction("Error404", "Errors");
                }

            }
            catch
            {
                return RedirectToAction("ApprovalsHistoryError", "Errors");
            }
        }


        //API FOR AUTOEMAIL ACCEPT/REJECT OF PRODUCTION REQUEST
        //ON - OrderNumbers
        //CU - CurrentUser (equivalent to the approver username)
        //YN - YES OR NO (Approved or Declined)
        //RD - requestedDate
        //AO - Advance Order (0 not advance and 1 if advance)
        //[Route("api/[controller]/{ON}/{CU}/{YN}/{RD}/{AO}")]
        //AID - Approval ID (ForApprovalId in the approvals table (formatted))
        //[HttpPost]
        public async Task<IActionResult> UpdateRequest(string ON, string CU, string YN, string RD, string AO, string AID)
        {
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefaultAsync();

            //If USER IS NOT IN THE USERTABLE ERROR
            if (userQuery == null)
            {
                return RedirectToAction("Error404", "Errors");
            }

            try
            {

                //VALIDATE THE API PARAMETERS
                if (isApprovedOrDeclined(ON)) { return RedirectToAction("StatusError", "Errors"); }
                if (!validateON(ON)) { return RedirectToAction("InvalidONError", "Errors"); }
                if (!await validateCU(CU, ON)) { return RedirectToAction("InvalidCUError", "Errors"); }
                if (!validateYN(YN)) { return RedirectToAction("InvalidYNError", "Errors"); }
                if(!validateRD(RD, AO)) { return RedirectToAction("ExpiredRequestError", "Errors"); }
                
                //IF API PARAMETERS ARE VALID UPDATE BASED ON YN
                string[] requests = ON.Split(',');
                int[] requestIds = Array.ConvertAll(requests, int.Parse);
                int yn = int.Parse(YN);
                await _context.Database.BeginTransactionAsync();
                DataTable dt = new();


                //Email Subject based on approved (YN=1) or declined (YN=2)
                string emailSubject = "";

                if (YN == "1") //APPROVE THE CURRENT REQUEST
                {
                    emailSubject = "Shop Supplies V3 Notification - Item Request - APPROVED";

                    foreach (int requestId in requestIds)
                    {
                        var updateRequest = await _context.SSv3_ITEM_REQUESTTBL.FindAsync(requestId);
                        updateRequest.RequestStatus = "Approved";
                        updateRequest.DateApproved = DateTime.Now;
                        await _context.SaveChangesAsync();
                    };

                    //UPDATE THE APPROVALS TABLE
                    int approvalId = int.Parse(AID.Substring(0, AID.IndexOf("-"))); //get the approval id from the formated ForApprovalId
                    var updateApproval = _context.SSv3_GAL_APPROVALSTBL.Find(approvalId);
                    updateApproval.ForApprovalStatus = "Approved";
                    await _context.SaveChangesAsync();


                    //QUERY THE UPDATED REQUEST FOR EMAIL SENDING TABLE
                    var query = from request in _context.SSv3_ITEM_REQUESTTBL
                                where request.isActive == 1 && request.RequestStatus.ToLower() == "approved" && requestIds.Contains(request.Id)
                                join item in _context.SSv3_GAL_ITEMTBL on request.InventoryItemId equals item.Id
                                where item.isActive == 1
                                join requestor in _context.SSv3_GAL_USERTBL on request.RequestorUserId equals requestor.Id
                                where requestor.isActive == 1
                                join approver in _context.SSv3_GAL_USERTBL on request.ApproverUserId equals approver.Id
                                where approver.isActive == 1
                                select new OrderSummary
                                {
                                    RequestCategory = request.RequestCategory,
                                    OrderNumber = request.OrderNumber,
                                    ItemNumber = item.ItemNumber,
                                    QuantityRequested = request.QuantityRequested,
                                    UnitOfMeasure = item.UnitOfMeasure,
                                    ItemDescription = item.Description,
                                    Purpose = request.Purpose
                                };

                    //CONVERT LIST TO DATATABLE
                    ListtoDataTableConverter converter = new ListtoDataTableConverter();
                    string[] colName = { "Request Category", "Order Number", "Item Number", "Quantity Requested", "Unit Of Measure", "Item Description", "Purpose" }; //Column Names for the tables
                    dt = converter.ToDataTable(query.ToList(), colName);

                    //FOR ALERT IN THE VIEW
                    TempData["RequestApproved"] = AID;

                }
                else if (YN == "2")
                {
                    emailSubject = "Shop Supplies V3 Notification - Item Request - DECLINED";

                    foreach (int order in requestIds)
                    {
                        var updateRequest = await _context.SSv3_ITEM_REQUESTTBL.FindAsync(order);
                        decimal requestedQty = updateRequest.QuantityRequested;
                        updateRequest.RequestStatus = "Declined";
                        updateRequest.DateApproved = DateTime.Now;
                        await _context.SaveChangesAsync();

                        var inventoryItem = await (from r in _context.SSv3_ITEM_REQUESTTBL where r.isActive == 1 && r.Id == order join inventory in _context.SSv3_GAL_INVENTORYTBL on r.InventoryItemId equals inventory.IncommingItemId select inventory).FirstOrDefaultAsync();
                        var updateInventory = await _context.SSv3_GAL_INVENTORYTBL.FindAsync(inventoryItem.Id);
                        updateInventory.InRequestTotalQuantity -= requestedQty;
                        await _context.SaveChangesAsync();
                    };

                    //UPDATE THE APPROVALS TABLE
                    int approvalId = int.Parse(AID.Substring(0, AID.IndexOf("-"))); //get the approval id from the formated ForApprovalId
                    var updateApproval = _context.SSv3_GAL_APPROVALSTBL.Find(approvalId);
                    updateApproval.ForApprovalStatus = "Declined";
                    await _context.SaveChangesAsync();

                    //QUERY THE UPDATED REQUEST FOR EMAIL SENDING TABLE
                    var query = from request in _context.SSv3_ITEM_REQUESTTBL
                                where request.isActive == 1 && request.RequestStatus.ToLower() == "declined" && requestIds.Contains(request.Id)
                                join item in _context.SSv3_GAL_ITEMTBL on request.InventoryItemId equals item.Id
                                where item.isActive == 1
                                join requestor in _context.SSv3_GAL_USERTBL on request.RequestorUserId equals requestor.Id
                                where requestor.isActive == 1
                                join approver in _context.SSv3_GAL_USERTBL on request.ApproverUserId equals approver.Id
                                where approver.isActive == 1
                                select new OrderSummary
                                {
                                    RequestCategory = request.RequestCategory,
                                    OrderNumber = request.OrderNumber,
                                    ItemNumber = item.ItemNumber,
                                    QuantityRequested = request.QuantityRequested,
                                    UnitOfMeasure = item.UnitOfMeasure,
                                    ItemDescription = item.Description,
                                    Purpose = request.Purpose
                                };

                    //CONVERT LIST TO DATATABLE
                    ListtoDataTableConverter converter = new ListtoDataTableConverter();
                    string[] colName = { "Request Category", "Order Number", "Item Number", "Quantity Requested", "Unit Of Measure", "Item Description", "Purpose" }; //Column Names for the tables
                    dt = converter.ToDataTable(query.ToList(), colName);

                    //FOR ALERT IN THE VIEW
                    TempData["RequestDeclined"] = AID;
                }

                //CONVERT DATATBLE TO HTML TABLE FOR EMAIL SENDING
                string htmlTable = ConvertionHelpers.ConvertDataTableToHTML(dt);


                //AUTO EMAIL
                var approverDetail = await (from user in _context.SSv3_GAL_USERTBL where user.UserName == CU && user.isActive == 1 && user.isApprover == true select user).FirstOrDefaultAsync();
                string approverName = approverDetail.Name;
                //string approverEmail = approverDetail.EmailAdd;
                int requestorId = await (from request in _context.SSv3_ITEM_REQUESTTBL where request.isActive == 1 && requestIds.Contains(request.Id) group request by request.RequestorUserId into requestor select requestor.Key).FirstOrDefaultAsync();
                var requestorDetail = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.Id == requestorId select user).FirstOrDefaultAsync();
                string requestorName = requestorDetail.Name;
                string requestorEmail = requestorDetail.EmailAdd;

                //GET EMAIL BODY
                string emailBody = EmailSendingHelpers.GetEmaiBody(yn, RD, approverName, requestorName, htmlTable, AID);
                //SEND EMAIL
                EmailSendingHelpers.SendEmail(requestorEmail, emailBody, emailSubject);

                await _context.Database.CommitTransactionAsync();
              
                return RedirectToAction("Index","Approvals");

            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                return RedirectToAction("ApprovalError", "Errors");
            }
        }

        //API FOR AUTOEMAIL ACCEPT/REJECT OF PULLOUT REQUEST
        //IN - Incoming Id (Id in incoming table)
        //RI - Request Id (Id in request Table)
        //CU - CurrentUser (equivalent to the approver/buyer username)
        //YN - YES OR NO (Approved or Declined)
        //[HttpPost]
        public async Task<IActionResult> PulloutApproval(string RI, string CU, int IN, string YN)
        {
            //CHECK IF USER IS IN THE USER TABLE
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefaultAsync();
            
            //For email sending details
            string? itemNumber = "";
            decimal quantity = 0;
            string reason = "";

            //If USER IS NOT IN THE USERTABLE ERROR
            if (userQuery == null)
            {
                return RedirectToAction("Error404", "Errors");
            }
            try
            {
                //VALIDATE THE API PARAMETERS
                if (!validateRI(RI)) { return RedirectToAction("InvalidINError", "Errors"); }
                if (!await validateBuyer(CU, RI)) { return RedirectToAction("InvalidCUError", "Errors"); }
                if (!validateIN(IN)) { return RedirectToAction("InvalidCUError", "Errors"); }
                if (!validateYN(YN)) { return RedirectToAction("InvalidYNError", "Errors"); }

                //IF API PARAMETERS ARE VALID UPDATE BASED ON YN
                int yn = int.Parse(YN);
                int requestId = int.Parse(RI);
                await _context.Database.BeginTransactionAsync();

                //Email Subject based on approved (YN=1) or declined (YN=2)
                string emailSubject = "";

                if (YN == "1") //APPROVE THE CURRENT REQUEST
                {
                    emailSubject = "Shop Supplies V3 Notification - Item Pullout Request - APPROVED ";
                    
                    //UPDATE THE REQUEST STATUS
                    var updateRequest = await _context.SSv3_ITEM_REQUESTTBL.FindAsync(requestId);
                    updateRequest.RequestStatus = "Approved";
                    updateRequest.DateApproved = DateTime.Now;
                    await _context.SaveChangesAsync();

                    int historyId = int.Parse(updateRequest.OrderNumber.ToLower().Replace("pout-", ""));

                    //UPDATE THE INCOMING HISTORY TABLE STATUS TO APPROVED
                    var incomingHistory = await _context.SSv3_GAL_INCOMING_HISTORYTBL.FindAsync(historyId);
                    incomingHistory.Status = "Approved";
                    await _context.SaveChangesAsync();

                    //ITEM REQUEST FOR APPROVAL IS ALREADY ON HOLD
                    //NO NEED TO SUBTRACT FROM THE INVENTORY QUANTITY

                    //For item Number
                    itemNumber = await (from item in _context.SSv3_GAL_ITEMTBL where item.isActive == 1 && item.Id == updateRequest.InventoryItemId select item.ItemNumber).FirstOrDefaultAsync();
                    //for request pullout quantity
                    quantity = updateRequest.QuantityRequested;
                    //for pullout reason
                    reason = updateRequest.Purpose;

                    if (string.IsNullOrEmpty(itemNumber))
                    {
                        return RedirectToAction("ItemNumberError", "Errors");
                    }

                    //FOR ALERT IN THE VIEW
                    TempData["RequestApproved"] = RI;

                }

                else if (YN == "2") //APPROVE THE CURRENT REQUEST
                {
                    emailSubject = "Shop Supplies V3 Notification - Item Pullout Request - DECLINED ";

                    //UPDATE THE REQUEST STATUS
                    var updateRequest = await _context.SSv3_ITEM_REQUESTTBL.FindAsync(requestId);
                    updateRequest.RequestStatus = "Declined";
                    updateRequest.DateApproved = DateTime.Now;
                    await _context.SaveChangesAsync();

                    int historyId = int.Parse(updateRequest.OrderNumber.ToLower().Replace("pout-", ""));

                    //UPDATE THE INCOMING HISTORY TABLE STATUS TO APPROVED
                    var incomingHistory = await _context.SSv3_GAL_INCOMING_HISTORYTBL.FindAsync(historyId);
                    incomingHistory.Status = "Declined";
                    await _context.SaveChangesAsync();

                    //ADD THE PULLOUT REQUEST QUANTITY (ON HOLD) BACK TO THE INVENTORY 
                    var incomingItem = await _context.SSv3_GAL_INCOMMINGTBL.FindAsync(IN);
                    incomingItem.TotalOnHoldQuantity -= updateRequest.QuantityRequested;
                    _context.SaveChanges();

                    //For item Number
                    itemNumber = await (from item in _context.SSv3_GAL_ITEMTBL where item.isActive == 1 && item.Id == updateRequest.InventoryItemId select item.ItemNumber).FirstOrDefaultAsync();
                    //for request pullout quantity
                    quantity = updateRequest.QuantityRequested;
                    //for pullout reason
                    reason = updateRequest.Purpose;


                    //FOR ALERT IN THE VIEW
                    TempData["RequestDeclined"] = RI;
                }


                //AUTO EMAIL
                var approverDetail = await (from user in _context.SSv3_GAL_USERTBL where user.UserName == CU && user.isActive == 1 select user).FirstOrDefaultAsync();
                string approverName = approverDetail.Name;
                int requestorId = await (from request in _context.SSv3_ITEM_REQUESTTBL where request.isActive == 1 && request.Id == requestId select request.RequestorUserId).FirstOrDefaultAsync();
                var requestorDetail = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.Id == requestorId select user).FirstOrDefaultAsync();
                string requestorName = requestorDetail.Name;
                string requestorEmail = requestorDetail.EmailAdd;

                //GET EMAIL BODY
                string emailBody = EmailSendingHelpers.GetEmaiBodyPullout(yn, approverName, itemNumber, quantity.ToString(), reason);
                //SEND EMAIL
                EmailSendingHelpers.SendEmail(requestorEmail, emailBody, emailSubject);

                await _context.Database.CommitTransactionAsync();

                return RedirectToAction("Index", "Approvals");
            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                return RedirectToAction("PulloutApprovalError", "Errors");
            }
        }



        //VALIDATE REQUEST DATE BASED ON ORDER NUMBER
        public bool validateON(string ON)
        {
            string[] orderNumbers = ON.Split(',');

            foreach(string orderNumber in orderNumbers)
            {
                if (!int.TryParse(orderNumber, out int orderNum))
                {
                    return false;
                }
            }
            return true;
        }

        //VALIDATE REQUEST DATE BASED ON CURRENT USER
        public async Task<bool> validateCU(string CU, string ON)
        {
            string[] requests = ON.Split(",");
            int[] requestIds;

            if (!validateON(ON)) //CHECK IF Order Numbers are VALID
            {
                return false;
            }
            else
            {
                requestIds = Array.ConvertAll(requests, int.Parse);

                if (CU.ToLower() != User.Identity.Name.Substring(5).ToLower())
                {
                    return false;
                }
                else
                {
                    try
                    {
                        int cuId = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == CU select user.Id).FirstOrDefaultAsync();

                        //QUERY THE DATABASE TO CHECK IF THE APPROVER IS VALID FOR THE REQUEST
                        var query = from request in _context.SSv3_ITEM_REQUESTTBL
                                    where request.isActive == 1 && request.RequestStatus.ToLower() == "for approval" && requestIds.Contains(request.Id)
                                    join approver in _context.SSv3_GAL_USERTBL on request.ApproverUserId equals approver.Id
                                    where approver.isActive == 1
                                    select request.ApproverUserId;
                        var queryList = await query.ToListAsync();

                        if (queryList.All(o => o == queryList.First())) //CHECK IF THE QUERY OUTPUT HAS THE SAME APPROVERS
                        {
                            if(cuId == queryList.First()) //IF ALL REQUEST HAVE ONLY ONE APPROVER CHECK IF THE CURRENT USER IS THE APPROVER
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        } 
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

           
        }

        //VALIDATE REQUEST DATE BASED ON ACCEPT OR DECLINED
        public bool validateYN(string YN)
        {
            if (int.TryParse(YN, out int ynNum))
            {
                if (ynNum == 1 || ynNum == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        //VALIDATE REQUEST DATE BASED ON ADVANCE ORDER OR NOT (CUT-OFF IS 1 DAY AT 2PM FOR REGULAR ORDER AND 2 DAYS AT 2PM FOR ADVANCE ORDER)
        public bool validateRD(string RD, string AO)
        {
            try
            {
                if (int.TryParse(AO, out int isAdvance))
                {
                    //if (DateTime.TryParseExact(RD, "g", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime requestedDate))
                    if (DateTime.TryParse(RD, out DateTime requestedDate))
                    {
                        if (isAdvance == 0)
                        {
                            if (DateTime.Now == requestedDate.AddDays(1))
                            {
                                if (DateTime.Now.Hour <= 13)
                                {
                                    if(DateTime.Now.Minute < 59)
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else if(DateTime.Now < requestedDate.AddDays(1))
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else if (isAdvance == 1)
                        {
                            if (DateTime.Now <= requestedDate.AddDays(2))
                            {
                                if (DateTime.Now.Hour <= 13)
                                {
                                    if (DateTime.Now.Minute < 59)
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        return true;
                                    }
                                }
                                else
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

        }

        //CHECKING IF REQUEST ARE ALREADY APPROVE
        public bool isApprovedOrDeclined(string ON)
        {
            string[] requests = ON.Split(',');
            int[] requestIds = Array.ConvertAll(requests, int.Parse);

            //VALIDATE IF ORDER IS ALREADY APPROVED OR DECLINED
            var query = (from request in _context.SSv3_ITEM_REQUESTTBL 
                        where request.isActive == 1 && request.RequestStatus.ToLower() == "for approval" && requestIds.Contains(request.Id)
                        select request.ApproverUserId).FirstOrDefault();
            if(query == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //VALIDATE THE REQUEST BASED ON PULLOUT ITEM NUMBER
        public bool validateRI(string RI)
        {
            if (!int.TryParse(RI, out int itemNumber))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //VALIDATE INCOMING ID
        public bool validateIN(int IN)
        {
            try
            {
                var query = from incoming in _context.SSv3_GAL_INCOMMINGTBL where incoming.isActive == 1 && incoming.Id == IN select incoming;
                if(query != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        //VALIDATE REQUEST DATE BASED ON CURRENT USER
        public async Task<bool> validateBuyer(string CU, string RI)
        {
            if (!validateRI(RI)) //CHECK IF Request Id is VALID Number
            {
                return false;
            }
            else
            {               
                if (CU.ToLower() != User.Identity.Name.Substring(5).ToLower())
                {
                    return false;
                }
                else
                {
                    try
                    {
                        int cuId = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == CU select user.Id).FirstOrDefaultAsync();
                        int requestId = int.Parse(RI);

                        //QUERY THE DATABASE TO CHECK IF THE APPROVER IS VALID FOR THE REQUEST
                        var query = from r in _context.SSv3_ITEM_REQUESTTBL
                                    where r.isActive == 1 && r.Id == requestId && r.RequestStatus.ToLower() == "for approval"
                                    join approver in _context.SSv3_GAL_USERTBL on r.ApproverUserId equals approver.Id
                                    where approver.isActive == 1 && approver.Id == cuId
                                    select r.ApproverUserId;

                        if (query != null) //IF REQUEST APPROVER AND CURRENT USER ARE THE SAME
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
            }


        }

    }
}
