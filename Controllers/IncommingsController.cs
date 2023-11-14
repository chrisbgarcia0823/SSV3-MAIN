using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSuppliesV3.Data;
using ShopSuppliesV3.Models;
using ShopSuppliesV3.ViewModel;
using System.Globalization;
using System.Xml.Linq;
using System.Transactions;
using ShopSuppliesV3.Helpers;
using System.Data;
using System.Security.Cryptography.X509Certificates;

namespace ShopSuppliesV3.Controllers
{
    public class IncommingsController : Controller
    {
        private readonly ShopSuppliesV3Context _context;

        public IncommingsController(ShopSuppliesV3Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {

            //ADMIN ACCESS ONLY
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser && user.AccessType.ToLower() == "admin" select user).FirstOrDefaultAsync();

            //IF USER IS NOT IN THE USER TABLE OR NOT ADMIN
            if (userQuery == null)
            {
                return RedirectToAction("Error404", "Errors");
            }

            //FOR APPROVER DROPDOWN IN SELECT MENU
            var buyers = from b in _context.SSv3_GAL_USERTBL where b.AccessType.ToLower() == "buyer" select b;
            ViewData["Buyers"] = await buyers.ToListAsync();

            var query = from i in _context.SSv3_GAL_INCOMMINGTBL
                        where i.isActive == 1 && i.Status.ToLower() == "new" || i.Status.ToLower() == "partial hold" || i.Status.ToLower() == "partial reworked good" || i.Status.ToLower() == "reworked good"
                        join user in _context.SSv3_GAL_USERTBL on i.addedBy equals user.UserName
                        join item in _context.SSv3_GAL_ITEMTBL on i.ItemId equals item.Id
                        where item.isActive == 1
                        join supplier in _context.SSv3_GAL_SUPPLIERTBL on item.SupplierId equals supplier.Id
                        where supplier.isActive == 1
                        select new Incommings
                        {
                            Id = i.Id,
                            //ItemId = item.Id,
                            ItemNumber = item.ItemNumber,
                            BUName = item.BUName,                         
                            Description = item.Description,
                            SupplierName = supplier.SupplierName,
                            Quantity = i.IncommingQuantity,
                            TotalOnHoldQuantity = i.TotalOnHoldQuantity,
                            TotalOutgoingQuantity = i.TotalOutgoingQuantity,
                            UnitPrice = i.UnitPrice,
                            TotalPrice = i.TotalPrice,
                            addedBy = user.Name,
                            Status = i.Status,
                            ExpirationDate = i.ExpirationDate,
                            DateAdded = i.DateAdded
                        };
            var incommingList = await query.ToListAsync();

            return View(incommingList);
        }

        public async Task<IActionResult> Create()
        {
            //ADMIN ACCESS ONLY
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser && user.AccessType.ToLower() == "admin" select user).FirstOrDefaultAsync();

            //IF USER IS NOT IN THE USER TABLE OR NOT ADMIN
            if (userQuery == null)
            {
                return RedirectToAction("Error404", "Errors");
            }

            List<string> bu = (from b in _context.SSv3_GAL_BUTBL where b.isActive == 1 select b.BUname).ToList();
            ViewData["BU"] = bu;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(IFormCollection formData, SSv3_GAL_INCOMMINGTBL incomingModel)
        {

            //ADMIN ACCESS ONLY
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser && user.AccessType.ToLower() == "admin" select user).FirstOrDefaultAsync();

            //IF USER IS NOT IN THE USER TABLE OR NOT ADMIN
            if (userQuery == null)
            {
                return RedirectToAction("Error404", "Errors");
            }

            //int itemId = int.Parse(formData["ItemId"].ToString());
            //decimal incommingQuantity = decimal.Parse(formData["IncommingQuantity"].ToString());

