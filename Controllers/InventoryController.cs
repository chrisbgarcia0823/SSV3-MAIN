using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Frameworks;
using ShopSuppliesV3.Data;
using ShopSuppliesV3.Models;
using ShopSuppliesV3.ViewModel;

namespace ShopSuppliesV3.Controllers
{
    public class InventoryController : Controller
    {
        private readonly ShopSuppliesV3Context _context;

        public InventoryController(ShopSuppliesV3Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            //FOR ADMIN ACCESS ONLY
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

            //If USER IS NOT IN THE USERTABLE ERROR OR ADMIN
            if (userQuery == null || userQuery.AccessType.ToLower() != "admin")
            {
                return RedirectToAction("Error404", "Errors");
            }

            var query = from inventory in _context.SSv3_GAL_INVENTORYTBL
                        join item in _context.SSv3_GAL_ITEMTBL on inventory.IncommingItemId equals item.Id
                        where item.isActive == 1
                        join supplier in _context.SSv3_GAL_SUPPLIERTBL on item.SupplierId equals supplier.Id
                        where supplier.isActive == 1
                        select new Store
                        {
                            InventoryId = inventory.Id,
                            ItemBU = item.BUName,
                            ItemId = item.Id,
                            ItemNumber = item.ItemNumber,
                            Description = item.Description,
                            Supplier = supplier.SupplierName,
                            TotalOnHoldQuantity = inventory.TotalOnHoldQuantity,
                            InRequestStocks = inventory.InRequestTotalQuantity,
                            RemainingStocksOnHand = inventory.RemainingStocksOnhand,
                            RemainingStocksToRequest = inventory.RemainingStocksToRequest,
                            OutgoingStocks = inventory.OutgoingTotalQuantity,
                            SafetyStock = item.SafetyStock,
                            Adjustment = inventory.Adjustment

                        };
            var inventoryList = await query.ToListAsync();

            return View(inventoryList);
        }

        [HttpPost]
        public async Task<IActionResult> AdjustInventory(IFormCollection formData)
        {
            //FOR ADMIN ACCESS ONLY
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

            //If USER IS NOT IN THE USERTABLE ERROR OR ADMIN
            if (userQuery == null || userQuery.AccessType.ToLower() != "admin")
            {
                return RedirectToAction("Error404", "Errors");
            }

            int.TryParse(formData["InventoryId"].ToString(), out int inventoryId);
            int.TryParse(formData["Adjustment"].ToString(), out int adjustment);
            string remarks = formData["Remarks"].ToString();

            try
            {
                _context.Database.BeginTransactionAsync();

                var inventoryAdjustment =  await _context.SSv3_GAL_INVENTORYTBL.FindAsync(inventoryId);
                inventoryAdjustment.Adjustment += adjustment;
                _context.SaveChanges();

                var newAdjustment = new SSv3_GAL_ADJUSTMENTTBL
                {
                    InventoryId = inventoryId,
                    Adjustment = adjustment,
                    AdjustedBy = currentUser,
                    Remarks = remarks,
                    AdjustmentDate = DateTime.Now
                   
                };

                _context.Add(newAdjustment);
                _context.SaveChanges();

                _context.Database.CommitTransactionAsync();

                TempData["ItemNumber"] = formData["ItemNumber"].ToString();

                //Edit request comes from SearchInventory ActionMethod and not from Index
                if (!string.IsNullOrEmpty(formData["FromEditInventorySearch"].ToString())) //SearchInventory is from MODAL EDITING input (Distinguish weather the request is from index or the SearchInventory Action method)
                {
                    return RedirectToAction("SearchInventory", new { itemNumber = formData["FromEditInventorySearch"].ToString() }); //Send back the itemNumber to the EditIncoming Action Method
                }
                
                return RedirectToAction("Index");
            }
            catch
            {
                _context.Database.RollbackTransactionAsync();
                return RedirectToAction("InventoryAdjustmentError", "Errors");
            }
        }

        //FOR THE SEARCH/FILTERED VIEW
        public async Task<IActionResult> SearchInventory(string itemNumber)
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
                    TempData["InventoryInitialSearch"] = "not null"; //Used in the view to distinguised weather the search has a parameter or not
                    return View();
                }
                else
                {
                    var query = from inventory in _context.SSv3_GAL_INVENTORYTBL
                                where inventory.RemainingStocksToRequest > 0
                                join item in _context.SSv3_GAL_ITEMTBL on inventory.IncommingItemId equals item.Id
                                where item.isActive == 1 && item.ItemNumber == itemNumber
                                //join incomming in _context.SSv3_GAL_INCOMMINGTBL on item.Id equals incomming.ItemId where incomming.isActive == 1 && (incomming.Status.ToLower() != "consume" || incomming.Status.ToLower() != "expired")
                                join supplier in _context.SSv3_GAL_SUPPLIERTBL on item.SupplierId equals supplier.Id
                                where supplier.isActive == 1
                                select new Store
                                {
                                    InventoryId = inventory.Id,
                                    ItemBU = item.BUName,
                                    ItemId = item.Id,
                                    ItemNumber = item.ItemNumber,
                                    Description = item.Description,
                                    Supplier = supplier.SupplierName,
                                    TotalOnHoldQuantity = inventory.TotalOnHoldQuantity,
                                    InRequestStocks = inventory.InRequestTotalQuantity,
                                    RemainingStocksOnHand = inventory.RemainingStocksOnhand,
                                    RemainingStocksToRequest = inventory.RemainingStocksToRequest,
                                    OutgoingStocks = inventory.OutgoingTotalQuantity,
                                    SafetyStock = item.SafetyStock,
                                    Adjustment = inventory.Adjustment

                                };
                    var inventoryList = await query.ToListAsync();

                    TempData["FromEditInventorySearch"] = itemNumber;
                    return View(inventoryList);
                }
            }
            catch
            {
                return RedirectToAction("InventorySearchError", "Errors");
            }
        }

        public async Task<IActionResult> InventoryAdjustment()
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
                var query = from adjustments in _context.SSv3_GAL_ADJUSTMENTTBL
                            join inventory in _context.SSv3_GAL_INVENTORYTBL on adjustments.InventoryId equals inventory.Id
                            join item in _context.SSv3_GAL_ITEMTBL on inventory.IncommingItemId equals item.Id
                            where item.isActive == 1
                            join user in _context.SSv3_GAL_USERTBL on adjustments.AdjustedBy equals user.UserName
                            where user.isActive == 1
                            orderby adjustments.AdjustmentDate descending
                            select new InventoryAdjustments
                            {
                                Id = adjustments.Id,
                                Name = user.Name,
                                ItemNumber = item.ItemNumber,
                                AdjustmentDate = adjustments.AdjustmentDate,
                                AdjustmentQty = adjustments.Adjustment,
                                Remarks = adjustments.Remarks,
                            };

                var ajustmentList = await query.ToListAsync();

                return View(ajustmentList);
            }
            catch
            {
                return RedirectToAction("InventoryAdjustmentView", "Errors");
            }
        }
    }
}
