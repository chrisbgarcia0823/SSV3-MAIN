using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSuppliesV3.Data;
using ShopSuppliesV3.Models;
using ShopSuppliesV3.ViewModel;
using ShopSuppliesV3.Helpers;

namespace ShopSuppliesV3.Controllers
{
    public class StoreController : Controller
    {
        private readonly ShopSuppliesV3Context _context;

        public StoreController(ShopSuppliesV3Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, string searchOption, int? pageNumber)
        {
            try
            {
                string currentUser = User.Identity.Name.Substring(5).ToLower();
                var userQuery = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefaultAsync();
                string BuName = userQuery.BUname;

                //If USER IS NOT IN THE USERTABLE ERROR
                if (userQuery == null)
                {
                    return RedirectToAction("Error404", "Errors");
                }

                List<Store>? storeItemList = new List<Store>();

                //FOR PAGINATION
                IQueryable<Store>? query;

                //FILTER BASED ON USER BU
                if (userQuery.BUname.ToLower() == "all" || userQuery.AccessType.ToLower() == "admin")
                {
                    if(!string.IsNullOrEmpty(searchOption)) //search option is selected
                    {
                        //pageNumber = 1; //FOR PAGINATION PAGE NUMBER

                        if(searchOption == "itemNumber") //Filter based on item number
                        {
                            //FOR PAGINATION
                            query = from inventory in _context.SSv3_GAL_INVENTORYTBL
                                                  //var query = from inventory in _context.SSv3_GAL_INVENTORYTBL
                                              where inventory.RemainingStocksToRequest > 0
                                        join item in _context.SSv3_GAL_ITEMTBL on inventory.IncommingItemId equals item.Id
                                        where item.isActive == 1 && item.ItemNumber.ToLower() == searchString.ToLower()
                                        //join incomming in _context.SSv3_GAL_INCOMMINGTBL on item.Id equals incomming.ItemId where incomming.isActive == 1 && (incomming.Status.ToLower() != "consume" || incomming.Status.ToLower() != "expired")
                                        join supplier in _context.SSv3_GAL_SUPPLIERTBL on item.SupplierId equals supplier.Id
                                        where supplier.isActive == 1
                                        select new Store
                                        {
                                            Id = inventory.Id,
                                            ItemBU = item.BUName,
                                            ItemId = item.Id,
                                            ItemNumber = item.ItemNumber,
                                            Description = item.Description,
                                            UnitOfMeasure = item.UnitOfMeasure,
                                            Supplier = supplier.SupplierName,
                                            RemainingStocksOnHand = inventory.RemainingStocksOnhand,
                                            RemainingStocksToRequest = inventory.RemainingStocksToRequest,
                                            SafetyStock = item.SafetyStock,
                                        };

                            storeItemList = await query.ToListAsync();
                        }
                        else if(searchOption == "itemDescription") //Filter based on item description
                        {
                            query = from inventory in _context.SSv3_GAL_INVENTORYTBL
                                        //var query = from inventory in _context.SSv3_GAL_INVENTORYTBL
                                    where inventory.RemainingStocksToRequest > 0
                                        join item in _context.SSv3_GAL_ITEMTBL on inventory.IncommingItemId equals item.Id
                                        where item.isActive == 1 && EF.Functions.Like(item.Description, $"%{searchString}%") //FOR CONTINUATION TO BE MODIFIED USING LIKE%
                                        //join incomming in _context.SSv3_GAL_INCOMMINGTBL on item.Id equals incomming.ItemId where incomming.isActive == 1 && (incomming.Status.ToLower() != "consume" || incomming.Status.ToLower() != "expired")
                                        join supplier in _context.SSv3_GAL_SUPPLIERTBL on item.SupplierId equals supplier.Id
                                        where supplier.isActive == 1
                                        select new Store
                                        {
                                            Id = inventory.Id,
                                            ItemBU = item.BUName,
                                            ItemId = item.Id,
                                            ItemNumber = item.ItemNumber,
                                            Description = item.Description,
                                            UnitOfMeasure = item.UnitOfMeasure,
                                            Supplier = supplier.SupplierName,
                                            RemainingStocksOnHand = inventory.RemainingStocksOnhand,
                                            RemainingStocksToRequest = inventory.RemainingStocksToRequest,
                                            SafetyStock = item.SafetyStock,
                                        };

                            storeItemList = await query.ToListAsync();
                        }
                        else
                        {
                            return RedirectToAction("Error404", "Errors");
                        }
                    }
                    else
                    {
                        query = from inventory in _context.SSv3_GAL_INVENTORYTBL
                                    //var query = from inventory in _context.SSv3_GAL_INVENTORYTBL
                                where inventory.RemainingStocksToRequest > 0
                                    join item in _context.SSv3_GAL_ITEMTBL on inventory.IncommingItemId equals item.Id
                                    where item.isActive == 1
                                    //join incomming in _context.SSv3_GAL_INCOMMINGTBL on item.Id equals incomming.ItemId where incomming.isActive == 1 && (incomming.Status.ToLower() != "consume" || incomming.Status.ToLower() != "expired")
                                    join supplier in _context.SSv3_GAL_SUPPLIERTBL on item.SupplierId equals supplier.Id
                                    where supplier.isActive == 1
                                    select new Store
                                    {
                                        Id = inventory.Id,
                                        ItemBU = item.BUName,
                                        ItemId = item.Id,
                                        ItemNumber = item.ItemNumber,
                                        Description = item.Description,
                                        UnitOfMeasure = item.UnitOfMeasure,
                                        Supplier = supplier.SupplierName,
                                        RemainingStocksOnHand = inventory.RemainingStocksOnhand,
                                        RemainingStocksToRequest = inventory.RemainingStocksToRequest,
                                        SafetyStock = item.SafetyStock,
                                    };

                        storeItemList = await query.ToListAsync();
                    }

                }
                else //THE USER IS NOT AN ADMIN
                {
                    if (!string.IsNullOrEmpty(searchOption)) //search option is selected
                    {
                        //pageNumber = 1; //FOR PAGINATION PAGE NUMBER

                        if (searchOption == "itemNumber") //Filter based on item number
                        {
                            query = from inventory in _context.SSv3_GAL_INVENTORYTBL
                                        //var query = from inventory in _context.SSv3_GAL_INVENTORYTBL
                                    where inventory.RemainingStocksToRequest > 0
                                        join item in _context.SSv3_GAL_ITEMTBL on inventory.IncommingItemId equals item.Id
                                        where item.isActive == 1 && item.BUName == BuName && item.ItemNumber.ToLower() == searchString.ToLower()
                                        //join incomming in _context.SSv3_GAL_INCOMMINGTBL on item.Id equals incomming.ItemId where incomming.isActive == 1 && (incomming.Status.ToLower() != "consume" || incomming.Status.ToLower() != "expired")
                                        join supplier in _context.SSv3_GAL_SUPPLIERTBL on item.SupplierId equals supplier.Id
                                        where supplier.isActive == 1
                                        select new Store
                                        {
                                            Id = inventory.Id,
                                            ItemBU = item.BUName,
                                            ItemId = item.Id,
                                            ItemNumber = item.ItemNumber,
                                            Description = item.Description,
                                            UnitOfMeasure = item.UnitOfMeasure,
                                            Supplier = supplier.SupplierName,
                                            RemainingStocksOnHand = inventory.RemainingStocksOnhand,
                                            RemainingStocksToRequest = inventory.RemainingStocksToRequest,
                                            SafetyStock = item.SafetyStock,
                                        };

                            storeItemList = await query.ToListAsync();
                        }
                        else if (searchOption == "itemDescription") //Filter based on item description
                        {
                            query = from inventory in _context.SSv3_GAL_INVENTORYTBL
                                        //var query = from inventory in _context.SSv3_GAL_INVENTORYTBL
                                    where inventory.RemainingStocksToRequest > 0
                                        join item in _context.SSv3_GAL_ITEMTBL on inventory.IncommingItemId equals item.Id
                                        where item.isActive == 1 && item.BUName == BuName && EF.Functions.Like(item.Description, $"%{searchString}%") //FOR CONTINUATION TO BE MODIFIED USING LIKE%
                                        //join incomming in _context.SSv3_GAL_INCOMMINGTBL on item.Id equals incomming.ItemId where incomming.isActive == 1 && (incomming.Status.ToLower() != "consume" || incomming.Status.ToLower() != "expired")
                                        join supplier in _context.SSv3_GAL_SUPPLIERTBL on item.SupplierId equals supplier.Id
                                        where supplier.isActive == 1
                                        select new Store
                                        {
                                            Id = inventory.Id,
                                            ItemBU = item.BUName,
                                            ItemId = item.Id,
                                            ItemNumber = item.ItemNumber,
                                            Description = item.Description,
                                            UnitOfMeasure = item.UnitOfMeasure,
                                            Supplier = supplier.SupplierName,
                                            RemainingStocksOnHand = inventory.RemainingStocksOnhand,
                                            RemainingStocksToRequest = inventory.RemainingStocksToRequest,
                                            SafetyStock = item.SafetyStock,
                                        };

                            storeItemList = await query.ToListAsync();
                        }
                        else
                        {
                            return RedirectToAction("Error404", "Errors");
                        }
                    }
                    else
                    {
                        query = from inventory in _context.SSv3_GAL_INVENTORYTBL
                                    //var query = from inventory in _context.SSv3_GAL_INVENTORYTBL
                                where inventory.RemainingStocksToRequest > 0
                                    join item in _context.SSv3_GAL_ITEMTBL on inventory.IncommingItemId equals item.Id
                                    where item.isActive == 1 && item.BUName == BuName
                                    //join incomming in _context.SSv3_GAL_INCOMMINGTBL on item.Id equals incomming.ItemId where incomming.isActive == 1 && (incomming.Status.ToLower() != "consume" || incomming.Status.ToLower() != "expired")
                                    join supplier in _context.SSv3_GAL_SUPPLIERTBL on item.SupplierId equals supplier.Id
                                    where supplier.isActive == 1
                                    select new Store
                                    {
                                        Id = inventory.Id,
                                        ItemBU = item.BUName,
                                        ItemId = item.Id,
                                        ItemNumber = item.ItemNumber,
                                        Description = item.Description,
                                        UnitOfMeasure = item.UnitOfMeasure,
                                        Supplier = supplier.SupplierName,
                                        RemainingStocksOnHand = inventory.RemainingStocksOnhand,
                                        RemainingStocksToRequest = inventory.RemainingStocksToRequest,
                                        SafetyStock = item.SafetyStock,
                                    };

                        storeItemList = await query.ToListAsync();
                    }
                }

                //FOR THE CART ITEM INDICATOR ON TOP NAV
                //string userName = User.Identity.Name.Substring(5);
                //int requestorUserId = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == userName select user.Id).FirstOrDefaultAsync();
                //int cartItemCount = await (from request in _context.SSv3_ITEM_REQUESTTBL where request.isActive == 1 && (request.RequestStatus.ToLower() == "new") && request.RequestorUserId == requestorUserId select request.Id).CountAsync();                
                //TempData["ItemCount"] = cartItemCount;

                //FOR THE SEARCH STRING AND SEARCH OPTION TO HOLD VALUE
                TempData["SearchString"] = searchString;
                TempData["SearchOption"] = searchOption;

                //FOR PAGINATION NUMBER OF ITEM TO DISPLAY
                int pageSize = 15;
                var count = await query.CountAsync();
                if(pageNumber == null)
                {
                    TempData["PageNumber"] = 1;
                }
                else
                {
                    TempData["PageNumber"] = pageNumber;
                }
                TempData["TotalPage"] = (int)Math.Ceiling(count / (double)pageSize);

                return View(await PaginatedList<Store>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));
                //return View(storeItemList);
            }
            catch
            {
                return RedirectToAction("Error404", "Errors");
            }

        }

