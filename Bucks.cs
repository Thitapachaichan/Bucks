using System;

namespace BucksLoan
{
    class Program
    {
        // ===== Main: เรียกใช้ "ทุกฟังก์ชัน" โดยตรงจากที่นี่ =====
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            bool running = true;

            while (running)                                        // LOOP: วนเมนูจนกว่าจะเลือกออก
            {
                ShowMenu();                                        // เรียกฟังก์ชัน
                int choice = ReadInt("เลือกเมนู (1-4): ", 1, 4);   // เรียกฟังก์ชัน

                switch (choice)                                    // SWITCH CASE
                {
                    case 1:
                        Console.WriteLine("\n--- ดอกเบี้ยคงที่ (Flat Rate) ---");
                        double p1 = ReadDouble("เงินต้น (บาท): ", 1, 1000000000);
                        double r1 = ReadDouble("ดอกเบี้ยต่อปี (%): ", 0, 100);
                        int m1 = ReadInt("ระยะเวลาผ่อน (เดือน): ", 1, 600);
                        double monthly1 = CalcFlatMonthly(p1, r1, m1);
                        PrintSummary(p1, monthly1, m1);
                        break;

                    case 2:
                        Console.WriteLine("\n--- ลดต้นลดดอก + ตารางผ่อน ---");
                        Console.WriteLine("ประเภทสินเชื่อ: 1=บ้าน 2=รถ 3=ส่วนบุคคล 4=อื่นๆ");
                        int type = ReadInt("เลือกประเภท: ", 1, 4);
                        Console.WriteLine("(อัตราดอกเบี้ยโดยประมาณ " + SuggestRate(type) + "% ต่อปี)");
                        double p2 = ReadDouble("เงินต้น (บาท): ", 1, 1000000000);
                        double r2 = ReadDouble("ดอกเบี้ยต่อปี (%): ", 0, 100);
                        int m2 = ReadInt("ระยะเวลาผ่อน (เดือน): ", 1, 600);
                        double monthly2 = CalcReducingMonthly(p2, r2, m2);
                        PrintSchedule(p2, r2, m2, monthly2);
                        PrintSummary(p2, monthly2, m2);
                        break;

                    case 3:
                        Console.WriteLine("\n--- เปรียบเทียบสินเชื่อ ---");
                        int n = ReadInt("จำนวนสินเชื่อที่จะเปรียบเทียบ (2-5): ", 2, 5);

                        string[] names = new string[n];            // ARRAY 1 มิติ
                        double[] principals = new double[n];
                        double[] rates = new double[n];
                        int[] months = new int[n];

                        ReadLoans(names, principals, rates, months);   // รับข้อมูลเข้า array ทุกช่อง

                        PrintCompareHeader();
                        int bestIndex = 0;
                        double bestInterest = 0;
                        for (int i = 0; i < n; i++)
                        {
                            double pay = CalcReducingMonthly(principals[i], rates[i], months[i]);
                            double interest = pay * months[i] - principals[i];
                            PrintLoanRow(names[i], pay, interest);

                            if (i == 0 || interest < bestInterest)
                            {
                                bestIndex = i;
                                bestInterest = interest;
                            }
                        }
                        Console.WriteLine("\nจ่ายดอกเบี้ยรวมน้อยที่สุด: " + names[bestIndex]
                                          + " (" + bestInterest.ToString("N2") + " บาท)");
                        break;

                    case 4:
                        running = false;
                        Console.WriteLine("ขอบคุณที่ใช้ระบบ Bucks (กำลังปิดโปรแกรม...)");
                        System.Threading.Thread.Sleep(1500);   // รอ 1.5 วินาทีให้อ่านข้อความทัน
                        Environment.Exit(0);                   // สั่งปิดโปรแกรม/หน้าต่าง terminal ทันที
                        break;
                }
            }
        }

        // ===== แสดงเมนู =====
        static void ShowMenu()
        {
            Console.WriteLine();
            Console.WriteLine("===== Bucks: ระบบคำนวณดอกเบี้ยและวางแผนผ่อนชำระ =====");
            Console.WriteLine("1. คำนวณแบบดอกเบี้ยคงที่ (Flat Rate)");
            Console.WriteLine("2. คำนวณแบบลดต้นลดดอก + ตารางผ่อนชำระ");
            Console.WriteLine("3. เปรียบเทียบหลายสินเชื่อ");
            Console.WriteLine("4. ออกจากโปรแกรม");
        }

