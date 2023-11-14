using Microsoft.AspNetCore.Mvc;
using ShopSuppliesV3.Data;
using ShopSuppliesV3.Models;
using ShopSuppliesV3.ViewModel;

namespace ShopSuppliesV3.Controllers
{
    public class CardexController : Controller
    {
        private readonly ShopSuppliesV3Context _context;

        public CardexController(ShopSuppliesV3Context context)
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

            var query = from outgoing in _context.SSv3_GAL_OUTGOINGTBL
                        where outgoing.isActive == 1
                        join request in _context.SSv3_ITEM_REQUESTTBL on outgoing.RequestId equals request.Id
                        where request.isActive == 1 && request.RequestStatus.ToLower() == "issued"
                        join item in _context.SSv3_GAL_ITEMTBL on request.InventoryItemId equals item.Id where item.isActive == 1
                        join requestor in _context.SSv3_GAL_USERTBL on request.RequestorUserId equals requestor.Id where requestor.isActive == 1
                        select new Outgoing
                        {
                            Id = outgoing.Id,
                            OrderNumber = request.OrderNumber,
                            RequestedByName = requestor.Name,
                            ItemNumber = item.ItemNumber,


                        };

            return View();
        }
    }
}
