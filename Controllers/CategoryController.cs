using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopSuppliesV3.Data;
using ShopSuppliesV3.Models;
using ShopSuppliesV3.ViewModel;

namespace ShopSuppliesV3.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ShopSuppliesV3Context _context;

        public CategoryController(ShopSuppliesV3Context context)
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

            var query = from category in _context.SSv3_ITEM_CATEGORYTBL where category.isActive == 1
                        join user in _context.SSv3_GAL_USERTBL on category.AddedByUserName equals user.UserName where user.isActive == 1 orderby category.Id descending
                        select new Category
                        {
                            Id = category.Id,
                            CategoryName = category.CategoryName,
                            AddedByName = user.Name,
                            DateAdded = category.DateAdded
                        };
            var categoryList = await query.ToListAsync();

            return View(categoryList);
        }

        //CREATE NEW CATEGORY
        [HttpPost]
        public async Task<IActionResult> CreateCategory(IFormCollection formData)
        {
            string categoryName = formData["CategoryName"].ToString();

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

                //CHECK IF CATEGORY ALREADY EXIST
                string categoryQuery = await (from category in _context.SSv3_ITEM_CATEGORYTBL where category.isActive == 1 && category.CategoryName.ToLower() == categoryName.ToLower() select category.CategoryName).FirstOrDefaultAsync();
                if (!string.IsNullOrEmpty(categoryQuery))
                {
                    TempData["CategoryExist"] = categoryName;
                    return RedirectToAction("Index");
                }

                //CREATE NEW CATEGORY IF NOT EXISTING
                SSv3_ITEM_CATEGORYTBL newCategory = new SSv3_ITEM_CATEGORYTBL
                {
                    CategoryName = categoryName,
                    AddedByUserName = currentUser,
                    DateAdded = DateTime.Now,
                    isActive = 1
                };

                _context.Add(newCategory);
                _context.SaveChanges();

                TempData["CategoryAddSuccess"] = categoryName;
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("CategoryAddingError", "Errors");
            }
        }

        //EDIT EXISTING CATEGORY
        [HttpPost]
        public async Task<IActionResult> EditCategory(IFormCollection formData)
        {
            string oldCategoryName = formData["OldCategoryName"].ToString();
            string newCategoryName = formData["NewCategoryName"].ToString();
            try
            {
                int categoryId = int.Parse(formData["EditCategoryId"].ToString());

                //FOR ADMIN ACCESS ONLY
                string currentUser = User.Identity.Name.Substring(5).ToLower();
                var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

                //If USER IS NOT IN THE USERTABLE ERROR OR ADMIN
                if (userQuery == null || userQuery.AccessType.ToLower() != "admin")
                {
                    return RedirectToAction("Error404", "Errors");
                }

                //Check if the edit category is used in an active item request
                var requestQuery = await (from request in _context.SSv3_ITEM_REQUESTTBL where request.isActive == 1 && request.RequestCategory.ToLower() == oldCategoryName.ToLower() select request).FirstOrDefaultAsync();
                if (requestQuery != null)
                {
                    TempData["EditWarning"] = oldCategoryName;
                    return RedirectToAction("Index");
                }

                //Check if the edit category has existing name (duplicate Entry)
                var checkDuplicateQuery = await (from category in _context.SSv3_ITEM_CATEGORYTBL where category.isActive == 1 && category.CategoryName.ToLower() == newCategoryName.ToLower() select category).FirstOrDefaultAsync();
                if (checkDuplicateQuery != null)
                {
                    TempData["EditWarningDuplicate"] = newCategoryName;
                    return RedirectToAction("Index");
                }

                //EDIT CATEGORY IF IT EXIST
                var categoryQuery = await (from category in _context.SSv3_ITEM_CATEGORYTBL where category.isActive == 1 && category.Id == categoryId select category).FirstOrDefaultAsync();
                if (categoryQuery == null)
                {
                    TempData["EditError"] = oldCategoryName;
                    return RedirectToAction("Index");
                }

                categoryQuery.CategoryName = newCategoryName;
                categoryQuery.EditByUserName = currentUser;
                categoryQuery.DateEdited = DateTime.Now;
                _context.SaveChanges();

                TempData["EditSuccess"] = oldCategoryName;
                TempData["EditSuccessNewName"] = newCategoryName;
                return RedirectToAction("Index");

            }
            catch
            {
                return RedirectToAction("CategoryEditError", "Errors");
            }
        }

        //DELETE THE CATEGORY
        [HttpPost]
        public async Task<IActionResult> DeleteCategory(IFormCollection formData)
        {
            string categoryName = formData["DeleteCategoryName"].ToString();

            try
            {
                int categoryId = int.Parse(formData["CategoryId"].ToString());

                //FOR ADMIN ACCESS ONLY
                string currentUser = User.Identity.Name.Substring(5).ToLower();
                var userQuery = (from user in _context.SSv3_GAL_USERTBL where user.isActive == 1 && user.UserName == currentUser select user).FirstOrDefault();

                //If USER IS NOT IN THE USERTABLE ERROR OR ADMIN
                if (userQuery == null || userQuery.AccessType.ToLower() != "admin")
                {
                    return RedirectToAction("Error404", "Errors");
                }

                //Check if the edit category is used in an active item request
                var requestQuery = await (from request in _context.SSv3_ITEM_REQUESTTBL where request.isActive == 1 && request.RequestCategory.ToLower() == categoryName.ToLower() select request).FirstOrDefaultAsync();
                if (requestQuery != null)
                {
                    TempData["DeleteWarning"] = categoryName;
                    return RedirectToAction("Index");
                }

                //DELETE SUPPLIER IF IT EXIST
                var categoryQuery = await (from category in _context.SSv3_ITEM_CATEGORYTBL where category.isActive == 1 && category.Id == categoryId select category).FirstOrDefaultAsync();
                if (categoryQuery == null)
                {
                    TempData["DeleteError"] = categoryName;
                    return RedirectToAction("Index");
                }

                categoryQuery.isActive = 0;
                categoryQuery.DeleteByUserName = currentUser;
                categoryQuery.DateDeleted = DateTime.Now;
                _context.SaveChanges();

                TempData["DeleteSuccess"] = categoryName;
                return RedirectToAction("Index");

            }
            catch
            {
                return RedirectToAction("CategoryDeleteError", "Errors");
            }
        }
    }
}
