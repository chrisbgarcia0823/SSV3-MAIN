using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ShopSuppliesV3.Data;
using ShopSuppliesV3.Models;
using ShopSuppliesV3.ViewModel;
using System.Xml.Linq;
using System.Net.Mail;
using static System.Net.Mime.MediaTypeNames;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using ShopSuppliesV3.Helpers;
using Humanizer;


namespace ShopSuppliesV3.Controllers
{
    public class RequestsController : Controller
    {
        private readonly ShopSuppliesV3Context _context;

        public RequestsController(ShopSuppliesV3Context context)
        {
            _context = context;
        }


        public async Task<IActionResult> Create(int inventoryId, int itemId, string itemNumber, decimal remainingStocksToRequest, string description, string unitOfMeasure, string requestorUserName, string buName)
        {
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

            //If USER IS NOT IN THE USERTABLE ERROR
            if (userQuery == null)
            {
                return RedirectToAction("Error404", "Errors");
            }

            ////FOR CART ITEM COUNT
            //string userName = User.Identity.Name.Substring(5);
            //int requestorUserId = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == userName select user.Id).FirstOrDefaultAsync();
            //int cartItemCount = await (from request in _context.SSv3_ITEM_REQUESTTBL where request.isActive == 1 && (request.RequestStatus.ToLower() == "new" || request.RequestStatus.ToLower() == "approved") && request.RequestorUserId == requestorUserId select request.Id).CountAsync();
            //TempData["ItemCount"] = cartItemCount;


            string requestorName = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == requestorUserName select user.Name).FirstOrDefaultAsync();
            TempData["BUname"] = buName;
            TempData["InventoryId"] = inventoryId;
            TempData["InventyoryItemId"] = itemId;
            TempData["ItemNumber"] = itemNumber;
            TempData["RemainingStocksToRequest"] = remainingStocksToRequest;
            TempData["Description"] = description;
            TempData["UnitOfMeasure"] = unitOfMeasure;
            TempData["RequestorName"] = requestorName;

            var categories = from category in _context.SSv3_ITEM_CATEGORYTBL where category.isActive == 1 select category.CategoryName;
            var areas = from area in _context.SSv3_GAL_AREATBL where area.isActive == 1 select area.AreaName;
 
            ViewData["RequestCategory"] = await categories.ToListAsync();
            ViewData["Areas"] = await areas.ToListAsync();

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(IFormCollection formData)
        {
            try
            {
                //int approverUserId = 1; //For sample only. Change when user access is made              
                string requestorUserName = formData["RequestorUserName"].ToString();
                string buName = formData["BUname"].ToString();
                string areaName = formData["AreaName"].ToString();
                string requestCategory = formData["RequestCategory"].ToString();
                //int approverId = int.Parse(formData["Approver"].ToString());
                var requestorDetails = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == requestorUserName select user).FirstOrDefaultAsync();
                //var approverDetails = await (from approver in _context.SSv3_GAL_USERTBL where approver.isActive == 1 && approver.isApprover == true && approver.Id == approverId select approver).FirstOrDefaultAsync();
                int inventyoryItemId = int.Parse(formData["inventyoryItemId"].ToString());
                decimal quantityRequested = decimal.Parse(formData["QuantityRequested"].ToString());
                string purpose = formData["Purpose"].ToString();
                string requestStatus = "New";

                //Save the New Request
                _context.Database.BeginTransaction(); //If for what ever reason the update on the database did not proceed, this function together with the roll back can ensure that database is not updated
                SSv3_ITEM_REQUESTTBL newRequest = new SSv3_ITEM_REQUESTTBL
                {
                    isActive = 1,
                    RequestCategory = requestCategory,
                    BUname = buName,
                    AreaName = areaName,
                    RequestStatus = requestStatus,
                    RequestorUserId = requestorDetails.Id,
                    InventoryItemId = inventyoryItemId,
                    QuantityRequested = quantityRequested,
                    Purpose = purpose,
                    ApproverUserId = null,
                    DateRequested = DateTime.Now,
                    DateApproved = null,
                };

                _context.Add(newRequest);
                await _context.SaveChangesAsync();

                //Subtract the new request quantity to the inventory RemainingStocksToRequest by updating the record
                int inventoryId = int.Parse(formData["InventoryId"].ToString());

                var updateInventory = _context.SSv3_GAL_INVENTORYTBL.Find(inventoryId);
                updateInventory.InRequestTotalQuantity += quantityRequested;

                _context.Update(updateInventory); //Update the database
                await _context.SaveChangesAsync(); //Save the changes

                _context.Database.CommitTransaction(); //Commit the transaction and apply the changes (used in conjuction with BeginTransaction() function)
                
                TempData["ItemNumberRequestSuccess"] = formData["ItemNumber"];
                return RedirectToAction("Index", "Store");
            }

