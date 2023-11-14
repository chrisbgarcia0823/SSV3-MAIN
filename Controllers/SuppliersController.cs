using Microsoft.AspNetCore.Mvc;
using ShopSuppliesV3.Data;
using ShopSuppliesV3.ViewModel;
using ShopSuppliesV3.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ShopSuppliesV3.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly ShopSuppliesV3Context _context;

        public SuppliersController(ShopSuppliesV3Context context)
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

            var query = from supplier in _context.SSv3_GAL_SUPPLIERTBL where supplier.isActive == 1 
                        join user in _context.SSv3_GAL_USERTBL on supplier.AddedByUserName equals user.UserName where user.isActive == 1
                        orderby supplier.Id descending select new Suppliers
                        {
                            Id = supplier.Id,
                            SupplierName = supplier.SupplierName,
                            AddedByName = user.Name,
                            DateAdded = supplier.DateAdded,
                        };

            var supplierList = await query.ToListAsync();

            return View(supplierList);
        }

        //CREATE NEW SUPPLIER
        [HttpPost]
        public async Task<IActionResult> CreateSupplier(IFormCollection formData)
        {
            string supplierName = formData["SupplierName"].ToString();

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

                //CHECK IF SUPPLIER ALREADY EXIST
                string supplierQuery =  await (from supplier in _context.SSv3_GAL_SUPPLIERTBL where supplier.isActive == 1 && supplier.SupplierName.ToLower() == supplierName.ToLower() select supplier.SupplierName).FirstOrDefaultAsync();
                if(!string.IsNullOrEmpty(supplierQuery))
                {
                    TempData["SupplierExist"] = supplierName;
                    return RedirectToAction("Index");
                }

                //CREATE NEW SUPPLIER IF NOT EXISTING
                SSv3_GAL_SUPPLIERTBL newSupplier = new SSv3_GAL_SUPPLIERTBL
                {
                    SupplierName = supplierName,
                    AddedByUserName = User.Identity.Name.Substring(5).ToUpper(),
                    DateAdded = DateTime.Now,
                    isActive = 1
                };

                _context.Add(newSupplier);
                _context.SaveChanges();

                TempData["SupplierAddSuccess"] = supplierName;
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("CreateSupplierError", "Errors");
            }

        }

        //EDIT THE EXISTING SUPPLIER
        [HttpPost]
        public async Task<IActionResult> EditSupplier(IFormCollection formData)
        {
            string oldSupplierName = formData["OldSupplierName"].ToString();
            string newSupplierName = formData["NewSupplierName"].ToString();
            try
            {
                int supplierId = int.Parse(formData["EditSupplierId"].ToString());

                //FOR ADMIN ACCESS ONLY
                string currentUser = User.Identity.Name.Substring(5).ToLower();
                var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

                //If USER IS NOT IN THE USERTABLE ERROR OR ADMIN
                if (userQuery == null || userQuery.AccessType.ToLower() != "admin")
                {
                    return RedirectToAction("Error404", "Errors");
                }

                //Check if the edit supplier is used in an active item
                var itemQuery = await (from item in _context.SSv3_GAL_ITEMTBL where item.isActive == 1 && item.SupplierId == supplierId select item).FirstOrDefaultAsync();
                if (itemQuery != null)
                {
                    TempData["EditWarning"] = oldSupplierName;
                    return RedirectToAction("Index");
                }

                //Check if the edit supplier has existing name (duplicate Entry)
                var checkDuplicateQuery = await (from supplier in _context.SSv3_GAL_SUPPLIERTBL where supplier.isActive == 1 && supplier.SupplierName.ToLower() == newSupplierName.ToLower() select supplier).FirstOrDefaultAsync();
                if (checkDuplicateQuery != null)
                {
                    TempData["EditWarningDuplicate"] = newSupplierName;
                    return RedirectToAction("Index");
                }

                //EDIT SUPPLIER IF IT EXIST
                var supplierQuery = await (from supplier in _context.SSv3_GAL_SUPPLIERTBL where supplier.isActive == 1 && supplier.Id == supplierId select supplier).FirstOrDefaultAsync();
                if (supplierQuery == null)
                {
                    TempData["EditError"] = oldSupplierName;
                    return RedirectToAction("Index");
                }

                supplierQuery.SupplierName = newSupplierName;
                supplierQuery.EditByUserName = currentUser;
                supplierQuery.DateEdited = DateTime.Now;
                _context.SaveChanges();

                TempData["EditSuccess"] = oldSupplierName;
                TempData["EditSuccessNewName"] = newSupplierName;
                return RedirectToAction("Index");

            }
            catch
            {
                return RedirectToAction("SupplierEditError", "Errors");
            }
        }

        //DELETE THE SUPPLIER
        [HttpPost]
        public async Task<IActionResult> DeleteSupplier(IFormCollection formData)
        {
            string supplierName = formData["DeleteSupplierName"].ToString();

            try
            {
                int supplierId = int.Parse(formData["SupplierId"].ToString());

                //FOR ADMIN ACCESS ONLY
                string currentUser = User.Identity.Name.Substring(5).ToLower();
                var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

                //If USER IS NOT IN THE USERTABLE ERROR OR ADMIN
                if (userQuery == null || userQuery.AccessType.ToLower() != "admin")
                {
                    return RedirectToAction("Error404", "Errors");
                }

                //Check if the delete request is used in an active item
                var itemQuery = await (from item in _context.SSv3_GAL_ITEMTBL where item.isActive == 1 && item.SupplierId == supplierId select item).FirstOrDefaultAsync();
                if (itemQuery != null)
                {
                    TempData["DeleteWarning"] = supplierName;
                    return RedirectToAction("Index");
                }

                //DELETE SUPPLIER IF IT EXIST
                var supplierQuery = await (from supplier in _context.SSv3_GAL_SUPPLIERTBL where supplier.isActive == 1 && supplier.Id == supplierId select supplier).FirstOrDefaultAsync();
                if (supplierQuery == null)
                {
                    TempData["DeleteError"] = supplierName;
                    return RedirectToAction("Index");
                }

                supplierQuery.isActive = 0;
                supplierQuery.DeleteByUserName = currentUser;
                supplierQuery.DateDeleted = DateTime.Now;
                _context.SaveChanges();

                TempData["DeleteSuccess"] = supplierName;
                return RedirectToAction("Index");

            }
            catch
            {
                return RedirectToAction("SupplierDeleteError", "Errors");
            }
        }
    }
}
