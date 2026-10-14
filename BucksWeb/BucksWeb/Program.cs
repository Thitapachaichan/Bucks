using BucksWeb.Models;
using BucksWeb.Services;

namespace BucksWeb
{
    public class Program
    {
        // Main(): จุดเริ่มต้นของโปรแกรม
        public static void Main(string[] args)
        {
            // เรียกฟังก์ชันจาก Main() ตอนเริ่มระบบ
            PrintStartupDemo();

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddRazorPages();

            var app = builder.Build();
            app.UseRouting();
            app.MapRazorPages();
            app.Run();
        }

        // แสดงตัวอย่างการคำนวณ และจำนวนประวัติที่อ่านจากไฟล์ ใน Console
        private static void PrintStartupDemo()
        {
            LoanResult demo = LoanCalculator.Calculate(100000, 12, 6, 2);
            Console.WriteLine("=== Bucks demo: 100,000 บาท 12% 6 งวด ===");
            for (int i = 0; i < demo.InterestArr.Length; i++)
            {
                Console.WriteLine($"งวด {i + 1}: ดอกเบี้ย {demo.InterestArr[i]:N2}  เงินต้น {demo.PrincipalArr[i]:N2}  คงเหลือ {demo.BalanceArr[i]:N2}");
            }

            int historyCount = FileService.ReadHistory(1000).Length;
            Console.WriteLine($"ประวัติที่บันทึกไว้ในไฟล์: {historyCount} รายการ");
        }
    }
}
