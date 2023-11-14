using Microsoft.AspNetCore.Mvc;
using ShopSuppliesV3.Data;
using ShopSuppliesV3.ViewModel;
using ShopSuppliesV3.Models;
using Microsoft.EntityFrameworkCore;

namespace ShopSuppliesV3.Controllers
{
    public class UMController : Controller
    {
        private readonly ShopSuppliesV3Context _context;

        public UMController(ShopSuppliesV3Context context)
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

            var query = from uom in _context.SSv3_GAL_UMTBL
                        where uom.isActive == 1
                        join user in _context.SSv3_GAL_USERTBL on uom.AddedBy equals user.UserName
                        where user.isActive == 1
                        orderby uom.Id descending
                        select new UOM
                        {
                            Id = uom.Id,
                            UnitOfMeasure = uom.UnitOfMeasure,
                            AddedByName = user.Name,
                            DateAdded = uom.DateAdded
                        };

            var umList = await query.ToListAsync();

            return View(umList);
        }

        //CREATE NEW UOM
        [HttpPost]
        public async Task<IActionResult> CreateUom(IFormCollection formData)
        {
            string uomName = formData["UomName"].ToString();

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

                //CHECK IF UOM ALREADY EXIST
                string uomQuery = await (from uom in _context.SSv3_GAL_UMTBL where uom.isActive == 1 && uom.UnitOfMeasure.ToLower() == uomName.ToLower() select uom.UnitOfMeasure).FirstOrDefaultAsync();
                if (!string.IsNullOrEmpty(uomQuery))
                {
                    TempData["UomExist"] = uomName;
                    return RedirectToAction("Index");
                }

                //CREATE NEW UOM IF NOT EXISTING
                SSv3_GAL_UMTBL newUOM = new SSv3_GAL_UMTBL
                {
                    UnitOfMeasure = uomName,
                    AddedBy = currentUser,
                    DateAdded = DateTime.Now,
                    isActive = 1
                };

                _context.Add(newUOM);
                _context.SaveChanges();

                TempData["UomAddSuccess"] = uomName;
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("UomAddingError", "Errors");
            }
        }

        //EDIT EXISTING UOM
        [HttpPost]
        public async Task<IActionResult> EditUom(IFormCollection formData)
        {
            string oldUomName = formData["OldUomName"].ToString();
            string newUomName = formData["NewUomName"].ToString();
            try
            {
                int uomId = int.Parse(formData["EditUomId"].ToString());

                //FOR ADMIN ACCESS ONLY
                string currentUser = User.Identity.Name.Substring(5).ToLower();
                var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

                //If USER IS NOT IN THE USERTABLE ERROR OR ADMIN
                if (userQuery == null || userQuery.AccessType.ToLower() != "admin")
                {
                    return RedirectToAction("Error404", "Errors");
                }

                //Check if the edit uom is used in an active item request
                var itemQuery = await (from item in _context.SSv3_GAL_ITEMTBL where item.isActive == 1 && item.UnitOfMeasure.ToLower() == oldUomName.ToLower() select item).FirstOrDefaultAsync();
                if (itemQuery != null)
                {
                    TempData["EditWarning"] = oldUomName;
                    return RedirectToAction("Index");
                }

                //Check if the edit uom has existing name (duplicate Entry)
                var checkDuplicateQuery = await (from uom in _context.SSv3_GAL_UMTBL where uom.isActive == 1 && uom.UnitOfMeasure.ToLower() == newUomName.ToLower() select uom).FirstOrDefaultAsync();
                if (checkDuplicateQuery != null)
                {
                    TempData["EditWarningDuplicate"] = newUomName;
                    return RedirectToAction("Index");
                }

                //EDIT UOM IF IT EXIST
                var uomQuery = await (from uom in _context.SSv3_GAL_UMTBL where uom.isActive == 1 && uom.Id == uomId select uom).FirstOrDefaultAsync();
                if (uomQuery == null)
                {
                    TempData["EditError"] = oldUomName;
                    return RedirectToAction("Index");
                }

                uomQuery.UnitOfMeasure = newUomName;
                uomQuery.EditByUserName = currentUser;
                uomQuery.DateEdited = DateTime.Now;
                _context.SaveChanges();

                TempData["EditSuccess"] = oldUomName;
                TempData["EditSuccessNewName"] = newUomName;
                return RedirectToAction("Index");

            }
            catch
            {
                return RedirectToAction("UomEditError", "Errors");
            }
        }

        //DELETE THE UOM
        [HttpPost]
        public async Task<IActionResult> DeleteUom(IFormCollection formData)
        {
            string uomName = formData["DeleteUomName"].ToString();

            try
            {
                int uomId = int.Parse(formData["UomId"].ToString());

                //FOR ADMIN ACCESS ONLY
                string currentUser = User.Identity.Name.Substring(5).ToLower();
                var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

                //If USER IS NOT IN THE USERTABLE ERROR OR ADMIN
                if (userQuery == null || userQuery.AccessType.ToLower() != "admin")
                {
                    return RedirectToAction("Error404", "Errors");
                }

                //Check if the edit uom is used in an active item
                var itemQuery = await (from item in _context.SSv3_GAL_ITEMTBL where item.isActive == 1 && item.UnitOfMeasure.ToLower() == uomName.ToLower() select item).FirstOrDefaultAsync();
                if (itemQuery != null)
                {
                    TempData["DeleteWarning"] = uomName;
                    return RedirectToAction("Index");
                }

                //DELETE UOM IF IT EXIST
                var uomQuery = await (from uom in _context.SSv3_GAL_UMTBL where uom.isActive == 1 && uom.Id == uomId select uom).FirstOrDefaultAsync();
                if (uomQuery == null)
                {
                    TempData["DeleteError"] = uomName;
                    return RedirectToAction("Index");
                }

                uomQuery.isActive = 0;
                uomQuery.DeleteByUserName = currentUser;
                uomQuery.DateDeleted = DateTime.Now;
                _context.SaveChanges();

                TempData["DeleteSuccess"] = uomName;
                return RedirectToAction("Index");

            }
            catch
            {
                return RedirectToAction("UomDeleteError", "Errors");
            }
        }
    }
}
