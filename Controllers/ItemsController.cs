using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using NuGet.Frameworks;
using ShopSuppliesV3.Data;
using ShopSuppliesV3.Models;
using ShopSuppliesV3.ViewModel;

namespace ShopSuppliesV3.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ShopSuppliesV3Context _context;

        public ItemsController(ShopSuppliesV3Context context)
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

            var query = from item in _context.SSv3_GAL_ITEMTBL
                        where item.isActive == 1
                        join supplier in _context.SSv3_GAL_SUPPLIERTBL on item.SupplierId equals supplier.Id
                        where supplier.isActive == 1
                        join user in _context.SSv3_GAL_USERTBL on item.AddedByUsername equals user.UserName
                        where user.isActive == 1
                        select new Items
                        {
                            Id = item.Id,
                            BUName = item.BUName,
                            BuyerName = item.BuyerName,
                            ItemNumber = item.ItemNumber,
                            Description = item.Description,
                            UnitOfMeasure = item.UnitOfMeasure,
                            SafetyStock = item.SafetyStock,
                            SupplierName = supplier.SupplierName,
                            UserName = user.Name,
                            DateAdded = item.DateAdded
                        };
            return View(await query.ToListAsync());
        }


        public async Task<IActionResult> Create()
        {
            //FOR ADMIN ACCESS ONLY
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

            //If USER IS NOT IN THE USERTABLE ERROR OR ADMIN
            if (userQuery == null || userQuery.AccessType.ToLower() != "admin")
            {
                return RedirectToAction("Error404", "Errors");
            }

            try
            {
                var suppliers = from sup in _context.SSv3_GAL_SUPPLIERTBL where sup.isActive == 1 select sup;
                var unitOfMeasure = from um in _context.SSv3_GAL_UMTBL select um.UnitOfMeasure;
                var bu = from b in _context.SSv3_GAL_BUTBL select b.BUname;
                var buyer = from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.AccessType.ToLower() == "buyer" select user.Name;

                ViewData["Suppliers"] = await suppliers.ToListAsync();
                ViewData["UnitOfMeasure"] = await unitOfMeasure.ToListAsync();
                ViewData["BU"] = await bu.ToListAsync();
                ViewData["BuyerName"] = await buyer.ToListAsync();

                return View();
            }
            catch
            {
                return RedirectToAction("ItemCreationError", "Errors");
            }
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection formData)
        {
            //FOR ADMIN ACCESS ONLY
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

            //If USER IS NOT IN THE USERTABLE ERROR OR ADMIN
            if (userQuery == null || userQuery.AccessType.ToLower() != "admin")
            {
                return RedirectToAction("Error404", "Errors");
            }

            try
            {
                SSv3_GAL_ITEMTBL newItem = new SSv3_GAL_ITEMTBL
                {
                    BUName = formData["BUName"].ToString(),
                    ItemNumber = formData["ItemNumber"].ToString(),
                    BuyerName = formData["BuyerName"].ToString(),
                    UnitOfMeasure = formData["UnitOfMeasure"].ToString(),
                    SupplierId = int.Parse(formData["SupplierId"].ToString()),
                    Description = formData["Description"].ToString(),
                    AddedByUsername = User.Identity.Name.Substring(5),
                    isActive = 1,
                    SafetyStock = int.Parse(formData["SafetyStock"].ToString()),
                    DateAdded = DateTime.Now
                };

                _context.Add(newItem);
                await _context.SaveChangesAsync();

                TempData["AddedSuccess"] = formData["ItemName"].ToString();

                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("ItemCreationError", "Errors");
            }

        }

        public async Task<IActionResult> EditItem(int itemId)
        {
            //FOR ADMIN ACCESS ONLY
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

            //If USER IS NOT IN THE USERTABLE ERROR OR ADMIN
            if (userQuery == null || userQuery.AccessType.ToLower() != "admin")
            {
                return RedirectToAction("Error404", "Errors");
            }

            try
            {
                var query = from item in _context.SSv3_GAL_ITEMTBL
                            where item.isActive == 1 && item.Id == itemId
                            join supplier in _context.SSv3_GAL_SUPPLIERTBL on item.SupplierId equals supplier.Id
                            where supplier.isActive == 1
                            join user in _context.SSv3_GAL_USERTBL on item.AddedByUsername equals user.UserName
                            where user.isActive == 1
                            select new Items
                            {
                                Id = item.Id,
                                BUName = item.BUName,
                                BuyerName = item.BuyerName,
                                ItemNumber = item.ItemNumber,
                                Description = item.Description,
                                UnitOfMeasure = item.UnitOfMeasure,
                                SafetyStock = item.SafetyStock,
                                SupplierName = supplier.SupplierName,
                                UserName = user.Name,
                                DateAdded = item.DateAdded
                            };

                var itemquery = await query.FirstOrDefaultAsync();

                if (itemquery == null)
                {
                    TempData["ItemDoesNotExis"] = itemId;
                    return RedirectToAction("Index");
                }

                var suppliers = from sup in _context.SSv3_GAL_SUPPLIERTBL where sup.isActive == 1 select sup;
                var unitOfMeasure = from um in _context.SSv3_GAL_UMTBL select um.UnitOfMeasure;
                var bu = from b in _context.SSv3_GAL_BUTBL select b.BUname;
                var buyer = from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.AccessType.ToLower() == "buyer" select user.Name;

                ViewData["Suppliers"] = await suppliers.ToListAsync();
                ViewData["UnitOfMeasure"] = await unitOfMeasure.ToListAsync();
                ViewData["BU"] = await bu.ToListAsync();
                ViewData["BuyerName"] = await buyer.ToListAsync();

                return View(itemquery);
            }
            catch
            {
                return RedirectToAction("ItemEditError", "Errors");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditItem(IFormCollection formData)
        {
            //FOR ADMIN ACCESS ONLY
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

            //If USER IS NOT IN THE USERTABLE ERROR OR ADMIN
            if (userQuery == null || userQuery.AccessType.ToLower() != "admin")
            {
                return RedirectToAction("Error404", "Errors");
            }

            try
            {
                int itemId = int.Parse(formData["ItemId"].ToString());
                string buName = formData["BUname"].ToString();
                string buyerName = formData["BuyerName"].ToString();
                string itemNumber = formData["ItemNumber"].ToString();
                int supplierId = int.Parse(formData["SupplierId"].ToString());
                string description = formData["Description"].ToString();
                decimal safetyStock = decimal.Parse(formData["SafetyStock"].ToString());
                string unitOfMeasure = formData["UnitOfMeasure"].ToString();

                //Check if item is already in the incoming and request
                var itemQueryCheck1 = await (from incoming in _context.SSv3_GAL_INCOMMINGTBL where incoming.isActive == 1 && incoming.ItemId == itemId select incoming).FirstOrDefaultAsync();
                var itemQueryCheck2 = await (from request in _context.SSv3_ITEM_REQUESTTBL where request.isActive == 1 && request.InventoryItemId == itemId select request).FirstOrDefaultAsync();
                if(itemQueryCheck1 != null || itemQueryCheck2 != null)
                {
                    TempData["ItemEditWarning"] = itemNumber;
                    return RedirectToAction("Index");
                }

                var itemQuery = await (from item in _context.SSv3_GAL_ITEMTBL where item.isActive == 1 && item.Id == itemId select item).FirstOrDefaultAsync();
                if (itemQuery == null)
                {
                    TempData["ItemDoesNotExis"] = itemId;
                    return RedirectToAction("Index");
                }

                itemQuery.BUName = buName;
                itemQuery.ItemNumber = itemNumber;
                itemQuery.UnitOfMeasure = unitOfMeasure;
                itemQuery.SupplierId = supplierId;
                itemQuery.Description = description;
                itemQuery.SafetyStock = safetyStock;
                itemQuery.BuyerName = buyerName;
                itemQuery.UnitOfMeasure = unitOfMeasure;
                itemQuery.EditByUserName = currentUser;
                itemQuery.DateEdited = DateTime.Now;
                await _context.SaveChangesAsync();

                TempData["EditItemSuccess"] = itemNumber;
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("ItemEditError", "Errors");
            }
        }


        //FOR GETTING THE INDIVIDUAL DATA TO BE DELETED USING FETCH API IN THE INDEX DELETE MODAL
        [Route("[controller]/[action]/{itemId}")] //Route is added (to match the fetch api @url action)
        public async Task<string> DeleteItem(int itemId)
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
                var itemQuery = await (from item in _context.SSv3_GAL_ITEMTBL where item.Id == itemId select item).FirstOrDefaultAsync();
                if(itemQuery == null)
                {
                    return "Item Not Exist";
                }

                //Check if Item is in Incoming Table or in active Request
                var incomingQuery = await (from incoming in _context.SSv3_GAL_INCOMMINGTBL where incoming.isActive == 1 && incoming.ItemId == itemQuery.Id select incoming).FirstOrDefaultAsync();
                var requestQuery = await (from request in _context.SSv3_ITEM_REQUESTTBL where request.isActive == 1 && request.InventoryItemId == itemQuery.Id select request).FirstOrDefaultAsync();
                if(incomingQuery != null || requestQuery != null)
                {
                    return "Delete Error";
                }

                //IF OK DELETE THE ITEM
                itemQuery.isActive = 0;
                itemQuery.DateDeleted = DateTime.Now;
                itemQuery.DeleteByUserName = currentUser;
                await _context.SaveChangesAsync();

                return "Delete Success";
            }

            catch
            {
                return "Delete Error";
            }

        }
    }
}
