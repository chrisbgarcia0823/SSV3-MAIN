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

namespace ShopSuppliesV3.Controllers
{
    public class IssuanceController : Controller
    {
        private ShopSuppliesV3Context _context;
        public IssuanceController(ShopSuppliesV3Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

            //If USER IS NOT IN THE USERTABLE ERROR
            if (userQuery == null)
            {
                return RedirectToAction("Error404", "Errors");
            }
            else if (userQuery.AccessType.ToLower() != "admin")
            {
                return RedirectToAction("Error404", "Errors");
            }

            var groupByQuery = from request in _context.SSv3_ITEM_REQUESTTBL
                               where request.isActive == 1 && request.RequestStatus.ToLower() == "approved" && request.RequestCategory.ToLower() != "pullout"
                               group request by new { request.RequestorUserId, request.ApproverUserId } into orders
                               select new Carts
                               {
                                   RequestorId = orders.Key.RequestorUserId,
                                   RequestCount = orders.Count(),
                                   //ApproverUserId = orders.Key.ApproverUserId,
                               };
            //Join
            var joinQuery = from request in groupByQuery
                            join requestor in _context.SSv3_GAL_USERTBL on request.RequestorId equals requestor.Id
                            where requestor.isActive == 1
                            //join approver in _context.SSv3_GAL_USERTBL on request.ApproverUserId equals approver.Id where approver.isActive == 1
                            select new Carts
                            {
                                RequestorId = requestor.Id,
                                RequestorName = requestor.Name,
                                RequestCount = request.RequestCount,
                                BUname = requestor.BUname,
                                //ApproverName = approver.Name
                            };


            //FOR ADMIN NOTIFICATION ONLY (TO CONSTRUCT THE ADMIN PAGE)
            int openOrder = (from carts in joinQuery select carts).Count();
            ViewBag.openOrder = openOrder;
            var requestList = await joinQuery.ToListAsync();

            return View(requestList);
        }


        public async Task<IActionResult> IssuanceRequestDetails(int cartId) //cartID is equivalent to userId
        {
            string currentUser = User.Identity.Name.Substring(5).ToLower();
            var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

            //If USER IS NOT IN THE USERTABLE ERROR
            if (userQuery == null)
            {
                return RedirectToAction("Error404", "Errors");
            }
            else if (userQuery.AccessType.ToLower() != "admin")
            {
                return RedirectToAction("Error404", "Errors");
            }

            var query = from request in _context.SSv3_ITEM_REQUESTTBL
                        where request.isActive == 1 && request.RequestStatus.ToLower() == "approved" && request.RequestorUserId == cartId
                        join inventory in _context.SSv3_GAL_INVENTORYTBL on request.InventoryItemId equals inventory.IncommingItemId
                        join item in _context.SSv3_GAL_ITEMTBL on inventory.IncommingItemId equals item.Id
                        where item.isActive == 1
                        join supplier in _context.SSv3_GAL_SUPPLIERTBL on item.SupplierId equals supplier.Id
                        where supplier.isActive == 1
                        join user in _context.SSv3_GAL_USERTBL on request.RequestorUserId equals user.Id
                        where user.isActive == 1
                        join approver in _context.SSv3_GAL_USERTBL on request.ApproverUserId equals approver.Id
                        where approver.isActive == 1
                        select new Requests
                        {
                            Id = request.Id,
                            //RequestorUserId = request.RequestorUserId,
                            AreaName = request.AreaName,
                            OrderNumber = request.OrderNumber,
                            ApproverName = approver.Name,
                            BUname = item.BUName,
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

            return View(requestList);
        }

    }
}
