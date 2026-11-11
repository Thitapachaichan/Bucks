using System;
using System.Globalization;
using System.IO;

namespace BucksLoan
{
    // Fixed limits used for input validation and table layout
    static class Limits
    {
        public const double MaxPrincipal = 1000000000;
        public const double MaxRate = 100;
        public const int MaxMonths = 600;
        public const int NameWidth = 16;
        public const int PageSize = 20;      // schedule rows shown before pausing
    }

    class Program
    {
        // ===== Main: controls the program and calls every function directly =====
        static void Main(string[] args)
        {
            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.InputEncoding = System.Text.Encoding.UTF8;      // allow Thai bank names to be typed
            }
            catch (IOException)                                         // no console attached: keep defaults
            {
            }
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;   // same number format on every machine
            bool running = true;

            while (running)                                              // MENU LOOP
            {
                Display.ShowMenu();
                int choice = InputHelper.ReadInt("Select a menu option (1-4): ", 1, 4);

                switch (choice)                                          // SWITCH CASE main menu
                {
                    case 1:
                    {
                        Console.WriteLine("\n--- Loan Calculation ---");
                        Console.WriteLine("Interest method: 1=Flat Rate  2=Reducing Balance");
                        int method = InputHelper.ReadInt("Select method: ", 1, 2);
                        Console.WriteLine("Loan type: 1=Home 2=Car 3=Personal 4=Other");
                        int type = InputHelper.ReadInt("Select type: ", 1, 4);
                        Console.WriteLine("(Approximate interest rate " + LoanCalculator.SuggestRate(type) + "% per year)");

                        double principal = InputHelper.ReadDouble("Principal (THB): ", 1, Limits.MaxPrincipal);
                        double rate = InputHelper.ReadDouble("Annual interest rate (%): ", 0, Limits.MaxRate);
                        int months = InputHelper.ReadInt("Repayment period (months): ", 1, Limits.MaxMonths);

                        double monthly = 0;
                        string methodName = "";

                        switch (method)                                  // SWITCH CASE choose calculation method
                        {
                            case 1:
                                monthly = LoanCalculator.CalcFlatMonthly(principal, rate, months);
                                methodName = "Flat Rate";
                                break;
                            case 2:
                                monthly = LoanCalculator.CalcReducingMonthly(principal, rate, months);
                                methodName = "Reducing Balance";
                                break;
                        }

                        int showSchedule = InputHelper.ReadInt("Show repayment schedule? (1=Yes, 0=No): ", 0, 1);
                        if (showSchedule == 1)                           // long schedules are optional
                        {
                            Display.PrintSchedule(principal, rate, months, monthly, method);
                        }
                        Display.PrintSummary(principal, monthly, months);

                        if (method == 1)                                 // IF-ELSE: flat rate looks cheaper than it is
                        {
                            double effective = LoanCalculator.CalcEffectiveRate(principal, monthly, months);
                            Console.WriteLine("Effective annual rate : " + effective.ToString("N2") + " % (nominal " + rate + " %)");
                        }
                        else
                        {
                            Console.WriteLine("Effective annual rate : " + rate.ToString("N2") + " %");
                        }

                        int save = InputHelper.ReadInt("\nSave result to file? (1=Yes, 0=No): ", 0, 1);
                        if (save == 1)
                        {
                            FileManager.SaveRecord(methodName,
                                "Principal " + principal.ToString("N2") + " | Interest " + rate + "% | "
                                + months + " months | Installment " + monthly.ToString("N2"));
                        }
                        break;
                    }

                    case 2:
                    {
                        Console.WriteLine("\n--- Loan Comparison ---");
                        Console.WriteLine("Interest method for all loans: 1=Flat Rate  2=Reducing Balance");
                        int cmpMethod = InputHelper.ReadInt("Select method: ", 1, 2);
                        int n = InputHelper.ReadInt("Number of loans to compare (2-5): ", 2, 5);

                        string[] names = new string[n];                  // 1D ARRAY: loan names
                        double[,] loanData = new double[n, 3];           // 2D ARRAY: [row=loan, column 0=principal 1=interest 2=months]

                        InputHelper.ReadLoans(names, loanData);          // read data into every cell

                        Display.PrintCompareHeader();
                        int bestIndex = 0;
                        double bestRate = 0;
                        double bestInterest = 0;
                        for (int i = 0; i < n; i++)
                        {
                            double pay = 0;
                            switch (cmpMethod)                           // SWITCH CASE method used for every loan
                            {
                                case 1:
                                    pay = LoanCalculator.CalcFlatMonthly(loanData[i, 0], loanData[i, 1], (int)loanData[i, 2]);
                                    break;
                                case 2:
                                    pay = LoanCalculator.CalcReducingMonthly(loanData[i, 0], loanData[i, 1], (int)loanData[i, 2]);
                                    break;
                            }
                            double interest = pay * loanData[i, 2] - loanData[i, 0];
                            double effRate = LoanCalculator.CalcEffectiveRate(loanData[i, 0], pay, (int)loanData[i, 2]);
                            Display.PrintLoanRow(names[i], pay, interest, effRate);

                            if (i == 0 || effRate < bestRate)            // compare by effective rate, not by total baht
                            {
                                bestIndex = i;
                                bestRate = effRate;
                                bestInterest = interest;
                            }
                        }
                        Console.WriteLine("\n(Ranked by effective annual rate, so loans with different amounts/terms are comparable)");
                        Console.WriteLine("Lowest-rate loan: " + names[bestIndex]
                                          + " (" + bestRate.ToString("N2") + " % per year, total interest "
                                          + bestInterest.ToString("N2") + " THB)");

                        int save2 = InputHelper.ReadInt("\nSave result to file? (1=Yes, 0=No): ", 0, 1);
                        if (save2 == 1)
                        {
                            FileManager.SaveRecord("Comparison of " + n + " loans",
                                "Lowest rate: " + names[bestIndex] + " | Effective rate " + bestRate.ToString("N2")
                                + "% | Total interest " + bestInterest.ToString("N2"));
                        }
                        break;
                    }

                    case 3:
                        FileManager.ShowHistory();
                        break;

                    case 4:
                        running = false;                                 // the loop ends by itself, no Environment.Exit needed
                        Console.WriteLine("Thank you for using Bucks (closing program...)");
                        System.Threading.Thread.Sleep(1500);
                        break;
                }
            }
        }
    }

    // Module for user input + validation
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
                    Console.WriteLine("  Please enter a whole number between " + min + " and " + max);
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
                    Console.WriteLine("  Please enter a number between " + min + " and " + max);
                }
            }
        }

        // Read loan data into the 1D array (names) and 2D array (numbers), filling every cell
        public static void ReadLoans(string[] names, double[,] loanData)
        {
            for (int i = 0; i < names.Length; i++)
            {
                Console.WriteLine("\nLoan " + (i + 1));
                Console.Write("Name/Bank: ");
                string text = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(text))                     // null or empty -> default name
                {
                    names[i] = "Loan " + (i + 1);
                }
                else
                {
                    names[i] = text.Trim();
                    if (names[i].Length > Limits.NameWidth)              // keep the table aligned
                    {
                        names[i] = names[i].Substring(0, Limits.NameWidth);
                    }
                }
                loanData[i, 0] = ReadDouble("Principal (THB): ", 1, Limits.MaxPrincipal);
                loanData[i, 1] = ReadDouble("Annual interest rate (%): ", 0, Limits.MaxRate);
                loanData[i, 2] = ReadInt("Repayment period (months): ", 1, Limits.MaxMonths);
            }
        }
    }

    // Module for calculation formulas
    static class LoanCalculator
    {
        // Flat Rate: interest is calculated on the full principal for the entire contract
        public static double CalcFlatMonthly(double principal, double annualRate, int months)
        {
            double totalInterest = principal * (annualRate / 100) * (months / 12.0);
            return (principal + totalInterest) / months;
        }

        // Reducing balance: fixed installment using the EMI formula
        public static double CalcReducingMonthly(double principal, double annualRate, int months)
        {
            double r = annualRate / 100 / 12;                  // monthly interest rate
            if (r == 0)                                        // avoid division by zero
            {
                return principal / months;
            }
            double factor = Math.Pow(1 + r, months);
            return principal * r * factor / (factor - 1);
        }

        // Effective annual rate: the reducing-balance rate that gives the same installment (bisection search)
        public static double CalcEffectiveRate(double principal, double monthly, int months)
        {
            if (monthly * months <= principal)                 // no interest at all
            {
                return 0;
            }
            double low = 0;
            double high = 1;                                   // monthly rate between 0% and 100%
            for (int i = 0; i < 100; i++)                      // LOOP: narrow the range
            {
                double mid = (low + high) / 2;
                double pay = CalcReducingMonthly(principal, mid * 12 * 100, months);
                if (pay < monthly)
                {
                    low = mid;
                }
                else
                {
                    high = mid;
                }
            }
            return (low + high) / 2 * 12 * 100;
        }

        // Approximate interest rate by loan type
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

    // Module for on-screen display
    static class Display
    {
        public static void ShowMenu()
        {
            Console.WriteLine();
            Console.WriteLine("===== Bucks: Interest Calculator and Repayment Planner =====");
            Console.WriteLine("1. Calculate loan (Flat Rate / Reducing Balance)");
            Console.WriteLine("2. Compare multiple loans");
            Console.WriteLine("3. View saved history");
            Console.WriteLine("4. Exit program");
        }

        // Repayment schedule for both methods (method 1 = Flat Rate, 2 = Reducing Balance)
        public static void PrintSchedule(double principal, double annualRate, int months, double payment, int method)
        {
            double r = annualRate / 100 / 12;
            double balance = principal;

            Console.WriteLine();
            Console.WriteLine("No. | " + "Payment".PadLeft(16) + " | " + "Interest".PadLeft(16)
                + " | " + "Principal".PadLeft(16) + " | " + "Balance".PadLeft(16));
            Console.WriteLine("-------------------------------------------------------------------------------");

            for (int i = 1; i <= months; i++)                  // LOOP through every installment
            {
                double interest;
                if (method == 1)                               // Flat Rate: same interest every month
                {
                    interest = principal * r;
                }
                else                                           // Reducing balance: interest on remaining balance
                {
                    interest = balance * r;
                }
                double principalPaid = payment - interest;     // portion that reduces the principal
                balance = balance - principalPaid;
                if (balance < 0.005)                           // prevent negative decimal remainder
                {
                    balance = 0;
                }
                Console.WriteLine(i.ToString().PadLeft(3) + " | "
                    + payment.ToString("N2").PadLeft(16) + " | "
                    + interest.ToString("N2").PadLeft(16) + " | "
                    + principalPaid.ToString("N2").PadLeft(16) + " | "
                    + balance.ToString("N2").PadLeft(16));

                if (i % Limits.PageSize == 0 && i < months)    // pause so long schedules can be read
                {
                    Console.Write("-- Press Enter to continue --");
                    Console.ReadLine();
                }
            }
        }

        public static void PrintSummary(double principal, double monthly, int months)
        {
            double total = monthly * months;
            Console.WriteLine();
            Console.WriteLine("Monthly installment   : " + monthly.ToString("N2") + " THB");
            Console.WriteLine("Total payment         : " + total.ToString("N2") + " THB");
            Console.WriteLine("Total interest        : " + (total - principal).ToString("N2") + " THB");
        }

        public static void PrintCompareHeader()
        {
            Console.WriteLine();
            Console.WriteLine("Name".PadRight(Limits.NameWidth) + " | " + "Monthly Pay".PadLeft(16)
                + " | " + "Total Interest".PadLeft(18) + " | " + "Eff. Rate %".PadLeft(11));
            Console.WriteLine("----------------------------------------------------------------------");
        }

        public static void PrintLoanRow(string name, double pay, double interest, double effRate)
        {
            Console.WriteLine(name.PadRight(Limits.NameWidth) + " | "
                + pay.ToString("N2").PadLeft(16) + " | "
                + interest.ToString("N2").PadLeft(18) + " | "
                + effRate.ToString("N2").PadLeft(11));
        }
    }

    // File management module (File I/O): save and read history
    static class FileManager
    {
        // Stored next to the program so the location is always the same
        private static readonly string FileName = Path.Combine(AppContext.BaseDirectory, "history.txt");

        // Write to the end of the file (true = append)
        public static void SaveRecord(string title, string detail)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(FileName, true))
                {
                    string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                    writer.WriteLine(time + " | " + title + " | " + detail);
                }
                Console.WriteLine("Saved to file " + FileName + " successfully");
            }
            catch (IOException)
            {
                Console.WriteLine("Unable to save file");
            }
            catch (UnauthorizedAccessException)                // read-only folder / no permission
            {
                Console.WriteLine("Unable to save file (no permission)");
            }
        }

        // Read the file line by line and display it
        public static void ShowHistory()
        {
            Console.WriteLine("\n--- Calculation History ---");
            if (!File.Exists(FileName))
            {
                Console.WriteLine("No saved history yet");
            }
            else
            {
                try
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
                            Console.WriteLine("File is empty");
                        }
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("Unable to read file");
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Unable to read file (no permission)");
                }
            }
        }
    }
}