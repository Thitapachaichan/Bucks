using System;
using System.IO;

// Loan Program: 4 menus
// 1) Calculate loan  2) Compare loans  3) View history  4) Exit
class LoanProgram
{
    const string FILE_NAME = "history.txt";   // [Ch.8 File I/O]

    // ---------- [Ch.1, 3, 5, 9] Main: menu loop + switch-case + modules ----------
    static void Main()
    {
        bool running = true;

        while (running)                        // [Ch.5 loop]
        {
            ShowMenu();                        // [Ch.4 method]
            string choice = Console.ReadLine();

            switch (choice)                    // [Ch.3 switch case]
            {
                case "1": CalculateLoan(); break;
                case "2": CompareLoans(); break;
                case "3": ShowHistory(); break;
                case "4":
                    Console.WriteLine("Cya!");
                    running = false;
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please select 1-4.");
                    break;
            }
        }
    }

    static void ShowMenu()
    {
        Console.WriteLine("\n===== LOAN PROGRAM =====");
        Console.WriteLine("1. Calculate loan");
        Console.WriteLine("2. Compare loans");
        Console.WriteLine("3. View saved history");
        Console.WriteLine("4. Exit");
        Console.Write("Choose: ");
    }

    // ---------- Module 1: Calculate loan ----------
    static void CalculateLoan()
    {
        Console.WriteLine("\n--- Calculate Loan ---");
        double principal = ReadPositive("Loan amount (baht): ");
        double rate = ReadPositive("Interest rate (% per year): ");
        int years = (int)ReadPositive("Years: ");

        // Algorithm result
        double monthly = MonthlyPayment(principal, rate, years);
        double total = monthly * years * 12;
        double interest = total - principal;

        Console.WriteLine($"Monthly payment : {monthly:N2}");
        Console.WriteLine($"Total payment   : {total:N2}");
        Console.WriteLine($"Total interest  : {interest:N2}");

        Console.Write("Save to history? (y/n): ");
        string ans = Console.ReadLine();
        if (ans == "y" || ans == "Y")          // [Ch.2 if-else]
        {
            SaveRecord($"Calculate | {principal:N0} baht, {rate}%, {years} yrs | monthly {monthly:N2}");
            Console.WriteLine("Saved.");
        }
        else
        {
            Console.WriteLine("Not saved.");
        }
    }

    // ---------- Module 2: Compare loans ----------
    static void CompareLoans()
    {
        Console.WriteLine("\n--- Compare Loans ---");
        double principal = ReadPositive("Loan amount (baht): ");
        int n = (int)ReadPositive("Number of offers to compare (2-5): ");

        if (n < 2 || n > 5)
        {
            Console.WriteLine("Please compare between 2 and 5 offers.");
            return;
        }

        string[] names = new string[n];         // [Ch.6 1D array] bank names
        double[,] result = new double[n, 3];    // [Ch.7 2D array] monthly, total, interest

        for (int i = 0; i < n; i++)             // [Ch.5 loop]
        {
            Console.Write($"Offer {i + 1} name: ");
            names[i] = Console.ReadLine();
            double rate = ReadPositive("  Rate (% per year): ");
            int years = (int)ReadPositive("  Years: ");

            double monthly = MonthlyPayment(principal, rate, years);
            result[i, 0] = monthly;
            result[i, 1] = monthly * years * 12;
            result[i, 2] = result[i, 1] - principal;
        }

        // Show table and find best (lowest total interest)
        Console.WriteLine("\nName\t\tMonthly\t\tTotal\t\tInterest");
        int best = 0;
        for (int i = 0; i < n; i++)
        {
            Console.WriteLine($"{names[i]}\t\t{result[i, 0]:N2}\t{result[i, 1]:N2}\t{result[i, 2]:N2}");
            if (result[i, 2] < result[best, 2])
                best = i;
        }

        Console.WriteLine($"\nBest offer: {names[best]} (lowest interest {result[best, 2]:N2})");
        SaveRecord($"Compare   | {principal:N0} baht, {n} offers | best: {names[best]} (interest {result[best, 2]:N2})");
        Console.WriteLine("Result saved to history.");
    }

    // ---------- Module 3: View history ----------
    static void ShowHistory()
    {
        Console.WriteLine("\n--- Saved History ---");

        if (!File.Exists(FILE_NAME))            // [Ch.2 if-else]
        {
            Console.WriteLine("No history yet.");
            return;
        }

        string[] lines = File.ReadAllLines(FILE_NAME);   // [Ch.8 File I/O] -> 1D array
        for (int i = 0; i < lines.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {lines[i]}");
        }
    }

    // ---------- Helper methods [Ch.4, 9] ----------
    // Algorithm: M = P*r*(1+r)^n / ((1+r)^n - 1)
    static double MonthlyPayment(double principal, double yearlyRate, int years)
    {
        int months = years * 12;
        double r = yearlyRate / 12 / 100;

        if (r == 0)
            return principal / months;

        double factor = Math.Pow(1 + r, months);
        return principal * r * factor / (factor - 1);
    }

    static double ReadPositive(string message) // Condition check. Not a number or less than 0? Invalid.
    {
        double value;
        Console.Write(message);
        while (!double.TryParse(Console.ReadLine(), out value) || value <= 0)
        {
            Console.Write("Invalid. " + message);
        }
        return value;
    }

    static void SaveRecord(string text)
    {
        File.AppendAllText(FILE_NAME, DateTime.Now.ToString("yyyy-MM-dd HH:mm") + " | " + text + Environment.NewLine);
    }
}