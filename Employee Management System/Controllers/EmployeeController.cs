using Employee_Management_System.Data;
using Employee_Management_System.Models;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Employee_Management_System.Controllers
{
    [Authorize(Roles = "User")]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        public EmployeeController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> DashboardAsync()
        {
            ApplicationUser user = await GetCurrentUserAsync();
            ViewBag.EmpId = user.EmpId;
            ViewBag.EmployeeName = user.Name;
            ViewBag.Pan = user.Pan;
            ViewBag.Address = user.Address;
            return View();
        }

        //Get the Financial Year selection form
        public IActionResult Index()
        {
            return View();
        }
        //Get the TaxForm page after Financial Year selection
        public async Task<IActionResult> TaxFormAsync(string year)
        {
            ApplicationUser user = await GetCurrentUserAsync();
            IEnumerable<Tax> taxList= _db.Taxes.Where(tax => tax.EmployeeUserName == user.UserName);
            if (taxList!=null && taxList.Any(tax => tax.FinancialYear == year))
            {
                Tax singleTax = taxList.FirstOrDefault(t => t.FinancialYear == year);
                if (singleTax != null && singleTax.Frozen==true && (singleTax.Accepted == false && singleTax.Rejected == false))
                    return RedirectToAction("SubmittedForm", singleTax);
                else if (singleTax != null && singleTax.Frozen == false && (singleTax.Accepted == false &&  singleTax.Rejected == false))
                    return RedirectToAction("SavedForm", singleTax);
                else if (singleTax != null && singleTax.Frozen == true && (singleTax.Accepted == true || singleTax.Rejected == true))
                    return RedirectToAction("ViewData", new { TaxId = singleTax.TaxId});
            }
            ViewBag.FinancialYear = year;
            ViewBag.EmpId = user.EmpId;
            ViewBag.EmployeeName = user.Name;
            ViewBag.Pan = user.Pan;
            ViewBag.Address = user.Address;
            ViewBag.EmployeeUserName = user.UserName;
            return View();
        }

        //Get the form in freezed mode as the form for that financial year has been already submitted
        public async Task<IActionResult> SubmittedFormAsync(Tax obj) 
        {
            ApplicationUser user = await GetCurrentUserAsync();
            ViewBag.Pan = user.Pan;
            ViewBag.Address = user.Address;
            return View(obj);
        }

        //Get the form in freezed mode as the form for that financial year has been already saved (status: Draft)
        public async Task<IActionResult> SavedFormAsync(Tax obj)
        {
            ApplicationUser user = await GetCurrentUserAsync();
            ViewBag.Pan = user.Pan;
            ViewBag.Address = user.Address;
            return View(obj);
        }

        //On clicking the save button, this action will get triggered and data will get saved to database but in the unfreeze mode
        [HttpPost]
        public ActionResult Save(Tax obj) 
        {
            DateTime date = DateTime.Now;
            obj.LTAReimbursement = obj.LTAReimbursement==true;
            obj.EducationAllowanceReimbursement = obj.EducationAllowanceReimbursement==true;
            obj.Date = date.Date;
            obj.Status = "Draft";
            obj.Frozen = false;
            obj.isSubmit = false;
            obj.Accepted = false;
            obj.Rejected = false;

            var existingTax = _db.Taxes.Find(obj.TaxId);
            try
            {
                if (existingTax != null)
                {
                    _db.Entry(existingTax).CurrentValues.SetValues(obj);
                }
                else
                {
                    _db.Taxes.Add(obj);
                }
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return RedirectToAction("MyHistory");
        }

        //On clicking the submit button, this action will get triggered and data will get saved to database but in the freeze mode
        public ActionResult Submit(Tax obj)
        {
            DateTime date = DateTime.Now;
            obj.Date = date.Date;
            obj.Status = "Submitted";
            obj.Frozen = true;
            obj.isSubmit = true;
            obj.Accepted = false;
            obj.Rejected = false;

            var existingTax = _db.Taxes.Find(obj.TaxId);
            try
            {
                if (existingTax != null)
                {
                    _db.Entry(existingTax).CurrentValues.SetValues(obj);
                }
                else
                {
                    _db.Taxes.Add(obj);
                }
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
            return RedirectToAction("MyHistory");
        }

        //It will fetch all the forms either submitted or saved by the current user
        public async Task<IActionResult> MyHistoryAsync()
        {
            ApplicationUser user = await GetCurrentUserAsync();
            IEnumerable<Tax> taxList = _db.Taxes.Where(tax => tax.EmployeeUserName == user.UserName);
            return View(taxList);
        }

        //In the MyHistory table, on clicking View Icon for any form, that form will be visible
        public async Task<IActionResult> ViewDataAsync(int TaxId)
        {
            var taxList = _db.Taxes.Find(TaxId);
            ApplicationUser user = await GetCurrentUserAsync();
            ViewBag.Pan = user.Pan;
            ViewBag.Address = user.Address;
            return View(taxList);
        }

        //On clicking Delete in MyHistory Page
        public IActionResult Delete(int TaxId)
        {
            if (TaxId == 0)
            {
                return NotFound();
            }
            var taxObj = _db.Taxes.Find(TaxId);
            try
            {
                if (taxObj != null && taxObj.isSubmit==false)
                {
                    _db.Taxes.Remove(taxObj);
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return RedirectToAction("MyHistory");
        }

        //On clicking the Request To Unfreeze, the form will appear asking for Unfreeze Reason
        public IActionResult RequestForUnfreeze(int TaxId)
        {
            if(TaxId==0)
            {
                return NotFound();
            }
            ViewBag.TaxId = TaxId;
            return View();
        }

        //On clicking "Submit" in the freezed form of Request To Unfreeze, this action will get triggered adding Unfreeze reason to the "UnfreezeReason" column
        [HttpPost]
        public IActionResult RequestForUnfreezeForm(int TaxId, string UnfreezeReason)
        {
            var taxList = _db.Taxes.Find(TaxId);
            try
            {
                if (taxList != null && UnfreezeReason!=null)
                {
                    taxList.UnfreezeReason = UnfreezeReason;
                    _db.Taxes.Update(taxList);
                    _db.SaveChanges();
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.ToString());
            }
            
            return RedirectToAction("MyHistory");
        }
    }
}