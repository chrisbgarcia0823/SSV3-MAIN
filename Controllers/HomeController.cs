using Microsoft.AspNetCore.Mvc;
using ShopSuppliesV3.Models;
using System.Diagnostics;
using ShopSuppliesV3.Models;
using ShopSuppliesV3.ViewModel;
using ShopSuppliesV3.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ShopSuppliesV3.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;

        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}

        private readonly ShopSuppliesV3Context _context;

        public HomeController(ShopSuppliesV3Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                //ITEMS NEAR EXPIRATION
                string[] status = { "full hold", "consumed", "expired" };
                var query = from incoming in _context.SSv3_GAL_INCOMMINGTBL
                            where incoming.isActive == 1 && !status.Contains(incoming.Status.ToLower()) && incoming.ExpirationDate < DateTime.Now.AddDays(30)
                            join item in _context.SSv3_GAL_ITEMTBL on incoming.ItemId equals item.Id
                            where item.isActive == 1
                            orderby incoming.ExpirationDate descending
                            select new Incommings
                            {
                                Id = incoming.Id,
                                ExpirationDate = incoming.ExpirationDate,
                                Quantity = incoming.IncommingQuantity,
                                ItemNumber = item.ItemNumber,
                                DateAdded = incoming.DateAdded
                            };

                ViewData["ItemsNearExpiration"] = await query.ToListAsync();

                //CURRENT NEW REQUEST
                var requestQuery = from request in _context.SSv3_ITEM_REQUESTTBL where request.isActive == 1 && request.RequestStatus.ToLower() == "new" select request;
                TempData["NewRequestCount"] = await requestQuery.CountAsync();

                //CURRENT PENDING REQUEST
                var pendingQuery = from request in _context.SSv3_ITEM_REQUESTTBL where request.isActive == 1 && request.RequestStatus.ToLower() == "pending" select request;
                TempData["PendingRequestCount"] = await requestQuery.CountAsync();

                //CURRENT APPROVE REQUEST
                var approveQuery = from request in _context.SSv3_ITEM_REQUESTTBL where request.isActive == 1 && request.RequestStatus.ToLower() == "approved" select request;
                TempData["ApprovedRequestCount"] = await requestQuery.CountAsync();

                //CURRENT ISSUED REQUEST
                var issuedQuery = from request in _context.SSv3_ITEM_REQUESTTBL where request.isActive == 1 && request.RequestStatus.ToLower() == "issued" select request;
                TempData["IssuedRequestCount"] = await requestQuery.CountAsync();


                return View();
            }
            catch
            {
                return RedirectToAction("HomeError", "Errors");
            }

        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Store>>> SafetyStocks()
        {
            var query = from inventory in _context.SSv3_GAL_INVENTORYTBL
                        where inventory.RemainingStocksToRequest > 0
                        join item in _context.SSv3_GAL_ITEMTBL on inventory.IncommingItemId equals item.Id
                        where item.isActive == 1 && item.SafetyStock > inventory.RemainingStocksOnhand
                        join supplier in _context.SSv3_GAL_SUPPLIERTBL on item.SupplierId equals supplier.Id
                        where supplier.isActive == 1
                        orderby inventory.RemainingStocksOnhand ascending
                        select new Store
                        {
                            //InventoryId = inventory.Id,
                            //ItemBU = item.BUName,
                            //ItemId = item.Id,
                            ItemNumber = item.ItemNumber,
                            //Description = item.Description,
                            //Supplier = supplier.SupplierName,
                            //TotalOnHoldQuantity = inventory.TotalOnHoldQuantity,
                            //InRequestStocks = inventory.InRequestTotalQuantity,
                            RemainingStocksOnHand = inventory.RemainingStocksOnhand,
                            //RemainingStocksToRequest = inventory.RemainingStocksToRequest,
                            //OutgoingStocks = inventory.OutgoingTotalQuantity,
                            SafetyStock = item.SafetyStock,
                            //Adjustment = inventory.Adjustment

                        };

            return await query.ToListAsync();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Incommings>>> Expiration()
        {
            string[] status = { "full hold", "consumed", "expired" };

            var query = from incoming in _context.SSv3_GAL_INCOMMINGTBL
                        where incoming.isActive == 1 && !status.Contains(incoming.Status.ToLower()) && incoming.ExpirationDate < DateTime.Now.AddDays(30)
                        join item in _context.SSv3_GAL_ITEMTBL on incoming.ItemId equals item.Id
                        where item.isActive == 1
                        select new Incommings
                        {
                            Id = incoming.Id,
                            ExpirationDate = incoming.ExpirationDate,
                            Quantity = incoming.IncommingQuantity,
                            ItemNumber = item.ItemNumber,
                            DateAdded = incoming.DateAdded
                        };

            return await query.ToListAsync();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Incommings>>> OnHold()
        {
            var query = from incoming in _context.SSv3_GAL_INCOMMINGTBL
                        where incoming.isActive == 1 && incoming.Status.ToLower() == "partial hold" || incoming.Status.ToLower() == "full hold"
                        join item in _context.SSv3_GAL_ITEMTBL on incoming.ItemId equals item.Id
                        where item.isActive == 1
                        select new Incommings
                        {
                            ItemNumber = item.ItemNumber,
                            Quantity = incoming.IncommingQuantity,
                        };

            return await query.ToListAsync();
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Incommings>>> TopRequested()
        {
            var requestQuery = from request in _context.SSv3_ITEM_REQUESTTBL where request.isActive == 1 group request by request.InventoryItemId into topItems select new { topItems.Key, count = topItems.Count() };
            var topItemQuery = from item in _context.SSv3_GAL_ITEMTBL where item.isActive == 1
                               join request in requestQuery on item.Id equals request.Key 
                               orderby request.count descending
                               select new Incommings
                               {
                                   ItemNumber = item.ItemNumber,
                                   Quantity = request.count
                               };

            return await topItemQuery.Take(10).ToListAsync();
        }

        //SAMPLE API
        //[Route("[controller]/api/{itemId}")]
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<SSv3_GAL_ITEMTBL>>> GetItemInfo(int itemId)
        //{
        //    var query = from item in _context.SSv3_GAL_ITEMTBL where item.Id == itemId select item;
        //    return await query.ToListAsync();
        //}

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}