            catch
            {
                _context.Database.RollbackTransaction(); // Rollback the transaction made in the database in cases of error (used in conjuction with BeginTransaction and CommitTransaction function)
                TempData["ItemRequestError"] = formData["ItemNumber"].ToString();
                return RedirectToAction("ItemRequestError", "Errors");
            }
        }
        

        public async Task<IActionResult> MyRequest()
        {
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();
            //If USER IS NOT IN THE USERTABLE ERROR
            if (userQuery == null)
            {
                return RedirectToAction("Error404", "Errors");
            }

            try
            {
                var query = from request in _context.SSv3_ITEM_REQUESTTBL
                            where request.isActive == 1 && request.RequestorUserId == userQuery.Id
                            join inventory in _context.SSv3_GAL_INVENTORYTBL on request.InventoryItemId equals inventory.IncommingItemId
                            join item in _context.SSv3_GAL_ITEMTBL on inventory.IncommingItemId equals item.Id
                            where item.isActive == 1
                            join supplier in _context.SSv3_GAL_SUPPLIERTBL on item.SupplierId equals supplier.Id
                            where supplier.isActive == 1
                            join user in _context.SSv3_GAL_USERTBL on request.RequestorUserId equals user.Id
                            where user.isActive == 1
                            join approver in _context.SSv3_GAL_USERTBL on request.ApproverUserId equals approver.Id
                            orderby request.DateRequested descending
                            where approver.isActive == 1
                            select new Requests
                            {
                                Id = request.Id,
                                //RequestorUserId = request.RequestorUserId,
                                AreaName = request.AreaName,
                                OrderNumber = request.OrderNumber,
                                ApproverName = approver.Name,
                                BUname = item.BUName,
                                RequestorUserId = request.RequestorUserId,
                                RequestStatus = request.RequestStatus,
                                RequestorName = user.Name,
                                ItemNumber = item.ItemNumber,
                                ItemDescription = item.Description,
                                UnitOfMeasure = item.UnitOfMeasure,
                                QuantityRequested = request.QuantityRequested,
                                //UnitPrice = request.UnitPrice,
                                Purpose = request.Purpose,
                                DateRequested = request.DateRequested,
                                DateApproved = request.DateApproved,
                            };

                var requestList = await query.ToListAsync();

                //CHECK IF THE REQUEST IS ALREADY ISSUED BUT CHECK IF FIRST IF requestList have value
                if (requestList.Count() > 0)
                {
                    var dateIssuedQuery = from outgoing in _context.SSv3_GAL_OUTGOINGTBL where outgoing.isActive == 1 && outgoing.RequestId == requestList.First().Id select outgoing.DateIssued;
                    if (dateIssuedQuery.ToList().Count() > 0)
                    {
                        TempData["DateIssued"] = await dateIssuedQuery.FirstOrDefaultAsync();
                    }
                    else
                    {
                        TempData["DateIssued"] = "";
                    }
                }

                return View(requestList);
            }
            catch
            {
                TempData["TrackingError"] = currentUser;
                return RedirectToAction("TrackingError", "Errors");
            }
        }



