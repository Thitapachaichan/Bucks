using System.Text;
using BucksWeb.Models;
using BucksWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BucksWeb.Pages
{
    // Integration: หน้านี้นำทุกโมดูลมารวมเป็นระบบเดียว
    // Validator -> Calculator -> Analyzer -> FileService
    public class IndexModel : PageModel
    {
        // ค่าที่รับจากฟอร์ม
        [BindProperty] public double Principal { get; set; } = 100000;
        [BindProperty] public double AnnualRate { get; set; } = 12;
        [BindProperty] public int Months { get; set; } = 24;
        [BindProperty] public int LoanType { get; set; } = 2;
        [BindProperty] public double ExtraPayment { get; set; } = 0;

        // ค่าที่ส่งไปแสดงผล
        public LoanResult Result { get; set; }
        public double[,] Yearly { get; set; }      // 2D array สรุปรายปี
        public string[,] History { get; set; }     // 2D array ประวัติจากไฟล์
        public string ErrorMessage { get; set; }
        public string Warning { get; set; }
        public double RealRate { get; set; }
        public bool HasExtra { get; set; }
        public int ExtraMonths { get; set; }
        public double ExtraSaved { get; set; }

        public void OnGet()
        {
            LoadHistory();
        }

        // กดปุ่ม "คำนวณ"
        public void OnPost()
        {
            Compute();
            if (Result != null)
            {
                FileService.SaveHistory(LoanCalculator.TypeName(LoanType), Principal,
                                        AnnualRate, Months, Result.MonthlyPayment, Result.TotalInterest);
            }
            LoadHistory();
        }

        // กดปุ่ม "ดาวน์โหลด CSV"
        public IActionResult OnPostDownload()
        {
            Compute();
            if (Result == null)
            {
                LoadHistory();
                return Page();
            }

            string csv = FileService.BuildCsv(Result);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "schedule.csv");
        }

        // กดปุ่ม "ล้างประวัติ"
        public void OnPostClearHistory()
        {
            FileService.ClearHistory();
            LoadHistory();
        }

        private void LoadHistory()
        {
            History = FileService.ReadHistoryTable(10);
        }

        private void Compute()
        {
            ErrorMessage = LoanValidator.Validate(Principal, AnnualRate, Months, LoanType);
            if (ErrorMessage != null)
                return;

            Result = LoanCalculator.Calculate(Principal, AnnualRate, Months, LoanType);
            Yearly = LoanAnalyzer.BuildYearlySummary(Result);

            if (Result.MonthlyPayment > Principal / 2)
                Warning = "ค่างวดสูงมากเมื่อเทียบกับเงินต้น ลองเพิ่มจำนวนงวด";

            if (LoanType == 1)
            {
                RealRate = LoanAnalyzer.EstimateEffectiveRate(Principal, Result.MonthlyPayment, Months);
            }
            else if (ExtraPayment > 0)
            {
                var sim = LoanAnalyzer.SimulateExtra(Principal, AnnualRate, Result.MonthlyPayment, ExtraPayment);
                HasExtra = true;
                ExtraMonths = sim.months;
                ExtraSaved = Result.TotalInterest - sim.totalInterest;
            }
        }
    }
}
