using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ShopSuppliesV3.Data;
using ShopSuppliesV3.Models;
using ShopSuppliesV3.ViewModel;
using System.Data;
using System.Net.Mail;
using System.Reflection;
using ShopSuppliesV3.Helpers;
using System.Collections.Generic;

namespace ShopSuppliesV3.Controllers
{
    public class CartsController : Controller
    {
        private readonly ShopSuppliesV3Context _context;

        public CartsController(ShopSuppliesV3Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                string currentUser = User.Identity.Name.Substring(5);
                var checkRequestor = await (from user in _context.SSv3_GAL_USERTBL where user.UserName.ToLower() == currentUser.ToLower() select user).FirstOrDefaultAsync();
                if (checkRequestor == null)
                {
                    return RedirectToAction("Error404", "Errors");
                }

                var query = from request in _context.SSv3_ITEM_REQUESTTBL
                            //where request.isActive == 1 && (request.RequestStatus.ToLower() == "new" || request.RequestStatus.ToLower() == "approved") && request.RequestorUserId == checkRequestor.Id
                            where request.isActive == 1 && request.RequestStatus.ToLower() == "new" && request.RequestorUserId == checkRequestor.Id
                            join item in _context.SSv3_GAL_ITEMTBL on request.InventoryItemId equals item.Id
                            where item.isActive == 1
                            //join user in _context.SSv3_GAL_USERTBL on request.ApproverUserId equals user.Id
                            //where user.isActive == 1
                            select new Requests
                            {
                                Id = request.Id,
                                BUname = request.BUname,
                                OrderNumber = request.OrderNumber,
                                RequestCategory = request.RequestCategory,
                                ItemNumber = item.ItemNumber,
                                ItemDescription = item.Description,
                                QuantityRequested = request.QuantityRequested,
                                UnitOfMeasure = item.UnitOfMeasure,
                                RequestStatus = request.RequestStatus,
                            };

                var requestList = await query.ToListAsync();

                var approvers = from a in _context.SSv3_GAL_USERTBL where a.isApprover == true select a;

                int requestorUserId = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user.Id).FirstOrDefaultAsync();
                int cartItemCount = await (from request in _context.SSv3_ITEM_REQUESTTBL where request.isActive == 1 && (request.RequestStatus.ToLower() == "new") && request.RequestorUserId == requestorUserId select request.Id).CountAsync();
                TempData["ItemCount"] = cartItemCount;

                ViewData["Approvers"] = await approvers.ToListAsync();
                return View(requestList);
            }