            try
            {
                _context.Database.BeginTransaction();
                DateTime? expirationDate;

                if (string.IsNullOrEmpty(formData["ExpirationDate"].ToString()))
                {
                    expirationDate = null;
                }
                else
                {
                    expirationDate = DateTime.ParseExact(formData["ExpirationDate"].ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                }

                //Create new incomming item
                SSv3_GAL_INCOMMINGTBL newIncomming = new SSv3_GAL_INCOMMINGTBL
                {
                    isActive = 1,
                    ItemId = incomingModel.ItemId,
                    IncommingQuantity = incomingModel.IncommingQuantity,
                    UnitPrice = incomingModel.UnitPrice,
                    TotalOutgoingQuantity = 0,
                    TotalOnHoldQuantity = 0,
                    PONumber = incomingModel.PONumber,
                    addedBy = User.Identity.Name.Substring(5),
                    Status = "New",
                    ExpirationDate = incomingModel.ExpirationDate,
                    DateAdded = DateTime.Now
                };

                _context.Add(newIncomming);
                await _context.SaveChangesAsync();

                //Check if the added incomming item is in the Inventory
                var InventoryId = (from inventory in _context.SSv3_GAL_INVENTORYTBL where inventory.IncommingItemId == incomingModel.ItemId select inventory.Id).FirstOrDefault();
                var query = from item in _context.SSv3_GAL_INCOMMINGTBL where item.ItemId == incomingModel.ItemId && (item.Status.ToLower() != "consumed" || item.Status.ToLower() != "expired")  select item.IncommingQuantity;
                var totalQuantity = query.Sum();

                if(InventoryId > 0) //Update the quantity of the item in the Inventory table corresponding to the item Id if the item is already exist in the Inventory table
                {
                    var InventoryItem = _context.SSv3_GAL_INVENTORYTBL.Find(InventoryId); //get the row in the Inventory table that contains the Id from the query(InventoryId)
                    InventoryItem.IncommingTotalQuantity = totalQuantity;
                    //InventoryItem.RemainingStocksToRequest = InventoryItem.IncommingTotalQuantity - (InventoryItem.OutgoingTotalQuantity + InventoryItem.InRequestTotalQuantity);
                    //InventoryItem.RemainingStocksOnhand = InventoryItem.IncommingTotalQuantity - InventoryItem.OutgoingTotalQuantity;
                    _context.Update(InventoryItem);
                    await _context.SaveChangesAsync();
                }
                else //Item not yet in inventory (add the item in the inventory table)
                {
                    //var safetyStocks = (from i in _context.SSv3_GAL_ITEMTBL where i.isActive == 1 && i.Id == incomingModel.ItemId select i.SafetyStock).First();

                    SSv3_GAL_INVENTORYTBL newItem = new SSv3_GAL_INVENTORYTBL
                    {
                        IncommingItemId = incomingModel.ItemId,
                        IncommingTotalQuantity = incomingModel.IncommingQuantity,
                        //RemainingStocksToRequest = incommingQuantity,
                        //RemainingStocksOnhand = incommingQuantity,
                        OutgoingTotalQuantity = 0
                    };
                    _context.Add(newItem);
                    await _context.SaveChangesAsync();
                }

                TempData["IncommingItemSuccess"] = "Success";

                _context.Database.CommitTransaction();
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                _context.Database.RollbackTransaction();
                TempData["ItemId"] = formData["ItemId"].ToString();
                TempData["Error"] = ex.ToString();
                return RedirectToAction("IncommingItemAddingError", "Errors");
            }
        }

        //FOR THE SEARCH/FILTERED VIEW
        public async Task<IActionResult> EditIncoming(string itemNumber)
        {
            //ADMIN ACCESS ONLY
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser && user.AccessType.ToLower() == "admin" select user).FirstOrDefaultAsync();

            //IF USER IS NOT IN THE USER TABLE OR NOT ADMIN
            if (userQuery == null)
            {
                return RedirectToAction("Error404", "Errors");
            }

