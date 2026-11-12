using System;
using System.IO;

// Project: Bucks Loan Program   [Ch.1 Problem solving: คำนวณและเปรียบเทียบเงินกู้]
// Menu: 1) Calculate loan  2) Compare loans  3) View history  4) Exit
class LoanProgram
{
    const string FILE_NAME = "history.txt";   // [Ch.8 File I/O]

    // ---------- [Ch.10 Integration] Main รวมทุกโมดูลผ่านเมนูเดียว ----------
    static void Main()
    {
        bool running = true;
        while (running)                                   // [Ch.5 loop]
        {
            ShowMenu();                                   // [Ch.4 method]
            switch (Console.ReadLine())                   // [Ch.3 switch case]
            {
                case "1": CalculateLoan(); break;
                case "2": CompareLoans(); break;
                case "3": ShowHistory(); break;
                case "4": Console.WriteLine("Cya!"); running = false; break;
                default: Console.WriteLine("Invalid choice. Please select 1-4."); break;
            }
        }
    }

    static void ShowMenu()
    {
        Console.WriteLine("\n===== BUCKS LOAN PROGRAM =====");
        Console.WriteLine("1. Calculate loan");
        Console.WriteLine("2. Compare loans");
        Console.WriteLine("3. View saved history");
        Console.WriteLine("4. Exit");
        Console.Write("Choose: ");
    }

    // ---------- [Ch.9 Module 1] Calculate loan ----------
    static void CalculateLoan()
    {
        Console.WriteLine("\n--- Calculate Loan ---");
        double principal = ReadNumber("Loan amount (baht): ", false);
        double rate = ReadNumber("Interest rate (% per year, 0 allowed): ", true);
        int years = ReadInt("Years (1-50): ", 1, 50);

        double monthly = MonthlyPayment(principal, rate, years);
        double total = monthly * years * 12;

        Console.WriteLine($"Monthly payment : {monthly:N2}");
        Console.WriteLine($"Total payment   : {total:N2}");
        Console.WriteLine($"Total interest  : {total - principal:N2}");

        Console.Write("Save to history? (y/n): ");
        if (Console.ReadLine().Trim().ToLower() == "y")   // [Ch.2 if-else]
        {
            SaveRecord($"Calculate | {principal:N0} baht, {rate}%, {years} yrs | monthly {monthly:N2}");
            Console.WriteLine("Saved.");
        }
        else
        {
            Console.WriteLine("Not saved.");
        }
    }

    // ---------- [Ch.9 Module 2] Compare loans ----------
    static void CompareLoans()
    {
        Console.WriteLine("\n--- Compare Loans ---");
        double principal = ReadNumber("Loan amount (baht): ", false);
        int n = ReadInt("Number of offers to compare (2-5): ", 2, 5);

        string[] names = new string[n];                   // [Ch.6 1D array]
        double[,] result = new double[n, 3];              // [Ch.7 2D array] monthly, total, interest

        for (int i = 0; i < n; i++)                       // [Ch.5 loop]
        {
            Console.Write($"Offer {i + 1} name: ");
            names[i] = Console.ReadLine().Trim();
            if (names[i] == "") names[i] = $"Offer {i + 1}";

            double rate = ReadNumber("  Rate (% per year, 0 allowed): ", true);
            int years = ReadInt("  Years (1-50): ", 1, 50);

            result[i, 0] = MonthlyPayment(principal, rate, years);
            result[i, 1] = result[i, 0] * years * 12;
            result[i, 2] = result[i, 1] - principal;
        }

        Console.WriteLine($"\n{"Name",-15}{"Monthly",15}{"Total",18}{"Interest",18}");
        int best = 0;
        for (int i = 0; i < n; i++)
        {
            Console.WriteLine($"{names[i],-15}{result[i, 0],15:N2}{result[i, 1],18:N2}{result[i, 2],18:N2}");
            if (result[i, 2] < result[best, 2]) best = i;
        }

        Console.WriteLine($"\nBest offer: {names[best]} (lowest interest {result[best, 2]:N2})");
        SaveRecord($"Compare   | {principal:N0} baht, {n} offers | best: {names[best]} (interest {result[best, 2]:N2})");
        Console.WriteLine("Result saved to history.");
    }

    // ---------- [Ch.9 Module 3] View history ----------
    static void ShowHistory()
    {
        Console.WriteLine("\n--- Saved History ---");
        if (!File.Exists(FILE_NAME))                      // [Ch.2 if-else]
        {
            Console.WriteLine("No history yet.");
            return;
        }

        string[] lines = File.ReadAllLines(FILE_NAME);    // [Ch.8 File I/O] -> [Ch.6 1D array]
        for (int i = 0; i < lines.Length; i++)
            Console.WriteLine($"{i + 1}. {lines[i]}");
    }

    // ---------- Helper methods [Ch.4 methods] ----------
    // [Ch.1 Algorithm] M = P*r*(1+r)^n / ((1+r)^n - 1)
    static double MonthlyPayment(double principal, double yearlyRate, int years)
    {
        int months = years * 12;
        double r = yearlyRate / 12 / 100;
        if (r == 0) return principal / months;

        double factor = Math.Pow(1 + r, months);
        return principal * r * factor / (factor - 1);
    }

    // ต้องเป็นตัวเลข ไม่ติดลบ (และไม่เป็น 0 ถ้า allowZero = false)
    static double ReadNumber(string message, bool allowZero)
    {
        double value;
        Console.Write(message);
        while (!double.TryParse(Console.ReadLine(), out value) || value < 0 || (!allowZero && value == 0))
            Console.Write("Invalid. " + message);
        return value;
    }

    // ต้องเป็นจำนวนเต็มในช่วง min-max
    static int ReadInt(string message, int min, int max)
    {
        int value;
        Console.Write(message);
        while (!int.TryParse(Console.ReadLine(), out value) || value < min || value > max)
            Console.Write($"Enter {min}-{max}. " + message);
        return value;
    }

    static void SaveRecord(string text)
    {
        File.AppendAllText(FILE_NAME, DateTime.Now.ToString("yyyy-MM-dd HH:mm") + " | " + text + Environment.NewLine);
    }
}