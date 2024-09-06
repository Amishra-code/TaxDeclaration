using Employee_Management_System.Data;
using Employee_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;

namespace Employee_Management_System.Controllers
{
    [Authorize(Roles="Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        //Get list of All the Tax forms which have Requested for Unfreeze, in case of no search field, all the list will appear, in case of search fileds Search() will triggered and will pass filteredList
        public IActionResult Notifications(IEnumerable<Tax>? filteredList = null)
        {
            IEnumerable<Tax> taxList = _db.Taxes.Where(tax => tax.UnfreezeReason != null);

            if (filteredList == null && TempData.ContainsKey("FilteredList"))
            {
                string filteredListJson = TempData["FilteredList"] as string;
                if (!string.IsNullOrEmpty(filteredListJson))
                {
                    filteredList = JsonConvert.DeserializeObject<IEnumerable<Tax>>(filteredListJson);
                }
            }

            if (filteredList != null)
            {
                filteredList = filteredList.Where(tax => tax.UnfreezeReason != null);
                return View(filteredList);
            }
            else
            {
                return View(taxList);
            }
        }

        //When Unfreeze button is triggered in the Notifications page, frozen field will get set to false and Unfreeze REason will get back to being null
        public IActionResult Unfreeze(int TaxId)
        {
            if (TaxId == 0)
            {
                return NotFound();
            }

            var taxListFromDb = _db.Taxes.Find(TaxId);

            if (taxListFromDb == null)
            {
                return NotFound();
            }

            taxListFromDb.Frozen = false;
            taxListFromDb.UnfreezeReason = null;
            taxListFromDb.Status = "Draft";

            try
            {
                _db.Taxes.Update(taxListFromDb);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


            return RedirectToAction("Notifications");
        }

        //On clicking the view icon present in the table of Index page, admin will be able to see any particular Tax Form
        public async Task<IActionResult> ViewEmployeeDataAsync(int TaxId)
        {
            var taxList = _db.Taxes.Find(TaxId);
            ApplicationUser user = await GetCurrentUserAsync();
            ViewBag.Pan = user.Pan;
            ViewBag.Address = user.Address;
            return View(taxList);
        }

        //On clicking Accepted
        public IActionResult Accepted(int TaxId)
        {
            if (TaxId == 0)
            {
                return NotFound();
            }

            var taxListFromDb = _db.Taxes.Find(TaxId);

            if (taxListFromDb == null)
            {
                return NotFound();
            }
            
            if(taxListFromDb.UnfreezeReason == null)
            {
                taxListFromDb.Frozen = true;
                taxListFromDb.Status = "Accepted";
                taxListFromDb.Accepted = true;
                try
                {
                    _db.Taxes.Update(taxListFromDb);
                    _db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            
            return RedirectToAction("Search");
        }

        //On clicking Rejected
        public IActionResult Rejected(int TaxId)
        {
            if (TaxId == 0)
            {
                return NotFound();
            }

            var taxListFromDb = _db.Taxes.Find(TaxId);

            if (taxListFromDb == null && taxListFromDb.UnfreezeReason != null)
            {
                return NotFound();
            }
            if (taxListFromDb.UnfreezeReason == null)
            {
                taxListFromDb.Frozen = true;
                taxListFromDb.Status = "Rejected";
                taxListFromDb.Rejected = true;
                try
                {
                    _db.Taxes.Update(taxListFromDb);
                    _db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            
            return RedirectToAction("Search");
        }

        //On searching for any particular Tax form either in the Index page
        public IActionResult Search(string FinancialYear=null, string DeclarationStatus=null, string EmployeeName=null, string EmpId=null,int currentPage=1)
        {
            IEnumerable<Tax> filteredList = _db.Taxes;

            if (FinancialYear != null)
            {
                filteredList = filteredList.Where(tax => tax.FinancialYear == FinancialYear);
            }
            if (DeclarationStatus != null)
            {
                filteredList = filteredList.Where(tax => tax.Status == DeclarationStatus);
            }
            if (EmployeeName != null)
            {
                filteredList = filteredList.Where(tax => tax.EmployeeName == EmployeeName);
            }
            if (EmpId != null)
            {
                filteredList = filteredList.Where(tax => tax.EmpId == EmpId);
            }
            Pagination pag = new Pagination();
            int totalFile = filteredList.Count();
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling(totalFile/(double)pageSize);
            filteredList = filteredList.Skip((currentPage-1)*pageSize).Take(pageSize);

            pag.Form = filteredList;
            pag.CurrentPage = currentPage;
            pag.TotalPage = totalPages;
            pag.PageSize = pageSize;
            pag.Status = DeclarationStatus;
            pag.EmpId = EmpId;
            pag.EmployeeName = EmployeeName;
            return View(pag);
        }

        //On searching for any particular Tax form either in the Notifications page
        public IActionResult SearchInNotifications(string FinancialYear, string EmployeeName, string EmpId)
        {
            IEnumerable<Tax> filteredList = _db.Taxes;
            if (FinancialYear != null)
            {
                filteredList = filteredList.Where(tax => tax.FinancialYear == FinancialYear);
            }
            if (EmployeeName != null)
            {
                filteredList = filteredList.Where(tax => tax.EmployeeName == EmployeeName);
            }
            if (EmpId != null)
            {
                filteredList = filteredList.Where(tax => tax.EmpId == EmpId);
            }

            filteredList = filteredList.ToList();
            string filteredListJson = JsonConvert.SerializeObject(filteredList);
            TempData["FilteredList"] = filteredListJson;
            return RedirectToAction("Notifications");
        }
    }
}