        public async Task<IActionResult> RequestHistory(DateTime startDate, DateTime endDate)
        {
            try
            {
                string currentUser = User.Identity.Name.Substring(5).ToLower();
                var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();
                //If USER IS NOT IN THE USERTABLE ERROR
                if (userQuery == null)
                {
                    return RedirectToAction("Error404", "Errors");
                }

                //FOR DATE FILTERs
                TempData["StartDate"] = startDate;
                TempData["EndDate"] = endDate;

                var query = from request in _context.SSv3_ITEM_REQUESTTBL
                            where request.isActive == 1 && request.RequestorUserId == userQuery.Id && request.DateRequested >= startDate && request.DateRequested <= endDate.AddDays(1)
                            join inventory in _context.SSv3_GAL_INVENTORYTBL on request.InventoryItemId equals inventory.IncommingItemId
                            join item in _context.SSv3_GAL_ITEMTBL on inventory.IncommingItemId equals item.Id
                            where item.isActive == 1
                            join user in _context.SSv3_GAL_USERTBL on request.RequestorUserId equals user.Id
                            where user.isActive == 1
                            join approver in _context.SSv3_GAL_USERTBL on request.ApproverUserId equals approver.Id
                            orderby request.DateRequested descending
                            where approver.isActive == 1
                            select new Requests
                            {
                                Id = request.Id,
                                //RequestorUserId = request.RequestorUserId,
                                AreaName = request.AreaName,
                                OrderNumber = request.OrderNumber,
                                ApproverName = approver.Name,
                                BUname = item.BUName,
                                RequestorUserId = request.RequestorUserId,
                                RequestStatus = request.RequestStatus,
                                RequestorName = user.Name,
                                ItemNumber = item.ItemNumber,
                                ItemDescription = item.Description,
                                UnitOfMeasure = item.UnitOfMeasure,
                                QuantityRequested = request.QuantityRequested,
                                //UnitPrice = request.UnitPrice,
                                Purpose = request.Purpose,
                                DateRequested = request.DateRequested,
                                DateApproved = request.DateApproved,
                            };

                if(query == null)
                {
                    return RedirectToAction("RequestSortError", "Errors");
                }

                return View(await query.ToListAsync());

            }
            catch
            {
                return RedirectToAction("RequestSortError", "Errors");
            }         
        }


        public async Task<IActionResult> RequestDetails(int requestId)
        {
            try
            {
                string currentUser = User.Identity.Name.Substring(5).ToLower();
                var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();
                //If USER IS NOT IN THE USERTABLE ERROR
                if (userQuery == null)
                {
                    return RedirectToAction("Error404", "Errors");
                }

                Requests? requestDetail = new Requests();

                //CHECK IF requestId is valid
                var request = await _context.SSv3_ITEM_REQUESTTBL.FindAsync(requestId);
                if(request == null)
                {
                    return RedirectToAction("RequestDetailError", "Errors");
                }

                //Get the details that are not issued
                if(request.RequestStatus.ToLower() == "for approval" || request.RequestStatus.ToLower() == "new" || request.RequestStatus.ToLower() == "declined" || request.RequestStatus.ToLower() == "approved")
                {
                    var query = from r in _context.SSv3_ITEM_REQUESTTBL
                                where r.isActive == 1 && r.RequestorUserId == userQuery.Id && r.Id == requestId
                                join item in _context.SSv3_GAL_ITEMTBL on request.InventoryItemId equals item.Id
                                where item.isActive == 1
                                join requestor in _context.SSv3_GAL_USERTBL on request.RequestorUserId equals requestor.Id
                                where requestor.isActive == 1
                                join approver in _context.SSv3_GAL_USERTBL on request.ApproverUserId equals approver.Id
                                where approver.isActive == 1 && approver.isApprover == true
                                select new Requests
                                {
                                    RequestCategory = r.RequestCategory,
                                    AreaName = r.AreaName,
                                    ApproverName = approver.Name,
                                    BUname = item.BUName,
                                    RequestStatus = r.RequestStatus,
                                    RequestorName = requestor.Name,
                                    ItemNumber = item.ItemNumber,
                                    ItemDescription = item.Description,
                                    UnitOfMeasure = item.UnitOfMeasure,
                                    QuantityRequested = r.QuantityRequested,
                                    Purpose = r.Purpose,
                                    DateRequested = r.DateRequested,
                                };
                    TempData["isIssued"] = "False";
                    requestDetail = await query.FirstOrDefaultAsync();
                }
                else
                {
                    var query = from r in _context.SSv3_ITEM_REQUESTTBL
                                where r.isActive == 1 && r.RequestorUserId == userQuery.Id && r.Id == requestId
                                join outgoing in _context.SSv3_GAL_OUTGOINGTBL on r.Id equals outgoing.RequestId
                                where outgoing.isActive == 1
                                join incoming in _context.SSv3_GAL_INCOMMINGTBL on outgoing.IncommingIds equals incoming.Id
                                where incoming.isActive == 1
                                join item in _context.SSv3_GAL_ITEMTBL on request.InventoryItemId equals item.Id
                                where item.isActive == 1
                                join requestor in _context.SSv3_GAL_USERTBL on r.RequestorUserId equals requestor.Id
                                where requestor.isActive == 1
                                join approver in _context.SSv3_GAL_USERTBL on r.ApproverUserId equals approver.Id
                                where approver.isActive == 1 && approver.isApprover == true
                                join issuer in _context.SSv3_GAL_USERTBL on outgoing.IssuerUsername equals issuer.UserName
                                where issuer.isActive == 1
                                select new Requests
                                {
                                    Id = r.Id,
                                    RequestCategory = r.RequestCategory,
                                    PONumber = incoming.PONumber,
                                    AreaName = r.AreaName,
                                    OrderNumber = r.OrderNumber,
                                    ApproverName = approver.Name,
                                    BUname = item.BUName,
                                    RequestorUserId = r.RequestorUserId,
                                    RequestStatus = r.RequestStatus,
                                    RequestorName = requestor.Name,
                                    ItemNumber = item.ItemNumber,
                                    ItemDescription = item.Description,
                                    UnitOfMeasure = item.UnitOfMeasure,
                                    QuantityRequested = r.QuantityRequested,
                                    UnitCost = outgoing.UnitCost,
                                    TotalCost = outgoing.TotalCost,
                                    Purpose = r.Purpose,
                                    DateRequested = r.DateRequested,
                                    DateApproved = r.DateApproved,
                                    DateIssued = outgoing.DateIssued,
                                    QuantityIssued = outgoing.QuantityIssued,
                                    IssuerName = issuer.Name,
                                };
                    TempData["isIssued"] = "True";
                    requestDetail = await query.FirstOrDefaultAsync();
                }

                

                return View(requestDetail);
            }
            catch
            {
                return RedirectToAction("RequestDetailError", "Errors");
            }        
        }

