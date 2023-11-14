using Microsoft.AspNetCore.Mvc;
using ShopSuppliesV3.Models;
using ShopSuppliesV3.Data;
using ShopSuppliesV3.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace ShopSuppliesV3.Controllers
{
    public class UsersController : Controller
    {
        private readonly ShopSuppliesV3Context _context;

        public UsersController(ShopSuppliesV3Context context)
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

            var query = from user in _context.SSv3_GAL_USERTBL
                        where user.isActive == 1
                        select new Users
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            isApprover = user.isApprover,
                            EmailAdd = user.EmailAdd,
                            Name = user.Name,
                            AccessType = user.AccessType,
                            BUname = user.BUname,
                            addedBy = user.addedBy,
                            DateAdded = user.DateAdded
                        };
            var usersList = await query.ToListAsync();

            var bu = from b in _context.SSv3_GAL_BUTBL where b.isActive == 1 select b.BUname;
            var buList = await bu.ToListAsync();

            var access = from a in _context.SSv3_GAL_ACCESSTYPETBL where a.isActive == 1 select a.AccessType;
            var accessList = await access.ToListAsync();

            //var approvers = from a in _context.SSv3_GAL_USERTBL where a.isApprover == true select a;
            //var approversList = await approvers.ToListAsync();

            ViewData["BU"] = buList;
            ViewData["Access"] = accessList;

            return View(usersList);
        }


        //ADDING A NEW USER
        [HttpPost]
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

            string userName = formData["UserName"].ToString().ToLower();
            string name = formData["Name"].ToString();

            try
            {

                //CHECK IF USER ALREADY EXIST IN THE DATABASE
                var query = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == userName select user).FirstOrDefault();

                if(query != null)
                {

                    TempData["UserAlreadyExist"] = name;
                    return RedirectToAction("Index");
                }

                string approverCheckboxValue = formData["isApprover"].ToString();
                string? approverEmail = formData["Approver"].ToString();
                bool isApprover = false;
                if(approverCheckboxValue.ToLower() == "true")
                {
                    isApprover = true;
                }

                SSv3_GAL_USERTBL newUser = new SSv3_GAL_USERTBL
                {
                    isActive = 1,
                    isApprover = isApprover,
                    //ApproverEmailAdd = approverEmail,
                    Name = name,
                    UserName = userName,
                    EmailAdd = formData["EmailAdd"].ToString(),
                    AccessType = formData["AccessType"].ToString(),
                    BUname = formData["BUname"].ToString(),
                    addedBy = User.Identity.Name.Substring(5),
                    DateAdded = DateTime.Now
                };

                _context.Add(newUser);
                await _context.SaveChangesAsync();

                TempData["UsersAddedSuccess"] = name;

                return RedirectToAction("Index");
            }
            catch
            {
                TempData["UserName"] = userName.ToString();
                return RedirectToAction("UserAddingError", "Errors");
            };

        }

        //FOR GETTING THE INDIVIDUAL DATA TO BE USED IN FETCH API IN THE INDEX EDIT MODAL
        [HttpGet]
        [Route("[controller]/[action]/{userId}")]
        public async Task<ActionResult<SSv3_GAL_USERTBL>> GetUserInfo(int userId)
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
                var query = from user in _context.SSv3_GAL_USERTBL where user.Id == userId select user;
                return await query.FirstOrDefaultAsync();
            }

            catch
            {
                return RedirectToAction("Error404", "Errors");
            }

        }

        //EDIT USER
        [HttpPost]
        public async Task<IActionResult> EditUser(IFormCollection formData)
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
                int userId = int.Parse(formData["EditUserId"].ToString());
                string name = formData["NameEdit"].ToString();
                string buName = formData["BUnameEdit"].ToString();
                string userName = formData["UserNameEdit"].ToString();
                string emailAdd = formData["EmailAddEdit"].ToString();
                string accessType = formData["AccessTypeEdit"].ToString();
                bool isApprover = false;
                if (!string.IsNullOrEmpty(formData["isApproverEdit"].ToString()))
                {
                    isApprover = true;
                }

                //Check if user has current request (check in request table if user has new, pending, approved request,  )
                string[] requestStatus = { "new", "pending", "approved"};
                var requestCheckQuery = await (from request in _context.SSv3_ITEM_REQUESTTBL where request.isActive == 1 &&
                                               request.RequestorUserId == userId && requestStatus.Contains(request.RequestStatus)
                                               select request).FirstOrDefaultAsync();
                if(requestCheckQuery != null)
                {
                    TempData["UserEditError"] = name;
                    return RedirectToAction("Index");
                }

                //Check if user is in the user table
                var userQueryCheck = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.Id == userId select user).FirstOrDefaultAsync();
                if (userQueryCheck == null)
                {
                    TempData["UserNotExistError"] = name;
                    return RedirectToAction("Index");
                }

                //IF ALL IS GOOD PERFORM EDIT ON THE REQUEST USER ID
                userQueryCheck.BUname = buName;
                userQueryCheck.UserName = userName;
                userQueryCheck.EmailAdd = emailAdd;
                userQueryCheck.AccessType = accessType;
                userQueryCheck.Name = name;
                userQueryCheck.isApprover = isApprover;
                userQueryCheck.DateEdited = DateTime.Now;
                userQueryCheck.EditByUserName = currentUser;
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("UserEditError", "Errors");
            }

        }

        //DELETE USER
        [HttpPost]
        public async Task<IActionResult> DeleteUser(IFormCollection formData)
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
                int userId = int.Parse(formData["DeleteUserId"].ToString());
                string name = formData["DeleteName"].ToString();

                //Check if user has current request (check in request table if user has new, pending, approved request,  )
                string[] requestStatus = { "new", "pending", "approved" };
                var requestCheckQuery = await (from request in _context.SSv3_ITEM_REQUESTTBL
                                               where request.isActive == 1 &&
                                               request.RequestorUserId == userId && requestStatus.Contains(request.RequestStatus)
                                               select request).FirstOrDefaultAsync();
                if (requestCheckQuery != null)
                {
                    TempData["UserDeleteError"] = name;
                    return RedirectToAction("Index");
                }

                //Check if user is in the user table
                var userQueryCheck = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.Id == userId select user).FirstOrDefaultAsync();
                if (userQueryCheck == null)
                {
                    TempData["UserNotExistError"] = name;
                    return RedirectToAction("Index");
                }

                //IF ALL IS GOOD PERFORM DELETE ON THE REQUEST USER ID
                userQueryCheck.isActive = 0;
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("UserDeleteError", "Errors");
            }

        }




        //SAMPLE API
        //[Route("[controller]/api/{itemId}")]
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<SSv3_GAL_ITEMTBL>>> GetItemInfo(int itemId)
        //{
        //    var query = from item in _context.SSv3_GAL_ITEMTBL where item.Id == itemId select item;
        //    return await query.ToListAsync();
        //}


        //public async Task<IActionResult> Create()
        //{
        //    var bu = from b in _context.SSv3_GAL_BUTBL select b.BUname;
        //    var buList = await bu.ToListAsync();

        //    var access = from a in _context.SSv3_GAL_ACCESSTYPETBL select a.AccessType;
        //    var accessList = await access.ToListAsync();

        //    //var approvers = from a in _context.SSv3_GAL_USERTBL where a.isApprover == true select a;
        //    //var approversList = await approvers.ToListAsync();

        //    ViewData["BU"] = buList;
        //    ViewData["Access"] = accessList;
        ////    //ViewData["Approvers"] = approversList;

        //    return View();
        //}
    }
}
