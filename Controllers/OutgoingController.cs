using Microsoft.AspNetCore.Mvc;
using ShopSuppliesV3.Data;
using ShopSuppliesV3.ViewModel;
using ShopSuppliesV3.Models;
using Microsoft.EntityFrameworkCore;
using ShopSuppliesV3.Helpers;
using System.Data;
using System.Text;


namespace ShopSuppliesV3.Controllers
{
    public class OutgoingController : Controller
    {
        private readonly ShopSuppliesV3Context _context;

        public OutgoingController(ShopSuppliesV3Context context)
        {
            _context = context;
        }

        public async Task<ActionResult<DataSet>> Index()
        {
            //ADMIN ACCESS ONLY
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser && user.AccessType.ToLower() == "admin" select user).FirstOrDefaultAsync();

            //IF USER IS NOT IN THE USER TABLE OR NOT ADMIN
            if (userQuery == null)
            {
                return RedirectToAction("Error404", "Errors");
            }

            var outgoingQuery = from outgoing in _context.SSv3_GAL_OUTGOINGTBL
                        where outgoing.isActive == 1
                        join request in _context.SSv3_ITEM_REQUESTTBL on outgoing.RequestId equals request.Id
                        where request.isActive == 1 && (request.RequestStatus.ToLower() == "issued" || request.RequestStatus.ToLower() == "partial")
                        join item in _context.SSv3_GAL_ITEMTBL on request.InventoryItemId equals item.Id
                        where item.isActive == 1
                        join requestor in _context.SSv3_GAL_USERTBL on request.RequestorUserId equals requestor.Id
                        where requestor.isActive == 1
                        join releaser in _context.SSv3_GAL_USERTBL on outgoing.IssuerUsername equals releaser.UserName where releaser.isActive == 1
                        join approver in _context.SSv3_GAL_USERTBL on request.ApproverUserId equals approver.Id where approver.isApprover == true && approver.isActive == 1
                        join incoming in _context.SSv3_GAL_INCOMMINGTBL on outgoing.IncommingIds equals incoming.Id where incoming.isActive == 1
                        select new Outgoing
                        {
                            OrderNumber = request.OrderNumber,
                            RequestCategory = request.RequestCategory,
                            RequestedByName = requestor.Name,
                            AreaName = request.AreaName,
                            DateRequested = request.DateRequested,                           
                            ApproverName = approver.Name,
                            ItemNumber = item.ItemNumber,
                            Description = item.Description,
                            QuantityRequested = request.QuantityRequested,
                            UnitOfMeasure = item.UnitOfMeasure,
                            RequestStatus = request.RequestStatus,
                            QuantityIssued = outgoing.QuantityIssued,
                            IssuedByName = releaser.Name,
                            DateIssued = outgoing.DateIssued,
                            //Purpose = request.Purpose,
                            OutgoingRemarks = outgoing.Remarks,
                            IncomingPOnumber = incoming.PONumber,
                            UnitCost = outgoing.UnitCost,
                            TotalCost = outgoing.TotalCost                           
                        };

            var outgoingList = await outgoingQuery.ToListAsync();


            var pulloutQuery = from request in _context.SSv3_ITEM_REQUESTTBL
                               where request.isActive == 1 && request.RequestCategory.ToLower() == "pullout" && request.RequestStatus.ToLower() == "approved"
                               join item in _context.SSv3_GAL_ITEMTBL on request.InventoryItemId equals item.Id
                               where item.isActive == 1
                               join requestor in _context.SSv3_GAL_USERTBL on request.RequestorUserId equals requestor.Id //the requestor is the store admin
                               where requestor.isActive == 1
                               join approver in _context.SSv3_GAL_USERTBL on request.ApproverUserId equals approver.Id //the approver is the buyer
                               where approver.AccessType.ToLower() == "buyer" && approver.isActive == 1
                               join history in _context.SSv3_GAL_INCOMING_HISTORYTBL on request.OrderNumber equals history.OrderNumber
                               join incoming in _context.SSv3_GAL_INCOMMINGTBL on history.IncomingId equals incoming.Id where incoming.isActive == 1
                               select new Outgoing
                               {
                                   OrderNumber = request.OrderNumber,
                                   RequestCategory = request.RequestCategory,
                                   RequestedByName = requestor.Name,
                                   AreaName = request.AreaName,
                                   DateRequested = request.DateRequested,
                                   ApproverName = approver.Name,
                                   ItemNumber = item.ItemNumber,
                                   Description = item.Description,
                                   QuantityRequested = request.QuantityRequested,
                                   UnitOfMeasure = item.UnitOfMeasure,
                                   RequestStatus = request.RequestStatus,
                                   Purpose = request.Purpose,
                                   OutgoingRemarks = history.Reason,
                                   IncomingPOnumber = incoming.PONumber,
                                   UnitCost = history.UnitPrice,
                                   TotalCost = history.TotalPrice
                               };