        public async Task<IActionResult> OrderDetail(int approvalId, string requestType, int pulloutRequestId)
        {
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();
            //If USER IS NOT IN THE USERTABLE ERROR
            if (userQuery == null)
            {
                return RedirectToAction("Error404", "Errors");
            }

            try
            {
                if(!string.IsNullOrEmpty(requestType)) //requestType from approvals view Details button
                {
                    if(requestType.ToLower() == "approver") //FOR APPROVER
                    {
                        //Get the latest request from approvals table where approver is the current user
                        var approvals = await _context.SSv3_GAL_APPROVALSTBL.FindAsync(approvalId);

                        //Get all the requested item from approval table order numbers column
                        string[] requestsNumbers = approvals.OrderNumbers.Split(',');
                        int[] requestId = Array.ConvertAll(requestsNumbers, int.Parse);

                        //Query each  item to be displayed
                        var queryRequest = from request in _context.SSv3_ITEM_REQUESTTBL
                                    where request.isActive == 1 && request.RequestorUserId == userQuery.Id && requestId.Contains(request.Id)
                                    join inventory in _context.SSv3_GAL_INVENTORYTBL on request.InventoryItemId equals inventory.IncommingItemId
                                    join item in _context.SSv3_GAL_ITEMTBL on inventory.IncommingItemId equals item.Id
                                    where item.isActive == 1
                                    join supplier in _context.SSv3_GAL_SUPPLIERTBL on item.SupplierId equals supplier.Id
                                    where supplier.isActive == 1
                                    join user in _context.SSv3_GAL_USERTBL on request.RequestorUserId equals user.Id
                                    where user.isActive == 1
                                    join approver in _context.SSv3_GAL_USERTBL on request.ApproverUserId equals approver.Id
                                    orderby request.DateRequested descending
                                    where approver.isActive == 1
                                    select new Requests
                                    {
                                        Id = request.Id,
                                        //RequestorUserId = request.RequestorUserId,
                                        AreaName = request.AreaName,
                                        OrderNumber = request.OrderNumber,
                                        ApproverName = approver.Name,
                                        BUname = item.BUName,
                                        RequestorUserId = request.RequestorUserId,
                                        RequestStatus = request.RequestStatus,
                                        RequestorName = user.Name,
                                        ItemNumber = item.ItemNumber,
                                        ItemDescription = item.Description,
                                        UnitOfMeasure = item.UnitOfMeasure,
                                        QuantityRequested = request.QuantityRequested,
                                        //UnitPrice = request.UnitPrice,
                                        Purpose = request.Purpose,
                                        DateRequested = request.DateRequested,
                                        DateApproved = request.DateApproved,
                                    };

                        var requestList = await queryRequest.ToListAsync();

                        return View(requestList);
                    }

                    else if (requestType.ToLower() == "buyer") //FOR APPROVER
                    {

                        //Query the request pull out item to be displayed
                        var queryRequest = from request in _context.SSv3_ITEM_REQUESTTBL
                                           where request.isActive == 1 && request.RequestorUserId == userQuery.Id && request.Id == pulloutRequestId
                                           join inventory in _context.SSv3_GAL_INVENTORYTBL on request.InventoryItemId equals inventory.IncommingItemId
                                           join item in _context.SSv3_GAL_ITEMTBL on inventory.IncommingItemId equals item.Id
                                           where item.isActive == 1
                                           join supplier in _context.SSv3_GAL_SUPPLIERTBL on item.SupplierId equals supplier.Id
                                           where supplier.isActive == 1
                                           join user in _context.SSv3_GAL_USERTBL on request.RequestorUserId equals user.Id
                                           where user.isActive == 1
                                           join approver in _context.SSv3_GAL_USERTBL on request.ApproverUserId equals approver.Id
                                           orderby request.DateRequested descending
                                           where approver.isActive == 1
                                           select new Requests
                                           {
                                               Id = request.Id,
                                               //RequestorUserId = request.RequestorUserId,
                                               AreaName = request.AreaName,
                                               OrderNumber = request.OrderNumber,
                                               ApproverName = approver.Name,
                                               BUname = item.BUName,
                                               RequestorUserId = request.RequestorUserId,
                                               RequestStatus = request.RequestStatus,
                                               RequestorName = user.Name,
                                               ItemNumber = item.ItemNumber,
                                               ItemDescription = item.Description,
                                               UnitOfMeasure = item.UnitOfMeasure,
                                               QuantityRequested = request.QuantityRequested,
                                               //UnitPrice = request.UnitPrice,
                                               Purpose = request.Purpose,
                                               DateRequested = request.DateRequested,
                                               DateApproved = request.DateApproved,
                                           };

                        var requestList = await queryRequest.ToListAsync();

                        return View(requestList);
                    }
                    else
                    {
                        return RedirectToAction("OrderDetailError", "Errors");
                    }
                }

                //FOR USER
                //Get the latest request of current user from approvals table
                var approvalQuery = await(from approvals in _context.SSv3_GAL_APPROVALSTBL where approvals.RequestorUsername.ToLower() == currentUser.ToLower() orderby approvals.RequestedDate descending select approvals).FirstOrDefaultAsync();

                //Get all the requested item from approval table order numbers column
                string[] requests = approvalQuery.OrderNumbers.Split(',');
                int[] requestIds = Array.ConvertAll(requests, int.Parse);

                //Query each  item to be displayed
                var currentRequest = from request in _context.SSv3_ITEM_REQUESTTBL
                            where request.isActive == 1 && request.RequestorUserId == userQuery.Id && requestIds.Contains(request.Id)
                            join inventory in _context.SSv3_GAL_INVENTORYTBL on request.InventoryItemId equals inventory.IncommingItemId
                            join item in _context.SSv3_GAL_ITEMTBL on inventory.IncommingItemId equals item.Id
                            where item.isActive == 1
                            join supplier in _context.SSv3_GAL_SUPPLIERTBL on item.SupplierId equals supplier.Id
                            where supplier.isActive == 1
                            join user in _context.SSv3_GAL_USERTBL on request.RequestorUserId equals user.Id
                            where user.isActive == 1
                            join approver in _context.SSv3_GAL_USERTBL on request.ApproverUserId equals approver.Id
                            orderby request.DateRequested descending
                            where approver.isActive == 1
                            select new Requests
                            {
                                Id = request.Id,
                                //RequestorUserId = request.RequestorUserId,
                                AreaName = request.AreaName,
                                OrderNumber = request.OrderNumber,
                                ApproverName = approver.Name,
                                BUname = item.BUName,
                                RequestorUserId = request.RequestorUserId,
                                RequestStatus = request.RequestStatus,
                                RequestorName = user.Name,
                                ItemNumber = item.ItemNumber,
                                ItemDescription = item.Description,
                                UnitOfMeasure = item.UnitOfMeasure,
                                QuantityRequested = request.QuantityRequested,
                                //UnitPrice = request.UnitPrice,
                                Purpose = request.Purpose,
                                DateRequested = request.DateRequested,
                                DateApproved = request.DateApproved,
                            };

                var currentRequestList = await currentRequest.ToListAsync();

                return View(currentRequestList);
            }
            catch
            {
                return RedirectToAction("OrderDetailError", "Errors");
            }

        }

    }
}