        //MOVE TO INVENTORY CONTROLLER
        //public async Task<IActionResult> TableView()
        //{
        //    string currentUser = User.Identity.Name.Substring(5).ToLower();
        //    var userQuery = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefaultAsync();

        //    //If USER IS NOT IN THE USERTABLE ERROR
        //    if (userQuery == null)
        //    {
        //        return RedirectToAction("Error404", "Errors");
        //    }

        //    var query = from inventory in _context.SSv3_GAL_INVENTORYTBL
        //                where inventory.RemainingStocksToRequest > 0
        //                join item in _context.SSv3_GAL_ITEMTBL on inventory.IncommingItemId equals item.Id
        //                where item.isActive == 1
        //                //join incomming in _context.SSv3_GAL_INCOMMINGTBL on item.Id equals incomming.ItemId where incomming.isActive == 1 && (incomming.Status.ToLower() != "consume" || incomming.Status.ToLower() != "expired")
        //                join supplier in _context.SSv3_GAL_SUPPLIERTBL on item.SupplierId equals supplier.Id
        //                where supplier.isActive == 1
        //                select new Store
        //                {
        //                    Id = inventory.Id,
        //                    ItemBU = item.BUName,
        //                    ItemId = item.Id,
        //                    //ItemName = item.ItemName,
        //                    Description = item.Description,
        //                    Supplier = supplier.SupplierName,
        //                    InRequestStocks = inventory.InRequestTotalQuantity,
        //                    RemainingStocksOnHand = inventory.RemainingStocksOnhand,
        //                    RemainingStocksToRequest = inventory.RemainingStocksToRequest,
        //                    OutgoingStocks = inventory.OutgoingTotalQuantity,
        //                    SafetyStock = item.SafetyStock,
        //                };
        //    var inventoryList = await query.ToListAsync();

        //    return View(inventoryList);
        //}

    }
}