            var pulloutList = await pulloutQuery.ToListAsync();

            ViewData["PullOutList"] = pulloutList;

            return View(outgoingList);
        }

        public async Task<IActionResult> IssueRequestedItem(int requestId)
        {
            //ADMIN ACCESS ONLY
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser && user.AccessType.ToLower() == "admin" select user).FirstOrDefaultAsync();

            //IF USER IS NOT IN THE USER TABLE OR NOT ADMIN
            if (userQuery == null)
            {
                return RedirectToAction("Error404", "Errors");
            }

            var query = from request in _context.SSv3_ITEM_REQUESTTBL
                        where request.isActive == 1 && request.Id == requestId
                        join inventory in _context.SSv3_GAL_INVENTORYTBL on request.InventoryItemId equals inventory.IncommingItemId
                        join item in _context.SSv3_GAL_ITEMTBL on inventory.IncommingItemId equals item.Id
                        where item.isActive == 1
                        join incommings in _context.SSv3_GAL_INCOMMINGTBL on item.Id equals incommings.ItemId where incommings.isActive == 1 && (incommings.Status.ToLower() != "expired" || incommings.Status.ToLower() != "consumed")
                        join supplier in _context.SSv3_GAL_SUPPLIERTBL on item.SupplierId equals supplier.Id
                        where supplier.isActive == 1
                        join user in _context.SSv3_GAL_USERTBL on request.RequestorUserId equals user.Id
                        where user.isActive == 1
                        select new Requests
                        {
                            Id = request.Id,
                            
                            ItemNumber = item.ItemNumber,
                            IncommingId = incommings.Id,
                            PONumber = incommings.PONumber,
                            AreaName = request.AreaName,
                            BUname = item.BUName,
                            RequestStatus = request.RequestStatus,
                            RequestorUserId = user.Id,
                            RequestorName = user.Name,
                            ItemId = item.Id,
                            //ItemName = item.ItemName,
                            ItemDescription = item.Description,
                            UnitOfMeasure = item.UnitOfMeasure,
                            QuantityRequested = request.QuantityRequested,
                            //UnitPrice = incommings.UnitPrice,
                            //TotalPrice = incommings.UnitPrice * request.QuantityRequested,
                            Purpose = request.Purpose,
                            DateRequested = request.DateRequested
                        };

            var requestInfo = await query.FirstOrDefaultAsync();

            var items = from incommings in _context.SSv3_GAL_INCOMMINGTBL where incommings.ItemId == requestInfo.ItemId && (incommings.Status.ToLower() != "consumed" || incommings.Status.ToLower() != "expired") && incommings.IncommingQuantity != incommings.TotalOutgoingQuantity select incommings;

            ViewData["InventoryItemList"] = await items.ToListAsync();