            catch
            {
                return RedirectToAction("CartError", "Errors");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckOut(IFormCollection formData)
        {
            //CHECK IF currentUser IS IN THE USER TABLE
            string currentUser = User.Identity.Name.Substring(5);
            var checkRequestor = await (from user in _context.SSv3_GAL_USERTBL where user.UserName.ToLower() == currentUser.ToLower() select user).FirstOrDefaultAsync();
            int isAdvance = 0;

            //For checking if advance order
            if (int.TryParse(formData["IsAdvance"].ToString(), out int num))
            {
                isAdvance = num;
            }

            if (checkRequestor == null)
            {
                return RedirectToAction("Error404", "Errors");
            }

            string selectAllBtn = formData["selectAllBtn"].ToString();
            string requestIdAll = formData["orderIdAll"].ToString();
            string selectedRequest = formData["selectItem"].ToString();
            int approverUserId = int.Parse(formData["Approver"].ToString());
            string[] requests;
            string orderNumbers; //used for email sending and approvals table order numbers column

            try
            {
                if(selectAllBtn.ToLower() == "on")
                {
                    //Transform orderIdAll to Array
                    requests = requestIdAll.Split(','); //get all the items in the cart
                    orderNumbers = requestIdAll;
                }
                else
                {
                    requests = selectedRequest.Split(","); // get the selected items in the cart
                    orderNumbers = selectedRequest;
                }

                int[] requestIds = Array.ConvertAll(requests, int.Parse);

                _context.Database.BeginTransaction();
                
                //Update Each Request based on selected Approver
                foreach(int requestId in requestIds)
                {
                    string date = DateTime.Now.ToString("MM-yyyy").Remove(2, 3);
                    string id = requestId.ToString("000000");

                    var updateRequest =  await _context.SSv3_ITEM_REQUESTTBL.FindAsync(requestId);
                    updateRequest.ApproverUserId = approverUserId;
                    updateRequest.isAdvance = isAdvance;
                    updateRequest.RequestStatus = "For Approval";
                    updateRequest.OrderNumber = $"{date}-{id}";
                    await _context.SaveChangesAsync();
                    
                };

             
                //QUERY THE UPDATED REQUEST FOR EMAIL SENDING TABLE
                var query = from request in _context.SSv3_ITEM_REQUESTTBL
                            where request.isActive == 1 && request.RequestStatus.ToLower() == "for approval" && requestIds.Contains(request.Id)
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


                //FOR AUTO EMAIL
                var approverDetail = await (from user in _context.SSv3_GAL_USERTBL where user.Id == approverUserId && user.isActive == 1 && user.isApprover == true select user).FirstOrDefaultAsync();
                string approverUsername = approverDetail.UserName;
                string approverEmail = approverDetail.EmailAdd;
                string requestorName = await (from user in _context.SSv3_GAL_USERTBL where user.UserName == currentUser select user.Name).FirstOrDefaultAsync();
                //string storeEmail = "christian.garci2@collins.com"; //CHANGE THIS WHEN DEPLOYED


                //URL PATTERN FOR ACCEPT/DECLINE
                //string dateRequested = DateTime.Now.AddDays(1).ToString("MMM-dd-yyyy");
                //string approvedHref = "http://ta5-12ls0d3/SSV3//Approvals/UpdateRequest?ON=" + orderNumbers + "&CU=" + approverUsername + "&YN=" + 1 + "&RD=" + dateRequested + "&AO=" + isAdvance + "&AID=" +  ""; //CHANGE THIS BASED ON API LINK (AO advanceOrder and will depend on isAdvance paramete)
                //string declinedHref = "http://ta5-12ls0d3/SSV3//Approvals/UpdateRequest?ON=" + orderNumbers + "&CU=" + approverUsername + "&YN=" + 2 + "&RD=" + dateRequested + "&AO=" + isAdvance + "&AID=" +  ""; //CHANGE THIS BASED ON API LINK (AO advanceOrder and will depend on isAdvance paramete)

                //Add the orders in APPROVALS TABLE
                SSv3_GAL_APPROVALSTBL newForApproval = new SSv3_GAL_APPROVALSTBL
                {
                    RequestorUsername = currentUser,
                    ForApprovalStatus = "For Approval",
                    ApproverUserName = approverUsername,
                    OrderNumbers = orderNumbers,
                    RequestedDate = DateTime.Now,
                    IsAdvanceOder = isAdvance
                };

                _context.Add(newForApproval);
                await _context.SaveChangesAsync();

                //DELETE ITEMS NOT INCLUDED IN CHECK-OUT
                string[] allId = requestIdAll.Split(","); //Get All the requestId in the cart
                foreach(string id in allId) //iterate on each item
                {
                    if (!requests.Contains(id)) //Check if the id is not the request that was checkout
                    {
                        int requestId = int.Parse(id); //request Id to be deleted

                        //CHECK IF ITEM EXIST IN CART
                        var requestQuery = await (from request in _context.SSv3_ITEM_REQUESTTBL where request.isActive == 1 && request.Id == requestId && request.RequestStatus.ToLower() == "new" select request).FirstOrDefaultAsync();
                        if (requestQuery == null)
                        {
                            _context.Database.RollbackTransaction();

                            return RedirectToAction("CheckOutError", "Errors");
                        }

                        //IF OK DELETE THE ITEM
                        requestQuery.isActive = 0;
                        requestQuery.DateDeleted = DateTime.Now;
                        requestQuery.DeleteByUserName = currentUser;
                        await _context.SaveChangesAsync();

                        //SUBTRACT THE ITEM QUANTITY IN THE INVENTORY TABLE
                        var inventoryQuery = await (from inventory in _context.SSv3_GAL_INVENTORYTBL where inventory.IncommingItemId == requestQuery.InventoryItemId select inventory).FirstOrDefaultAsync();
                        inventoryQuery.InRequestTotalQuantity -= requestQuery.QuantityRequested;
                        await _context.SaveChangesAsync();
                    }
                }


                //CONVERT LIST TO DATATABLE
                ListtoDataTableConverter converter = new ListtoDataTableConverter();
                string[] colName = { "Request Category", "Order Number", "Item Number", "Quantity Requested", "Unit Of Measure", "Item Description", "Purpose" }; //Column Names for the tables
                DataTable dt = converter.ToDataTable(query.ToList(), colName);

                //CONVERT DATATBLE TO HTML TABLE FOR EMAIL SENDING
                string htmlTable = ConvertionHelpers.ConvertDataTableToHTML(dt);

                //Get the request Id of the newly created requests and the string format of forApprovalId
                string forApprovalId = newForApproval.Id.ToString() + "-" + newForApproval.RequestedDate.ToString("yyyyMMdd");

                //GET EMAIL BODY
                string emailBody = EmailSendingHelpers.GetEmaiBody(orderNumbers, approverUsername, requestorName, htmlTable, isAdvance, forApprovalId);
                //SEND EMAIL
                string emailSubject = "Shop Supplies V3 Notification - Item Request - FOR APPROVAL";
                EmailSendingHelpers.SendEmail(approverEmail, emailBody, emailSubject);

                _context.Database.CommitTransaction();

                TempData["CheckoutSuccess"] = "success";
                return RedirectToAction("Index");
            }
            catch
            {
                _context.Database.RollbackTransaction();

                return RedirectToAction("CheckOutError", "Errors");
            }
        }

        //FOR GETTING THE ITEM ID DATA TO BE DELETED USING FETCH API IN THE CART INDEX DELETE MODAL
        [Route("[controller]/[action]/{requestItemId}")] //Route is added (to match the fetch @url action)
        public async Task<string> DeleteRequestItem(int requestItemId)
        {
            //FOR CURRENT USER ACCESS ONLY
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

            //If USER IS NOT IN THE USERTABLE ERROR OR ADMIN
            if (userQuery == null || userQuery.AccessType.ToLower() != "admin")
            {
                return "UnAuthorized User";
            }

            try
            {
                //CHECK IF ITEM EXIST IN CART
                var requestQuery = await (from request in _context.SSv3_ITEM_REQUESTTBL where request.isActive == 1 && request.Id == requestItemId && request.RequestStatus.ToLower() == "new" select request).FirstOrDefaultAsync();
                if (requestQuery == null)
                {
                    return "Item Does Not Exist";
                }

               await _context.Database.BeginTransactionAsync();

                //IF OK DELETE THE ITEM
                requestQuery.isActive = 0;
                requestQuery.DateDeleted = DateTime.Now;
                requestQuery.DeleteByUserName = currentUser;
                await _context.SaveChangesAsync();

                //SUBTRACT THE ITEM QUANTITY IN THE INVENTORY TABLE
                var inventoryQuery = await (from inventory in _context.SSv3_GAL_INVENTORYTBL where inventory.IncommingItemId == requestQuery.InventoryItemId select inventory).FirstOrDefaultAsync();
                inventoryQuery.InRequestTotalQuantity -= requestQuery.QuantityRequested;
                await _context.SaveChangesAsync();

                await _context.Database.CommitTransactionAsync();

                return "Delete Success"; //Used in alert in view
            }

            catch
            {
                await _context.Database.RollbackTransactionAsync();
                return "Delete Error"; //Used in alert in view
            }
        }


        public async Task<IActionResult> EditRequestItem(int requestId)
        {
            //CHECK IF currentUser IS IN THE USER TABLE
            string currentUser = User.Identity.Name.Substring(5);
            var checkRequestor = await (from user in _context.SSv3_GAL_USERTBL where user.UserName.ToLower() == currentUser.ToLower() select user).FirstOrDefaultAsync();

            if (checkRequestor == null)
            {
                return RedirectToAction("Error404", "Errors");
            }

            try
            {
                _context.Database.BeginTransaction();

                var query = from request in _context.SSv3_ITEM_REQUESTTBL
                            where request.isActive == 1 && request.Id == requestId && request.RequestorUserId == checkRequestor.Id
                            join inventory in _context.SSv3_GAL_INVENTORYTBL on request.InventoryItemId equals inventory.IncommingItemId
                            join item in _context.SSv3_GAL_ITEMTBL on request.InventoryItemId equals item.Id
                            where item.isActive == 1
                            select new Requests
                            {
                                ItemId = item.Id,
                                Id = request.Id,
                                ItemNumber = item.ItemNumber,
                                RequestCategory = request.RequestCategory,
                                AreaName = request.AreaName,
                                QuantityRequested = request.QuantityRequested,
                                Purpose = request.Purpose,
                                RequestorName = checkRequestor.Name,
                                RemainingQuntityToRequest = inventory.RemainingStocksToRequest,
                                UnitOfMeasure = item.UnitOfMeasure
                            };

                //CHECK IF REQUEST ID EXIST
                if (query == null)
                {
                    TempData["ItemDoesNotExist"] = requestId; //FOR ALERT IN THE INDEX VIEW
                    return RedirectToAction("Index");
                }

                var categories = from category in _context.SSv3_ITEM_CATEGORYTBL where category.isActive == 1 select category.CategoryName;
                var areas = from area in _context.SSv3_GAL_AREATBL where area.isActive == 1 select area.AreaName;

                ViewData["RequestCategory"] = await categories.ToListAsync();
                ViewData["Areas"] = await areas.ToListAsync();

                var requestData = await query.FirstOrDefaultAsync();
                return View(requestData);
            }
            catch
            {
                return RedirectToAction("CartOrderEditError", "Errors");
            }
        }


        //FOR CONTINUATION - SUBTRACT QTY FROM INVENTORY THEN ADD NEW QTY TO INVENTORY
        [HttpPost]
        public async Task<IActionResult> EditRequestItem(Requests modelData)
        {
            
            //CHECK IF currentUser IS IN THE USER TABLE
            string currentUser = User.Identity.Name.Substring(5);
            var checkRequestor = await (from user in _context.SSv3_GAL_USERTBL where user.UserName.ToLower() == currentUser.ToLower() select user).FirstOrDefaultAsync();

            if (checkRequestor == null)
            {
                return RedirectToAction("Error404", "Errors");
            }

            try
            {
                _context.Database.BeginTransaction();

                var queryRequest = await _context.SSv3_ITEM_REQUESTTBL.FindAsync(modelData.Id);

                //CHECK IF REQUEST ID EXIST
                if (queryRequest == null)
                {
                    TempData["ItemDoesNotExist"] = "test"; //FOR ALERT IN THE INDEX VIEW
                    return RedirectToAction("Index");
                }

                var inventoryQuery = await (from inventory in _context.SSv3_GAL_INVENTORYTBL where inventory.IncommingItemId == modelData.ItemId select inventory).FirstOrDefaultAsync();
                inventoryQuery.InRequestTotalQuantity -= queryRequest.QuantityRequested;
                inventoryQuery.InRequestTotalQuantity += modelData.QuantityRequested;
                await _context.SaveChangesAsync();

                queryRequest.QuantityRequested = modelData.QuantityRequested;
                queryRequest.AreaName = modelData.AreaName;
                queryRequest.RequestCategory = modelData.RequestCategory;
                queryRequest.Purpose = modelData.Purpose;
                await _context.SaveChangesAsync();

                _context.Database.CommitTransaction();

                return RedirectToAction("Index");
            }
            catch
            {
                _context.Database.RollbackTransaction();
                return RedirectToAction("CartOrderEditError", "Errors");
            }
        }
    }
}
