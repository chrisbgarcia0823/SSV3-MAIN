using Microsoft.AspNetCore.Mvc;
using ShopSuppliesV3.ViewModel;
using ShopSuppliesV3.Models;
using ShopSuppliesV3.Data;
using Microsoft.EntityFrameworkCore;

namespace ShopSuppliesV3.Controllers
{
    public class AreaController : Controller
    {
        private readonly ShopSuppliesV3Context _context;

        public AreaController(ShopSuppliesV3Context context)
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

            var query = from area in _context.SSv3_GAL_AREATBL where area.isActive == 1
                        join user in _context.SSv3_GAL_USERTBL on area.AddedBy equals user.UserName where user.isActive == 1
                        orderby area.Id descending select new Area
                        {
                            Id = area.Id,
                            AreaName = area.AreaName,
                            AddedByName = user.Name,
                            DateAdded = area.DateAdded
                        };

            var areaList = await query.ToListAsync();

            return View(areaList);
        }

        //CREATE NEW AREA
        [HttpPost]
        public async Task<IActionResult> CreateArea(IFormCollection formData)
        {
            string areaName = formData["AreaName"].ToString();

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

                //CHECK IF AREA ALREADY EXIST
                string areaQuery = await (from area in _context.SSv3_GAL_AREATBL where area.isActive == 1 && area.AreaName.ToLower() == areaName.ToLower() select area.AreaName).FirstOrDefaultAsync();
                if (!string.IsNullOrEmpty(areaQuery))
                {
                    TempData["AreaExist"] = areaName;
                    return RedirectToAction("Index");
                }

                //CREATE NEW AREA IF NOT EXISTING
                SSv3_GAL_AREATBL newArea = new SSv3_GAL_AREATBL
                {
                    AreaName = areaName,
                    AddedBy = currentUser,
                    DateAdded = DateTime.Now,
                    isActive = 1
                };

                _context.Add(newArea);
                _context.SaveChanges();

                TempData["AreaAddSuccess"] = areaName;
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("AreaAddingError", "Errors");
            }
        }

        //EDIT EXISTING AREA
        [HttpPost]
        public async Task<IActionResult> EditArea(IFormCollection formData)
        {
            string oldAreaName = formData["OldAreaName"].ToString();
            string newAreaName = formData["NewAreaName"].ToString();
            try
            {
                int areaId = int.Parse(formData["EditAreaId"].ToString());

                //FOR ADMIN ACCESS ONLY
                string currentUser = User.Identity.Name.Substring(5).ToLower();
                var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

                //If USER IS NOT IN THE USERTABLE ERROR OR ADMIN
                if (userQuery == null || userQuery.AccessType.ToLower() != "admin")
                {
                    return RedirectToAction("Error404", "Errors");
                }

                //Check if the edit area is used in an active item request
                var requestQuery = await (from request in _context.SSv3_ITEM_REQUESTTBL where request.isActive == 1 && request.AreaName.ToLower() == oldAreaName.ToLower() select request).FirstOrDefaultAsync();
                if (requestQuery != null)
                {
                    TempData["EditWarning"] = oldAreaName;
                    return RedirectToAction("Index");
                }

                //Check if the edit area has existing name (duplicate Entry)
                var checkDuplicateQuery = await (from area in _context.SSv3_GAL_AREATBL where area.isActive == 1 && area.AreaName.ToLower() == newAreaName.ToLower() select area).FirstOrDefaultAsync();
                if (checkDuplicateQuery != null)
                {
                    TempData["EditWarningDuplicate"] = newAreaName;
                    return RedirectToAction("Index");
                }

                //EDIT AREA IF IT EXIST
                var areaQuery = await (from area in _context.SSv3_GAL_AREATBL where area.isActive == 1 && area.Id == areaId select area).FirstOrDefaultAsync();
                if (areaQuery == null)
                {
                    TempData["EditError"] = oldAreaName;
                    return RedirectToAction("Index");
                }

                areaQuery.AreaName = newAreaName;
                areaQuery.EditByUserName = currentUser;
                areaQuery.DateEdited = DateTime.Now;
                _context.SaveChanges();

                TempData["EditSuccess"] = oldAreaName;
                TempData["EditSuccessNewName"] = newAreaName;
                return RedirectToAction("Index");

            }
            catch
            {
                return RedirectToAction("AreaEditError", "Errors");
            }
        }

        //DELETE THE AREA
        [HttpPost]
        public async Task<IActionResult> DeleteArea(IFormCollection formData)
        {
            string areaName = formData["DeleteAreaName"].ToString();

            try
            {
                int areaId = int.Parse(formData["AreaId"].ToString());

                //FOR ADMIN ACCESS ONLY
                string currentUser = User.Identity.Name.Substring(5).ToLower();
                var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

                //If USER IS NOT IN THE USERTABLE ERROR OR ADMIN
                if (userQuery == null || userQuery.AccessType.ToLower() != "admin")
                {
                    return RedirectToAction("Error404", "Errors");
                }

                //Check if the edit area is used in an active item request
                var requestQuery = await (from request in _context.SSv3_ITEM_REQUESTTBL where request.isActive == 1 && request.AreaName.ToLower() == areaName.ToLower() select request).FirstOrDefaultAsync();
                if (requestQuery != null)
                {
                    TempData["DeleteWarning"] = areaName;
                    return RedirectToAction("Index");
                }

                //DELETE AREA IF IT EXIST
                var areaQuery = await (from area in _context.SSv3_GAL_AREATBL where area.isActive == 1 && area.Id == areaId select area).FirstOrDefaultAsync();
                if (areaQuery == null)
                {
                    TempData["DeleteError"] = areaName;
                    return RedirectToAction("Index");
                }

                areaQuery.isActive = 0;
                areaQuery.DeleteByUserName = currentUser;
                areaQuery.DateDeleted = DateTime.Now;
                _context.SaveChanges();

                TempData["DeleteSuccess"] = areaName;
                return RedirectToAction("Index");

            }
            catch
            {
                return RedirectToAction("AreaDeleteError", "Errors");
            }
        }
    }
}
