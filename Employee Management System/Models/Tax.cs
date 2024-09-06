using System.ComponentModel.DataAnnotations;

namespace Employee_Management_System.Models
{
    public class Tax
    {
        [Key]
        public int TaxId { get; set; }
        public string EmpId { get; set; }
        public string EmployeeUserName { get; set; }
        public string EmployeeName { get; set; }
        public string FinancialYear { get; set; }
        public decimal? OtherIncome { get; set; }
        public decimal? SukanyaSamriddhiAccount { get; set; }
        public decimal? PPF_NSC_ULIP { get; set; }
        public decimal? LifeInsurancePremium { get; set; }
        public decimal? TuitionFee { get; set; }
        public decimal? BankFixedDeposit { get; set; }
        public decimal? PrincipalAmountHousingLoan { get; set; }
        [Range(1000, 50000, ErrorMessage = "The value for National Pension Scheme must be between 0 and 50000.")]
        public decimal? NationalPensionScheme { get; set; }
        public decimal? HigherEducationLoan { get; set; }
        public decimal? InterestHousingLoan { get; set; }
        public decimal? HouseRent { get; set; }
        public decimal? TDS { get; set; }
        public decimal? MedicalClaim { get; set; }
        [Range(1000, 50000, ErrorMessage = "The value for NHealth Checkup for family must be between 0 and 50000.")]
        public decimal? HealthCheckup { get; set; }
        public bool LTAReimbursement { get; set; } = false;
        public bool EducationAllowanceReimbursement { get; set; } = false;
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public bool Frozen {  get; set; }
        public bool isSubmit { get; set; }
        public bool Accepted { get; set; }
        public bool Rejected { get; set; }
        public string? UnfreezeReason { get; set; }
    }
}
