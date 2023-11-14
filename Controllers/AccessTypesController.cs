using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSuppliesV3.Data;
using ShopSuppliesV3.Models;
using ShopSuppliesV3.ViewModel;

namespace ShopSuppliesV3.Controllers
{
    public class AccessTypesController : Controller
    {
        private readonly ShopSuppliesV3Context _context;

        public AccessTypesController(ShopSuppliesV3Context context)
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

            var query = from access in _context.SSv3_GAL_ACCESSTYPETBL
                        where access.isActive == 1
                        join user in _context.SSv3_GAL_USERTBL on access.AddedBy equals user.UserName
                        where user.isActive == 1
                        orderby access.Id descending
                        select new Access
                        {
                            Id = access.Id,
                            AccessType = access.AccessType,
                            AddedByName = user.Name,
                            DateAdded = access.DateAdded
                        };

            var accessList = await query.ToListAsync();

            return View(accessList);
        }

        //CREATE NEW ACCESS
        [HttpPost]
        public async Task<IActionResult> CreateAccess(IFormCollection formData)
        {
            string accessName = formData["AccessName"].ToString();

            try
            {
                //FOR ADMIN ACCESS ONLY
                string currentUser = User.Identity.Name.Substring(5).ToLower();
                var userQuery = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefaultAsync();

                //If USER IS NOT IN THE USERTABLE ERROR OR ADMIN
                if (userQuery == null || userQuery.AccessType.ToLower() != "admin")
                {
                    return RedirectToAction("Error404", "Errors");
                }

                //CHECK IF ACCESS ALREADY EXIST
                string accessQuery = await (from access in _context.SSv3_GAL_ACCESSTYPETBL where access.isActive == 1 && access.AccessType.ToLower() == accessName.ToLower() select access.AccessType).FirstOrDefaultAsync();
                if (!string.IsNullOrEmpty(accessQuery))
                {
                    TempData["AccessExist"] = accessName;
                    return RedirectToAction("Index");
                }

                //CREATE NEW ACCESS IF NOT EXISTING
                SSv3_GAL_ACCESSTYPETBL newAccesss = new SSv3_GAL_ACCESSTYPETBL
                {
                    AccessType = accessName,
                    AddedBy = currentUser,
                    DateAdded = DateTime.Now,
                    isActive = 1
                };

                _context.Add(newAccesss);
                _context.SaveChanges();

                TempData["AccessAddSuccess"] = accessName;
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("AccessAddingError", "Errors");
            }
        }

        //EDIT EXISTING ACCESS
        [HttpPost]
        public async Task<IActionResult> EditAccess(IFormCollection formData)
        {
            string oldAccessName = formData["OldAccessName"].ToString();
            string newAccessName = formData["NewAccessName"].ToString();
            try
            {
                int accessId = int.Parse(formData["EditAccessId"].ToString());

                //FOR ADMIN ACCESS ONLY
                string currentUser = User.Identity.Name.Substring(5).ToLower();
                var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

                //If USER IS NOT IN THE USERTABLE ERROR OR ADMIN
                if (userQuery == null || userQuery.AccessType.ToLower() != "admin")
                {
                    return RedirectToAction("Error404", "Errors");
                }

                //Check if the edit access is used in an active user
                var userQueryCheck = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.AccessType.ToLower() == oldAccessName.ToLower() select user).FirstOrDefaultAsync();
                if (userQueryCheck != null)
                {
                    TempData["EditWarning"] = oldAccessName;
                    return RedirectToAction("Index");
                }

                //Check if the edit access has existing name (duplicate Entry)
                var checkDuplicateQuery = await (from access in _context.SSv3_GAL_ACCESSTYPETBL where access.isActive == 1 && access.AccessType.ToLower() == newAccessName.ToLower() select access).FirstOrDefaultAsync();
                if (checkDuplicateQuery != null)
                {
                    TempData["EditWarningDuplicate"] = newAccessName;
                    return RedirectToAction("Index");
                }

                //EDIT ACCESS IF IT EXIST
                var accessQuery = await (from access in _context.SSv3_GAL_ACCESSTYPETBL where access.isActive == 1 && access.Id == accessId select access).FirstOrDefaultAsync();
                if (accessQuery == null)
                {
                    TempData["EditError"] = oldAccessName;
                    return RedirectToAction("Index");
                }

                accessQuery.AccessType = newAccessName;
                accessQuery.EditByUserName = currentUser;
                accessQuery.DateEdited = DateTime.Now;
                _context.SaveChanges();

                TempData["EditSuccess"] = oldAccessName;
                TempData["EditSuccessNewName"] = newAccessName;
                return RedirectToAction("Index");

            }
            catch
            {
                return RedirectToAction("AccessEditError", "Errors");
            }
        }

        //DELETE THE ACCESS
        [HttpPost]
        public async Task<IActionResult> DeleteAccess(IFormCollection formData)
        {
            string accessName = formData["DeleteAccessName"].ToString();

            try
            {
                int accessId = int.Parse(formData["AccessId"].ToString());

                //FOR ADMIN ACCESS ONLY
                string currentUser = User.Identity.Name.Substring(5).ToLower();
                var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

                //If USER IS NOT IN THE USERTABLE ERROR OR ADMIN
                if (userQuery == null || userQuery.AccessType.ToLower() != "admin")
                {
                    return RedirectToAction("Error404", "Errors");
                }

                //Check if the edit access is used in an active user
                var userQueryCheck = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.AccessType.ToLower() == accessName.ToLower() select user).FirstOrDefaultAsync();
                if (userQueryCheck != null)
                {
                    TempData["DeleteWarning"] = accessName;
                    return RedirectToAction("Index");
                }

                //DELETE ACCESS IF IT EXIST
                var accessQuery = await (from access in _context.SSv3_GAL_ACCESSTYPETBL where access.isActive == 1 && access.Id == accessId select access).FirstOrDefaultAsync();
                if (accessQuery == null)
                {
                    TempData["DeleteError"] = accessName;
                    return RedirectToAction("Index");
                }

                accessQuery.isActive = 0;
                accessQuery.DeleteByUserName = currentUser;
                accessQuery.DateDeleted = DateTime.Now;
                _context.SaveChanges();

                TempData["DeleteSuccess"] = accessName;
                return RedirectToAction("Index");

            }
            catch
            {
                return RedirectToAction("AccessDeleteError", "Errors");
            }
        }
    }
}