        // ===== รับตัวเลขพร้อมตรวจสอบ (วนถามซ้ำจนกว่าจะถูก) =====
        static int ReadInt(string prompt, int min, int max)
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

        static double ReadDouble(string prompt, double min, double max)
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

        // ===== สูตรคำนวณ =====
        // Flat Rate: ดอกเบี้ยคิดจากเงินต้นเต็มจำนวนตลอดสัญญา
        static double CalcFlatMonthly(double principal, double annualRate, int months)
        {
            double totalInterest = principal * (annualRate / 100) * (months / 12.0);
            return (principal + totalInterest) / months;
        }

        // ลดต้นลดดอก: ค่างวดคงที่ตามสูตร EMI
        static double CalcReducingMonthly(double principal, double annualRate, int months)
        {
            double r = annualRate / 100 / 12;                      // ดอกเบี้ยต่อเดือน
            if (r == 0)                                            // กันหารด้วยศูนย์
            {
                return principal / months;
            }
            double factor = Math.Pow(1 + r, months);
            return principal * r * factor / (factor - 1);
        }

        // เสนออัตราดอกเบี้ยโดยประมาณตามประเภทสินเชื่อ
        static double SuggestRate(int loanType)
        {
            switch (loanType)                                      // SWITCH CASE (ทุก case มีคำสั่ง)
            {
                case 1: return 6.5;
                case 2: return 4.0;
                case 3: return 18.0;
                default: return 10.0;
            }
        }

        // ===== ตารางผ่อนชำระ (วนทุกเดือน) =====
        static void PrintSchedule(double principal, double annualRate, int months, double payment)
        {
            double r = annualRate / 100 / 12;
            double balance = principal;

            Console.WriteLine();
            Console.WriteLine("งวด |  ค่างวด  |  ดอกเบี้ย |  ตัดต้น  |  ยอดคงเหลือ");
            Console.WriteLine("----------------------------------------------------");

            for (int i = 1; i <= months; i++)                      // LOOP หลายรอบ
            {
                double interest = balance * r;                     // ดอกเบี้ยเดือนนี้
                double principalPaid = payment - interest;         // ส่วนที่ตัดเงินต้น
                balance = balance - principalPaid;
                if (balance < 0.005)                               // กันเศษทศนิยมติดลบ
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

        // ===== สรุปผล =====
        static void PrintSummary(double principal, double monthly, int months)
        {
            double total = monthly * months;
            Console.WriteLine();
            Console.WriteLine("ค่างวดต่อเดือน : " + monthly.ToString("N2") + " บาท");
            Console.WriteLine("ยอดชำระรวม     : " + total.ToString("N2") + " บาท");
            Console.WriteLine("ดอกเบี้ยรวม    : " + (total - principal).ToString("N2") + " บาท");
        }

        // ===== รับข้อมูลเข้า array ทุกช่อง =====
        static void ReadLoans(string[] names, double[] principals, double[] rates, int[] months)
        {
            for (int i = 0; i < names.Length; i++)
            {
                Console.WriteLine("\nสินเชื่อที่ " + (i + 1));
                Console.Write("ชื่อ/ธนาคาร: ");
                names[i] = Console.ReadLine();
                principals[i] = ReadDouble("เงินต้น (บาท): ", 1, 1000000000);
                rates[i] = ReadDouble("ดอกเบี้ยต่อปี (%): ", 0, 100);
                months[i] = ReadInt("ระยะเวลาผ่อน (เดือน): ", 1, 600);
            }
        }

        static void PrintCompareHeader()
        {
            Console.WriteLine();
            Console.WriteLine("ชื่อ             | ค่างวด/เดือน | ดอกเบี้ยรวม");
            Console.WriteLine("---------------------------------------------");
        }

        static void PrintLoanRow(string name, double pay, double interest)
        {
            Console.WriteLine(name.PadRight(16) + " | "
                + pay.ToString("N2").PadLeft(12) + " | "
                + interest.ToString("N2").PadLeft(11));
        }
    }
}