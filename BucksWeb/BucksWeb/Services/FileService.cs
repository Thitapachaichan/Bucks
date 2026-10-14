using System.Globalization;
using System.Text;
using BucksWeb.Models;

namespace BucksWeb.Services
{
    // โมดูลไฟล์ (File I/O): เขียน/อ่านประวัติ และสร้าง CSV
    public static class FileService
    {
        private const string HistoryFile = "history.txt";
        private const int HistoryColumns = 7;

        // สร้างข้อความ CSV สำหรับดาวน์โหลด
        public static string BuildCsv(LoanResult result)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Period,Payment,Interest,Principal,Balance");
            for (int i = 0; i < result.InterestArr.Length; i++)
            {
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "{0},{1:F2},{2:F2},{3:F2},{4:F2}",
                    i + 1, result.MonthlyPayment, result.InterestArr[i],
                    result.PrincipalArr[i], result.BalanceArr[i]));
            }
            return sb.ToString();
        }

        // เขียนไฟล์: เพิ่มบรรทัดประวัติการคำนวณต่อท้ายไฟล์ history.txt
        public static void SaveHistory(string typeName, double principal, double rate,
                                       int months, double payment, double totalInterest)
        {
            string line = string.Format(CultureInfo.InvariantCulture,
                "{0:yyyy-MM-dd HH:mm}|{1}|{2:F2}|{3}|{4}|{5:F2}|{6:F2}",
                DateTime.Now, typeName, principal, rate, months, payment, totalInterest);

            using (StreamWriter sw = new StreamWriter(HistoryFile, true))
            {
                sw.WriteLine(line);
            }
        }

        // อ่านไฟล์: คืน array ของบรรทัดล่าสุดไม่เกิน maxLines
        public static string[] ReadHistory(int maxLines)
        {
            if (!File.Exists(HistoryFile))
                return new string[0];

            string[] all = File.ReadAllLines(HistoryFile);
            int start = Math.Max(0, all.Length - maxLines);
            string[] recent = new string[all.Length - start];
            for (int i = start; i < all.Length; i++)
                recent[i - start] = all[i];
            return recent;
        }

        // 2D array: แปลงบรรทัดประวัติเป็นตาราง (แถว = รายการ, คอลัมน์ = ข้อมูล 7 ช่อง)
        public static string[,] ReadHistoryTable(int maxRows)
        {
            string[] lines = ReadHistory(maxRows);
            string[,] table = new string[lines.Length, HistoryColumns];
            for (int i = 0; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split('|');
                for (int j = 0; j < HistoryColumns && j < parts.Length; j++)
                    table[i, j] = parts[j];
            }
            return table;
        }

        // ลบไฟล์ประวัติ
        public static void ClearHistory()
        {
            if (File.Exists(HistoryFile))
                File.Delete(HistoryFile);
        }
    }
}
