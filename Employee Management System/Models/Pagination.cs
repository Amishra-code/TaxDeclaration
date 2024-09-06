namespace Employee_Management_System.Models
{
    public class Pagination
    {
        public IEnumerable<Tax> Form { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPage { get; set; }

        public string FinacialYear { get; set; }
        public string EmpId { get; set; }

        public string EmployeeName { get; set; }
        public string Status { get; set; }
    }
}
