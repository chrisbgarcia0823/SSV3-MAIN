using Microsoft.AspNetCore.Mvc;
using ShopSuppliesV3.Data;
using ShopSuppliesV3.ViewModel;
using ShopSuppliesV3.Models;
using Microsoft.EntityFrameworkCore;

namespace ShopSuppliesV3.Controllers
{
    public class BUController : Controller
    {
        private readonly ShopSuppliesV3Context _context;

        public BUController(ShopSuppliesV3Context context)
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
            var query = from bu in _context.SSv3_GAL_BUTBL
                        where bu.isActive == 1
                        join user in _context.SSv3_GAL_USERTBL on bu.AddedByUserName equals user.UserName
                        where user.isActive == 1
                        orderby bu.Id descending
                        select new BU
                        {
                            Id = bu.Id,
                            BUname = bu.BUname,
                            AddedByName = user.Name,
                            DateAdded = bu.DateAdded
                        };

            var buList = await query.ToListAsync();

            return View(buList);
        }


        //CREATE NEW BU
        [HttpPost]
        public async Task<IActionResult> CreateBu(IFormCollection formData)
        {
            string buName = formData["BuName"].ToString();

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

                //CHECK IF BU ALREADY EXIST
                string buQuery = await (from bu in _context.SSv3_GAL_BUTBL where bu.isActive == 1 && bu.BUname.ToLower() == buName.ToLower() select bu.BUname).FirstOrDefaultAsync();
                if (!string.IsNullOrEmpty(buQuery))
                {
                    TempData["BuExist"] = buName;
                    return RedirectToAction("Index");
                }

                //CREATE NEW BU IF NOT EXISTING
                SSv3_GAL_BUTBL newBu = new SSv3_GAL_BUTBL
                {
                    BUname = buName,
                    AddedByUserName = currentUser,
                    DateAdded = DateTime.Now,
                    isActive = 1
                };

                _context.Add(newBu);
                _context.SaveChanges();

                TempData["BuAddSuccess"] = buName;
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("BuAddingError", "Errors");
            }
        }

        //EDIT EXISTING BU
        [HttpPost]
        public async Task<IActionResult> EditBu(IFormCollection formData)
        {
            string oldBuName = formData["OldBuName"].ToString();
            string newBuName = formData["NewBuName"].ToString();
            try
            {
                int buId = int.Parse(formData["EditBuId"].ToString());

                //FOR ADMIN ACCESS ONLY
                string currentUser = User.Identity.Name.Substring(5).ToLower();
                var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

                //If USER IS NOT IN THE USERTABLE ERROR OR ADMIN
                if (userQuery == null || userQuery.AccessType.ToLower() != "admin")
                {
                    return RedirectToAction("Error404", "Errors");
                }

                //Check if the edit BU is used in an active item
                var itemQuery = await (from item in _context.SSv3_GAL_ITEMTBL where item.isActive == 1 && item.BUName.ToLower() == oldBuName.ToLower() select item).FirstOrDefaultAsync();
                if (itemQuery != null)
                {
                    TempData["EditWarning"] = oldBuName;
                    return RedirectToAction("Index");
                }

                //Check if the edit BU is used in an active user
                var userQueryCheck = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.BUname.ToLower() == oldBuName.ToLower() select user).FirstOrDefaultAsync();
                if (userQueryCheck != null)
                {
                    TempData["EditWarning"] = oldBuName;
                    return RedirectToAction("Index");
                }

                //Check if the edit bu has existing name (duplicate Entry)
                var checkDuplicateQuery = await (from bu in _context.SSv3_GAL_BUTBL where bu.isActive == 1 && bu.BUname.ToLower() == newBuName.ToLower() select bu).FirstOrDefaultAsync();
                if (checkDuplicateQuery != null)
                {
                    TempData["EditWarningDuplicate"] = newBuName;
                    return RedirectToAction("Index");
                }

                //EDIT BU IF IT EXIST
                var buQuery = await (from bu in _context.SSv3_GAL_BUTBL where bu.isActive == 1 && bu.Id == buId select bu).FirstOrDefaultAsync();
                if (buQuery == null)
                {
                    TempData["EditError"] = oldBuName;
                    return RedirectToAction("Index");
                }

                buQuery.BUname = newBuName;
                buQuery.EditByUserName = currentUser;
                buQuery.DateEdited = DateTime.Now;
                _context.SaveChanges();

                TempData["EditSuccess"] = oldBuName;
                TempData["EditSuccessNewName"] = newBuName;
                return RedirectToAction("Index");

            }
            catch
            {
                return RedirectToAction("BuEditError", "Errors");
            }
        }

        //DELETE THE BU
        [HttpPost]
        public async Task<IActionResult> DeleteBu(IFormCollection formData)
        {
            string buName = formData["DeleteBuName"].ToString();

            try
            {
                int buId = int.Parse(formData["BuId"].ToString());

                //FOR ADMIN ACCESS ONLY
                string currentUser = User.Identity.Name.Substring(5).ToLower();
                var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

                //If USER IS NOT IN THE USERTABLE ERROR OR ADMIN
                if (userQuery == null || userQuery.AccessType.ToLower() != "admin")
                {
                    return RedirectToAction("Error404", "Errors");
                }

                //Check if the edit bu is used in an active item
                var itemQuery = await (from item in _context.SSv3_GAL_ITEMTBL where item.isActive == 1 && item.BUName.ToLower() == buName.ToLower() select item).FirstOrDefaultAsync();
                if (itemQuery != null)
                {
                    TempData["DeleteWarning"] = buName;
                    return RedirectToAction("Index");
                }

                //Check if the edit bu is used in an active user
                var userQueryCheck = await (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.BUname.ToLower() == buName.ToLower() select user).FirstOrDefaultAsync();
                if (userQueryCheck != null)
                {
                    TempData["DeleteWarning"] = buName;
                    return RedirectToAction("Index");
                }

                //DELETE BU IF IT EXIST
                var buQuery = await (from bu in _context.SSv3_GAL_BUTBL where bu.isActive == 1 && bu.Id == buId select bu).FirstOrDefaultAsync();
                if (buQuery == null)
                {
                    TempData["DeleteError"] = buName;
                    return RedirectToAction("Index");
                }

                buQuery.isActive = 0;
                buQuery.DeleteByUserName = currentUser;
                buQuery.DateDeleted = DateTime.Now;
                _context.SaveChanges();

                TempData["DeleteSuccess"] = buName;
                return RedirectToAction("Index");

            }
            catch
            {
                return RedirectToAction("BuDeleteError", "Errors");
            }
        }
    }
}