            return View(requestInfo);
        }

        [HttpPost]
        public async Task<IActionResult> IssueRequestedItem(IFormCollection formData)
        {
            //ADMIN ACCESS ONLY
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser && user.AccessType.ToLower() == "admin" select user).FirstOrDefaultAsync();

            //IF USER IS NOT IN THE USER TABLE OR NOT ADMIN
            if (userQuery == null)
            {
                return RedirectToAction("Error404", "Errors");
            }

            int requestId = int.Parse(formData["Id"].ToString());
            int CartId = int.Parse(formData["RequestorUserId"].ToString()); //CartId is the same as the userId
            //decimal unitCost = 0;
            //decimal totalCost = 0;
            //decimal.TryParse(formData["UnitCost"].ToString(), out unitCost);
            //decimal.TryParse(formData["TotalCost"].ToString(), out totalCost);

            ////ERROR FOR COST
            //if (unitCost == 0 || totalCost == 0)
            //{
            //    TempData["CostingError"] = unitCost.ToString() + "and" + totalCost.ToString() + "error";
            //    return View(requestId);
            //}

            try
            {
                
                decimal quantityRequested = decimal.Parse(formData["QuantityRequested"].ToString());

                int itemId = int.Parse(formData["ItemId"].ToString());

                var query = from incomming in _context.SSv3_GAL_INCOMMINGTBL
                            where incomming.ItemId == itemId && incomming.isActive == 1 && (incomming.Status.ToLower() != "expired" || incomming.Status.ToLower() != "consumed")
                            select incomming.Id;

                List<int> incommingIds = await query.ToListAsync();

                //SUBTRACT THE QUANTITY REQUESTED IN THE INCOMMINGS TABLE
                await _context.Database.BeginTransactionAsync();
                decimal totalOutgoingQuantityRequested = 0;
                //string incommingIdWidrawals = "";
                foreach (int id in incommingIds)
                {
                    decimal.TryParse(formData[$"{id}"].ToString(), out decimal outgoingQuantity ); //id is used since in the view, each input element has an id based on the incomming Id(used in javascript) and the value inputted to it is the quantity to be issued.
                    decimal.TryParse(formData[$"UnitCost-{id}"].ToString(), out decimal unitCost); //id is used since in the view, each input element has an id based on the incomming Id(used in javascript) and the value inputted to it is the quantity to be issued.
                    decimal.TryParse(formData[$"TotalCost-{id}"].ToString(), out decimal totalCost); //id is used since in the view, each input element has an id based on the incomming Id(used in javascript) and the value inputted to it is the quantity to be issued.
                    string remarks = formData[$"Remarks-{id}"].ToString();
                    if (outgoingQuantity > 0)
                    {
                        //incommingIdWidrawals += (id + ",");
                        var incommings = await _context.SSv3_GAL_INCOMMINGTBL.FindAsync(id);
                        incommings.TotalOutgoingQuantity += outgoingQuantity;
                        if(incommings.IncommingQuantity == incommings.TotalOutgoingQuantity)
                        {
                            incommings.Status = "Consumed";
                        }

                        _context.Update(incommings);
                        await _context.SaveChangesAsync();

                        totalOutgoingQuantityRequested += outgoingQuantity;

                        //CREATE THE OUTGOING
                        SSv3_GAL_OUTGOINGTBL newOutgoing = new SSv3_GAL_OUTGOINGTBL
                        {
                            isActive = 1,
                            RequestId = requestId,
                            QuantityIssued = outgoingQuantity,
                            Remarks = remarks,
                            UnitCost = unitCost,
                            TotalCost = totalCost,
                            IncommingIds = id,
                            DateIssued = DateTime.Now,
                            IssuerUsername = User.Identity.Name.Substring(5)

                        };
                        _context.Add(newOutgoing);
                        await _context.SaveChangesAsync();
                    };
                };

                //CONDITION IF THE ISSUED QUANTITY IS EQUIVALENT TO THE REQUESTED QUANTITY OR LESS THAN THE REQUESTED QUANTITY
                if(totalOutgoingQuantityRequested < quantityRequested) //If the issued quantity is less than the requested quantity
                {
                    //CHANGE THE REQUEST STATUS TO PARTIAL (partially issued)
                    var updateRequest = await _context.SSv3_ITEM_REQUESTTBL.FindAsync(requestId);
                    updateRequest.RequestStatus = "Partial";
                    updateRequest.DateApproved = DateTime.Now;
                    _context.Update(updateRequest);

                    //UPDATE THE INVENTORY
                    int inventoryId = (from inventory in _context.SSv3_GAL_INVENTORYTBL where inventory.IncommingItemId == itemId select inventory.Id).FirstOrDefault();
                    var inventoryUpdate = await _context.SSv3_GAL_INVENTORYTBL.FindAsync(inventoryId);
                    inventoryUpdate.OutgoingTotalQuantity += totalOutgoingQuantityRequested;
                    inventoryUpdate.InRequestTotalQuantity -= quantityRequested;
                    //inventoryUpdate.RemainingStocksToRequest = inventoryUpdate.IncommingTotalQuantity - (inventoryUpdate.OutgoingTotalQuantity + inventoryUpdate.InRequestTotalQuantity);
                    //inventoryUpdate.RemainingStocksOnhand = inventoryUpdate.IncommingTotalQuantity - inventoryUpdate.OutgoingTotalQuantity;
                    _context.Update(inventoryUpdate);
                    await _context.SaveChangesAsync();

                    //ADD EMAIL SENDING THAT THE ITEM ISSUED IS LESS THAN REQUESTED AND TO REQUEST AGAIN
                }
                else
                {
                    //CHANGE THE REQUEST STATUS TO ISSUED
                    var updateRequest = await _context.SSv3_ITEM_REQUESTTBL.FindAsync(requestId);
                    updateRequest.RequestStatus = "Issued";
                    updateRequest.DateApproved = DateTime.Now;
                    _context.Update(updateRequest);

                    //UPDATE THE INVENTORY
                    int inventoryId = (from inventory in _context.SSv3_GAL_INVENTORYTBL where inventory.IncommingItemId == itemId select inventory.Id).FirstOrDefault();
                    var inventoryUpdate = await _context.SSv3_GAL_INVENTORYTBL.FindAsync(inventoryId);
                    inventoryUpdate.OutgoingTotalQuantity += totalOutgoingQuantityRequested;
                    inventoryUpdate.InRequestTotalQuantity -= quantityRequested;
                    //inventoryUpdate.RemainingStocksToRequest = inventoryUpdate.IncommingTotalQuantity - (inventoryUpdate.OutgoingTotalQuantity + inventoryUpdate.InRequestTotalQuantity);
                    //inventoryUpdate.RemainingStocksOnhand = inventoryUpdate.IncommingTotalQuantity - inventoryUpdate.OutgoingTotalQuantity;
                    _context.Update(inventoryUpdate);
                    await _context.SaveChangesAsync();

                    //ADD EMAIL SENDING THAT THE ITEM ISSUED IS EQUAL TO THE ITEM REQUESTED


                }

                //IF EVERYTHING IS OK COMMIT TO THE DATABASE


                //CHECK IF THERE IS REMAINING ITEM IN THE REQUEST
                var updatedRequest = await (from request in _context.SSv3_ITEM_REQUESTTBL where request.isActive == 1 && request.RequestStatus.ToLower() == "approved" && request.RequestorUserId == CartId select request).FirstOrDefaultAsync();

                if (updatedRequest == null) //NO REMAINING ITEM IN THE REQUEST
                {
                    await _context.Database.CommitTransactionAsync();

                    TempData["ItemIssuedSuccess"] = CartId;
                    return RedirectToAction("Index", "Issuance");
                }
                else
                {
                    if (updatedRequest.Id > 0)
                    {
                        await _context.Database.CommitTransactionAsync();

                        TempData["ItemIssuedSuccess"] = updatedRequest.OrderNumber;
                        return RedirectToAction("IssuanceRequestDetails", "Issuance", new { cartId = CartId });
                    }
                }

                await _context.Database.CommitTransactionAsync();

                TempData["ItemIssuedSuccess"] = CartId;
                return RedirectToAction("Index", "Issuance");

            }
            catch
            {
                await _context.Database.RollbackTransactionAsync();
                TempData["RequestIssuanceError"] = CartId;
                return RedirectToAction("RequestIssuanceError", "Errors");
            }

        }
     }
}