            try
            {
                if (string.IsNullOrEmpty(itemNumber))
                {
                    TempData["InitialSearch"] = "not null"; //Used in the view to distinguised weather the search has a parameter or not
                    return View();
                }
                else
                {
                    var query = from i in _context.SSv3_GAL_INCOMMINGTBL
                                where i.isActive == 1 && i.Status.ToLower() == "new" || i.Status.ToLower() == "partial hold" || i.Status.ToLower() == "partial reworked good" || i.Status.ToLower() == "reworked good"
                                join user in _context.SSv3_GAL_USERTBL on i.addedBy equals user.UserName
                                join item in _context.SSv3_GAL_ITEMTBL on i.ItemId equals item.Id
                                where item.isActive == 1 && item.ItemNumber == itemNumber
                                join supplier in _context.SSv3_GAL_SUPPLIERTBL on item.SupplierId equals supplier.Id
                                where supplier.isActive == 1
                                select new Incommings
                                {
                                    Id = i.Id,
                                    ItemNumber = item.ItemNumber,
                                    BUName = item.BUName,
                                    Description = item.Description,
                                    SupplierName = supplier.SupplierName,
                                    Quantity = i.IncommingQuantity,
                                    TotalOnHoldQuantity = i.TotalOnHoldQuantity,
                                    TotalOutgoingQuantity = i.TotalOutgoingQuantity,
                                    UnitPrice = i.UnitPrice,
                                    TotalPrice = i.TotalPrice,
                                    addedBy = user.Name,
                                    Status = i.Status,
                                    ExpirationDate = i.ExpirationDate,
                                    DateAdded = i.DateAdded
                                };
                    var incommingList = await query.ToListAsync();

                    TempData["FromEditItemSearch"] = itemNumber;
                    return View(incommingList);
                }
            }
            catch
            {
                return RedirectToAction("IncomingSearchError", "Errors");
            }

        }

        //EDIT THE STATUS FOR REWORK ITEMS (ON HOLD) IN INDEX AND EDITINCOMING VIEW
        [HttpPost]
        public async Task<IActionResult> EditIncoming(IFormCollection formData)
        {
            //ADMIN ACCESS ONLY
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser && user.AccessType.ToLower() == "admin" select user).FirstOrDefaultAsync();

            //IF USER IS NOT IN THE USER TABLE OR NOT ADMIN
            if (userQuery == null)
            {
                return RedirectToAction("Error404", "Errors");
            }

