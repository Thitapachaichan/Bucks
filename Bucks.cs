using System;
using System.Globalization;
using System.IO;

namespace BucksLoan
{
    class Program
    {
        // ===== Main: ควบคุมโปรแกรม และเรียกทุกฟังก์ชันโดยตรง =====
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            bool running = true;

            while (running)                                              // LOOP เมนู
            {
                Display.ShowMenu();
                int choice = InputHelper.ReadInt("เลือกเมนู (1-4): ", 1, 4);

                switch (choice)                                          // SWITCH CASE เมนูหลัก
                {
                    case 1:
                    {
                        Console.WriteLine("\n--- คำนวณเงินกู้ ---");
                        Console.WriteLine("วิธีคิดดอกเบี้ย: 1=ดอกเบี้ยคงที่ (Flat Rate)  2=ลดต้นลดดอก");
                        int method = InputHelper.ReadInt("เลือกวิธี: ", 1, 2);
                        Console.WriteLine("ประเภทสินเชื่อ: 1=บ้าน 2=รถ 3=ส่วนบุคคล 4=อื่นๆ");
                        int type = InputHelper.ReadInt("เลือกประเภท: ", 1, 4);
                        Console.WriteLine("(อัตราดอกเบี้ยโดยประมาณ " + LoanCalculator.SuggestRate(type) + "% ต่อปี)");

                        double principal = InputHelper.ReadDouble("เงินต้น (บาท): ", 1, 1000000000);
                        double rate = InputHelper.ReadDouble("ดอกเบี้ยต่อปี (%): ", 0, 100);
                        int months = InputHelper.ReadInt("ระยะเวลาผ่อน (เดือน): ", 1, 600);

                        double monthly = 0;
                        string methodName = "";

                        switch (method)                                  // SWITCH CASE เลือกวิธีคิด
                        {
                            case 1:
                                monthly = LoanCalculator.CalcFlatMonthly(principal, rate, months);
                                methodName = "Flat Rate";
                                break;
                            case 2:
                                monthly = LoanCalculator.CalcReducingMonthly(principal, rate, months);
                                methodName = "ลดต้นลดดอก";
                                Display.PrintSchedule(principal, rate, months, monthly);
                                break;
                        }

                        Display.PrintSummary(principal, monthly, months);

                        int save = InputHelper.ReadInt("\nบันทึกผลลงไฟล์? (1=ใช่, 0=ไม่): ", 0, 1);
                        if (save == 1)
                        {
                            FileManager.SaveRecord(methodName,
                                "เงินต้น " + principal.ToString("N2") + " | ดอกเบี้ย " + rate + "% | "
                                + months + " เดือน | ค่างวด " + monthly.ToString("N2"));
                        }
                        break;
                    }

                    case 2:
                    {
                        Console.WriteLine("\n--- เปรียบเทียบสินเชื่อ ---");
                        int n = InputHelper.ReadInt("จำนวนสินเชื่อที่จะเปรียบเทียบ (2-5): ", 2, 5);

                        string[] names = new string[n];                  // ARRAY 1 มิติ: ชื่อสินเชื่อ
                        double[,] loanData = new double[n, 3];           // ARRAY 2 มิติ: [แถว=สินเชื่อ, คอลัมน์ 0=เงินต้น 1=ดอกเบี้ย 2=เดือน]

                        InputHelper.ReadLoans(names, loanData);          // รับข้อมูลเข้าทุกช่อง

                        Display.PrintCompareHeader();
                        int bestIndex = 0;
                        double bestInterest = 0;
                        for (int i = 0; i < n; i++)
                        {
                            double pay = LoanCalculator.CalcReducingMonthly(loanData[i, 0], loanData[i, 1], (int)loanData[i, 2]);
                            double interest = pay * loanData[i, 2] - loanData[i, 0];
                            Display.PrintLoanRow(names[i], pay, interest);

                            if (i == 0 || interest < bestInterest)
                            {
                                bestIndex = i;
                                bestInterest = interest;
                            }
                        }
                        Console.WriteLine("\nจ่ายดอกเบี้ยรวมน้อยที่สุด: " + names[bestIndex]
                                          + " (" + bestInterest.ToString("N2") + " บาท)");

                        int save2 = InputHelper.ReadInt("\nบันทึกผลลงไฟล์? (1=ใช่, 0=ไม่): ", 0, 1);
                        if (save2 == 1)
                        {
                            FileManager.SaveRecord("เปรียบเทียบ " + n + " สินเชื่อ",
                                "ถูกที่สุด: " + names[bestIndex] + " | ดอกเบี้ยรวม " + bestInterest.ToString("N2"));
                        }
                        break;
                    }

                    case 3:
                        FileManager.ShowHistory();
                        break;

                    case 4:
                        running = false;
                        Console.WriteLine("ขอบคุณที่ใช้ระบบ Bucks (กำลังปิดโปรแกรม...)");
                        System.Threading.Thread.Sleep(1500);
                        Environment.Exit(0);
                        break;
                }
            }
        }
    }

    // โมดูลรับข้อมูลจากผู้ใช้ + ตรวจสอบความถูกต้อง
    static class InputHelper
    {
        public static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                string text = Console.ReadLine();
                int value;
                if (int.TryParse(text, out value) && value >= min && value <= max)
                {
                    return value;
                }
                else
                {
                    Console.WriteLine("  กรุณากรอกจำนวนเต็มระหว่าง " + min + " ถึง " + max);
                }
            }
        }

        public static double ReadDouble(string prompt, double min, double max)
        {
            while (true)
            {
                Console.Write(prompt);
                string text = Console.ReadLine();
                double value;
                if (double.TryParse(text, out value) && value >= min && value <= max)
                {
                    return value;
                }
                else
                {
                    Console.WriteLine("  กรุณากรอกตัวเลขระหว่าง " + min + " ถึง " + max);
                }
            }
        }

        // รับข้อมูลสินเชื่อเข้า array 1 มิติ (ชื่อ) และ array 2 มิติ (ตัวเลข) ให้ครบทุกช่อง
        public static void ReadLoans(string[] names, double[,] loanData)
        {
            for (int i = 0; i < names.Length; i++)
            {
                Console.WriteLine("\nสินเชื่อที่ " + (i + 1));
                Console.Write("ชื่อ/ธนาคาร: ");
                names[i] = Console.ReadLine();
                loanData[i, 0] = ReadDouble("เงินต้น (บาท): ", 1, 1000000000);
                loanData[i, 1] = ReadDouble("ดอกเบี้ยต่อปี (%): ", 0, 100);
                loanData[i, 2] = ReadInt("ระยะเวลาผ่อน (เดือน): ", 1, 600);
            }
        }
    }

    // โมดูลสูตรคำนวณ
    static class LoanCalculator
    {
        // Flat Rate: ดอกเบี้ยคิดจากเงินต้นเต็มจำนวนตลอดสัญญา
        public static double CalcFlatMonthly(double principal, double annualRate, int months)
        {
            double totalInterest = principal * (annualRate / 100) * (months / 12.0);
            return (principal + totalInterest) / months;
        }

        // ลดต้นลดดอก: ค่างวดคงที่ตามสูตร EMI
        public static double CalcReducingMonthly(double principal, double annualRate, int months)
        {
            double r = annualRate / 100 / 12;                  // ดอกเบี้ยต่อเดือน
            if (r == 0)                                        // กันหารด้วยศูนย์
            {
                return principal / months;
            }
            double factor = Math.Pow(1 + r, months);
            return principal * r * factor / (factor - 1);
        }

        // อัตราดอกเบี้ยโดยประมาณตามประเภทสินเชื่อ
        public static double SuggestRate(int loanType)
        {
            switch (loanType)
            {
                case 1: return 6.5;
                case 2: return 4.0;
                case 3: return 18.0;
                default: return 10.0;
            }
        }
    }

    // โมดูลแสดงผลบนหน้าจอ
    static class Display
    {
        public static void ShowMenu()
        {
            Console.WriteLine();
            Console.WriteLine("===== Bucks: ระบบคำนวณดอกเบี้ยและวางแผนผ่อนชำระ =====");
            Console.WriteLine("1. คำนวณเงินกู้ (Flat Rate / ลดต้นลดดอก)");
            Console.WriteLine("2. เปรียบเทียบหลายสินเชื่อ");
            Console.WriteLine("3. ดูประวัติที่บันทึกไว้");
            Console.WriteLine("4. ออกจากโปรแกรม");
        }

        public static void PrintSchedule(double principal, double annualRate, int months, double payment)
        {
            double r = annualRate / 100 / 12;
            double balance = principal;

            Console.WriteLine();
            Console.WriteLine("งวด |  ค่างวด  |  ดอกเบี้ย |  ตัดต้น  |  ยอดคงเหลือ");
            Console.WriteLine("----------------------------------------------------");

            for (int i = 1; i <= months; i++)                  // LOOP ทุกงวด
            {
                double interest = balance * r;                 // ดอกเบี้ยเดือนนี้
                double principalPaid = payment - interest;     // ส่วนที่ตัดเงินต้น
                balance = balance - principalPaid;
                if (balance < 0.005)                           // กันเศษทศนิยมติดลบ
                {
                    balance = 0;
                }
                Console.WriteLine(i.ToString().PadLeft(3) + " | "
                    + payment.ToString("N2").PadLeft(8) + " | "
                    + interest.ToString("N2").PadLeft(9) + " | "
                    + principalPaid.ToString("N2").PadLeft(8) + " | "
                    + balance.ToString("N2").PadLeft(11));
            }
        }

        public static void PrintSummary(double principal, double monthly, int months)
        {
            double total = monthly * months;
            Console.WriteLine();
            Console.WriteLine("ค่างวดต่อเดือน : " + monthly.ToString("N2") + " บาท");
            Console.WriteLine("ยอดชำระรวม     : " + total.ToString("N2") + " บาท");
            Console.WriteLine("ดอกเบี้ยรวม    : " + (total - principal).ToString("N2") + " บาท");
        }

        public static void PrintCompareHeader()
        {
            Console.WriteLine();
            Console.WriteLine("ชื่อ             | ค่างวด/เดือน | ดอกเบี้ยรวม");
            Console.WriteLine("---------------------------------------------");
        }

        public static void PrintLoanRow(string name, double pay, double interest)
        {
            Console.WriteLine(name.PadRight(16) + " | "
                + pay.ToString("N2").PadLeft(12) + " | "
                + interest.ToString("N2").PadLeft(11));
        }
    }

    // โมดูลจัดการไฟล์ (File I/O): บันทึกและอ่านประวัติ
    static class FileManager
    {
        private const string FileName = "history.txt";

        // เขียนต่อท้ายไฟล์ (true = append)
        public static void SaveRecord(string title, string detail)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(FileName, true))
                {
                    string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                    writer.WriteLine(time + " | " + title + " | " + detail);
                }
                Console.WriteLine("บันทึกลงไฟล์ " + FileName + " เรียบร้อย");
            }
            catch (IOException)
            {
                Console.WriteLine("ไม่สามารถบันทึกไฟล์ได้");
            }
        }

        // อ่านไฟล์ทีละบรรทัดแล้วแสดงผล
        public static void ShowHistory()
        {
            Console.WriteLine("\n--- ประวัติการคำนวณ ---");
            if (!File.Exists(FileName))
            {
                Console.WriteLine("ยังไม่มีประวัติที่บันทึกไว้");
            }
            else
            {
                using (StreamReader reader = new StreamReader(FileName))
                {
                    string line;
                    int count = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        count++;
                        Console.WriteLine(count + ". " + line);
                    }
                    if (count == 0)
                    {
                        Console.WriteLine("ไฟล์ว่างเปล่า");
                    }
                }
            }
        }
    }
}