            try
            {
                int incomingId = int.Parse(formData["IncomingId"].ToString());
                //int incomingItemId = int.Parse(formData["IncomingItemId"].ToString()); //the item id
                decimal incomingQty = decimal.Parse(formData["IncomingQty"].ToString()); //the incoming quantity
                decimal outgoingQty = decimal.Parse(formData["OutgoingQty"].ToString()); //the totalOutgoing quantity
                string action = formData["Action"].ToString();
                decimal quantity = decimal.Parse(formData["Quantity"].ToString()); //The pull-in or pull-out quantity (should be the onhold quantity in incoming table)
                decimal unitPrice = decimal.Parse(formData["UnitPrice"].ToString());
                string reason = formData["Reason"].ToString();


                _context.Database.BeginTransaction();

                //UPDATE the status and TotalOnHoldQuantity of the Incoming Item based on the action(pull-in or pullout) of incoming history
                //CHECK IF INCOMING ID IS VALID
                var incomingItem = await _context.SSv3_GAL_INCOMMINGTBL.FindAsync(incomingId);
                if (incomingItem == null) //Check if incoming Id is valid
                {
                    TempData["IncomingId"] = incomingId;
                    return RedirectToAction("IncomingItemNotExist", "Errors");
                }

                decimal remainingQty = incomingQty - (outgoingQty + quantity); //check if the remaining quantity (if zero(0), status will be full hold, if more than zero(0) status partial hold)

                //FOR PULLOUT ITEMS
                if(action.ToLower() == "pullout")
                {
                    //Create a new incoming history table for pull-in or pull-out items
                    SSv3_GAL_INCOMING_HISTORYTBL newIncomingHistory = new SSv3_GAL_INCOMING_HISTORYTBL
                    {
                        IncomingId = incomingId,
                        Action = action, //Either pull-in or pull-out (pull-in if the item will be put to stock and pull-out if the item will get out of stock)
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        Reason = reason,
                        DateAdded = DateTime.Now,
                        AddedByUsername = currentUser,
                        Status = "For Approval"
                    };

                    _context.SSv3_GAL_INCOMING_HISTORYTBL.Add(newIncomingHistory);
                    _context.SaveChanges();

                    //Get the Id of the newly created row of incoming history to be used as ordernumber in the request table
                    int incomingHistoryId = await (from history in _context.SSv3_GAL_INCOMING_HISTORYTBL orderby history.Id descending select history.Id).FirstOrDefaultAsync();
                    string incomingHistoryIdFormatted = incomingHistoryId.ToString("0000000");
                    string orderNumber = $"Pout-{incomingHistoryIdFormatted}"; //ORDER NUMBERS FOR PULLOUT ITEM WILL START AT Pout

                    var historyOrderNumber =  await _context.SSv3_GAL_INCOMING_HISTORYTBL.FindAsync(incomingHistoryId);
                    historyOrderNumber.OrderNumber = orderNumber;


                    //UPDATE THE INVENTORY TABLE
                    int incomingItemId = incomingItem.ItemId;
                    var inventoryItem = await (from inventory in _context.SSv3_GAL_INVENTORYTBL where inventory.IncommingItemId == incomingItemId select inventory).FirstOrDefaultAsync();
                    //check if Item exist in inventory table
                    if (inventoryItem == null)
                    {
                        return RedirectToAction("IncommingEditError", "Errors");
                    }

                    inventoryItem.TotalOnHoldQuantity += quantity;
                    _context.SaveChanges();

                    //CREATE A REQUEST FOR THE PULLOUT ITEM
                    string buName = formData["BUname"].ToString();
                    int buyerUserId = int.Parse(formData["Buyer"].ToString()); //Buyer is the approver for pull-out of items
                    SSv3_ITEM_REQUESTTBL newRequest = new SSv3_ITEM_REQUESTTBL
                    {
                        RequestCategory = "Pullout",
                        AreaName = "Store",
                        isActive = 1,
                        BUname = buName,
                        RequestStatus = "For Approval",
                        RequestorUserId = userQuery.Id,
                        InventoryItemId = inventoryItem.Id,
                        QuantityRequested = quantity,
                        Purpose = reason,
                        DateRequested = DateTime.Now,
                        ApproverUserId = buyerUserId,
                        OrderNumber = orderNumber //This will serve as the history Id in the incoming history table. (refer above for the format)
                };

                   _context.Add(newRequest);
                   _context.SaveChanges();

                    //SEND EMAIL TO BUYER FOR APPROVAL OF THE PULL-OUT ITEM
                    var buyerDetail = await (from buyer in _context.SSv3_GAL_USERTBL where buyer.Id == buyerUserId && buyer.isActive == 1 select buyer).FirstOrDefaultAsync();
                    string buyerUsername = buyerDetail.UserName;
                    string buyerEmail = buyerDetail.EmailAdd;
                    string requestorName = await (from user in _context.SSv3_GAL_USERTBL where user.UserName == currentUser select user.Name).FirstOrDefaultAsync();
                    string itemNumber = await (from item in _context.SSv3_GAL_ITEMTBL where item.isActive == 1 && item.Id == incomingItemId select item.ItemNumber).FirstOrDefaultAsync();
                    int requestId = await (from request in _context.SSv3_ITEM_REQUESTTBL where request.isActive == 1 orderby request.Id descending select request.Id).FirstOrDefaultAsync();

                    string emailBody = EmailSendingHelpers.GetEmaiBodyPullout(requestId, buyerUsername, requestorName, itemNumber, reason, quantity, incomingId);
                    string emailSubject = "Shop Supplies V3 Notification - Item Pullout Request - FOR APPROVAL ";
                    EmailSendingHelpers.SendEmail(buyerEmail, emailBody, emailSubject);

                    //CHECKING FOR PULL OUT QUANTITY
                    if (remainingQty == 0) //All quantity or remaining quantity is pull-out (put out of the stock)
                    {
                        incomingItem.Status = "Full Hold";
                        incomingItem.TotalOnHoldQuantity += quantity;
                        _context.SaveChanges();

                    }
                    else if(remainingQty > 0) //Some quantity or remaining quantity is pull-out (put out of the stock)
                    {
                        incomingItem.Status = "Partial Hold";
                        incomingItem.TotalOnHoldQuantity += quantity;
                        _context.SaveChanges();
                    }
                    else
                    {
                        _context.Database.RollbackTransaction();
                        return RedirectToAction("IncommingEditError", "Errors");
                    }
                }
                else if(action.ToLower() == "pullin")
                {
                    //Create a new incoming history table for pull-in or pull-out items
                    SSv3_GAL_INCOMING_HISTORYTBL newIncomingHistory = new SSv3_GAL_INCOMING_HISTORYTBL
                    {
                        IncomingId = incomingId,
                        Action = action, //Either pull-in or pull-out (pull-in if the item will be put to stock and pull-out if the item will get out of stock)
                        Quantity = quantity,
                        UnitPrice = unitPrice,
                        Reason = reason,
                        DateAdded = DateTime.Now,
                        AddedByUsername = currentUser
                    };

                    //UPDATE THE INVENTORY TABLE
                    int incomingItemId = incomingItem.ItemId;
                    var inventoryItem = await (from inventory in _context.SSv3_GAL_INVENTORYTBL where inventory.IncommingItemId == incomingItemId select inventory).FirstOrDefaultAsync();
                    //check if Item exist in inventory table
                    if (inventoryItem == null)
                    {
                        return RedirectToAction("IncommingEditError", "Errors");
                    }

                    inventoryItem.TotalOnHoldQuantity -= quantity;
                    _context.SaveChanges();

                    if (remainingQty == 0) //All quantity or remaining quantity is pull-in (put back in stock)
                    {
                        incomingItem.Status = "Reworked Good";
                        incomingItem.TotalOnHoldQuantity -= quantity;
                        _context.SaveChanges();
                    }
                    else if (remainingQty > 0) //Some quantity or remaining quantity is pull-in (put back in stock)
                    {
                        incomingItem.Status = "Partial Reworked Good";
                        incomingItem.TotalOnHoldQuantity -= quantity;
                        _context.SaveChanges();
                    }
                    else
                    {
                        _context.Database.RollbackTransaction();
                        return RedirectToAction("IncommingEditError", "Errors");
                    }
                }
                else
                {
                    _context.Database.RollbackTransaction();
                    return RedirectToAction("IncommingEditError", "Errors");
                }


                _context.Database.CommitTransaction();
                TempData["EditIncomingSuccess"] = incomingId; //Change When Incomming Id is added

                //Edit request comes from EditIncoming ActionMethod and not from Index
                if (!string.IsNullOrEmpty(formData["FromEditIncoming"].ToString())) //FromEditIncoming is from MODAL EDITING input (Distinguish weather the request is from index or the EditIncoming Action method
                {
                    return RedirectToAction("EditIncoming" , new {itemNumber = formData["FromEditIncoming"].ToString()}); //Send back the itemNumber to the EditIncoming Action Method
                }

                //Edit request comes from Index Action method
                return RedirectToAction("Index");
            }
            catch
            {
                _context.Database.RollbackTransaction();
                return RedirectToAction("IncommingEditError", "Errors");
            }
        }




        //FOR GETTING THE INDIVIDUAL DATA TO BE DELETED USING FETCH API IN THE INDEX DELETE MODAL
        [Route("[controller]/[action]/{incomingId}")] //Route is added (to match the fetch @url action)
        public async Task<string> DeleteIncoming(int incomingId)
        {
            //FOR ADMIN ACCESS ONLY
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

            //If USER IS NOT IN THE USERTABLE ERROR OR ADMIN
            if (userQuery == null || userQuery.AccessType.ToLower() != "admin")
            {
                return "UnAuthorized User";
            }

            try
            {
                var incomingQuery = await (from incoming in _context.SSv3_GAL_INCOMMINGTBL where incoming.isActive == 1 && incoming.Id == incomingId select incoming).FirstOrDefaultAsync();
                if(incomingQuery == null)
                {
                    return "Item Not Exist";
                }


                //Check if Item is in active Outgoing or is modified (has a pull-in/pull-out request)
                var outgoingQuery = await (from outgoing in _context.SSv3_GAL_OUTGOINGTBL where outgoing.isActive == 1 && outgoing.IncommingIds == incomingId select outgoing).FirstOrDefaultAsync();
                if (outgoingQuery != null)
                {
                    return "Delete Error Active";
                }

                var incomingModQuery = await (from incoming in _context.SSv3_GAL_INCOMMINGTBL where incoming.isActive == 1 && (incoming.Status.ToLower() == "full hold" || incoming.Status.ToLower() == "partial hold") && incoming.Id == incomingId select incoming).FirstOrDefaultAsync();
                if (incomingModQuery != null)
                {
                    return "Delete Error OnHold";
                }

                await _context.Database.BeginTransactionAsync();

                //IF OK DELETE THE ITEM
                incomingQuery.isActive = 0;
                incomingQuery.DateDeleted = DateTime.Now;
                incomingQuery.DeleteByUserName = currentUser;
                await _context.SaveChangesAsync();

                //SUBTRACT THE ITEM QUANTITY IN THE INVENTORY TABLE
                int itemId = incomingQuery.ItemId;
                decimal incomingQty = incomingQuery.IncommingQuantity;
                var inventoryItemQuery = await (from inventory in _context.SSv3_GAL_INVENTORYTBL where inventory.IncommingItemId == itemId select inventory).FirstOrDefaultAsync();
                inventoryItemQuery.IncommingTotalQuantity -= incomingQty;
                await _context.SaveChangesAsync();

                await _context.Database.CommitTransactionAsync();

                return "Delete Success";
            }

            catch
            {
                await _context.Database.RollbackTransactionAsync();
                return "Delete Error";
            }
        }

        public async Task<IActionResult> PullHistory()
        {
            var query = from history in _context.SSv3_GAL_INCOMING_HISTORYTBL
                        join incoming in _context.SSv3_GAL_INCOMMINGTBL on history.IncomingId equals incoming.Id
                        join item in _context.SSv3_GAL_ITEMTBL on incoming.ItemId equals item.Id
                        where item.isActive == 1
                        where incoming.isActive == 1
                        join user in _context.SSv3_GAL_USERTBL on history.AddedByUsername equals user.UserName
                        orderby history.Id descending
                        select new PullHistory
                        {
                            Id = history.Id,
                            BUname = item.BUName,
                            OrderNumber = history.OrderNumber, //reference in the request table
                            Status = history.Status, //request for pullout status
                            ItemNumber = item.ItemNumber,
                            Action = history.Action,
                            PullQuantity = history.Quantity,
                            UnitOfMeasure = item.UnitOfMeasure,
                            UnitPrice = history.UnitPrice,
                            TotalPrice = history.TotalPrice,
                            Reason = history.Reason,
                            TransactedBy = user.Name,
                            TransactionDate = history.DateAdded,
                        };

            var historyList = await query.ToListAsync();

            return View(historyList);
        }

        //API FOR ITEM DESCRIPTION BASED ON DROP DOWN
        [Route("[controller]/api/{itemId}/{BUName}")]
        [HttpGet]
        public async Task<ActionResult<List<SSv3_GAL_ITEMTBL>>> GetItemInfo(int itemId, string BUName)
        {
            var itemDetail = from item in _context.SSv3_GAL_ITEMTBL
                             where item.Id == itemId && item.isActive == 1 && item.BUName.ToLower() == BUName.ToLower()
                             select new SSv3_GAL_ITEMTBL
                             {
                                 Description = item.Description,
                                 UnitOfMeasure = item.UnitOfMeasure
                             };
            return await itemDetail.ToListAsync();
        }


        [Route("[controller]/api/{BUName}")]
        [HttpGet]
        public async Task<ActionResult<List<SSv3_GAL_ITEMTBL>>> GetItemInfo(string BUName)
        {
            var itemList = from item in _context.SSv3_GAL_ITEMTBL
                             where item.BUName.ToLower() == BUName.ToLower() && item.isActive == 1
                             select new SSv3_GAL_ITEMTBL
                             {
                                 ItemNumber = item.ItemNumber,
                                 Id = item.Id,
                             };
            return await itemList.ToListAsync();
        }
    }
}
