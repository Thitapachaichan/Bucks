using BucksWeb.Models;

namespace BucksWeb.Services
{
    // โมดูลคำนวณหลัก
    public static class LoanCalculator
    {
        // ฟังก์ชันหลัก: เลือกสูตรด้วย switch case
        public static LoanResult Calculate(double principal, double annualRate, int months, int loanType)
        {
            LoanResult result = new LoanResult();
            result.InterestArr = new double[months];
            result.PrincipalArr = new double[months];
            result.BalanceArr = new double[months];

            switch (loanType)
            {
                case 1:
                    CalcFlat(principal, annualRate, months, result);
                    break;
                case 2:
                    CalcEffective(principal, annualRate, months, result);
                    break;
                default:
                    throw new ArgumentException("ประเภทดอกเบี้ยไม่ถูกต้อง");
            }

            result.TotalInterest = SumArray(result.InterestArr);
            result.TotalPaid = principal + result.TotalInterest;
            return result;
        }

        // ดอกเบี้ยแบบ Flat
        private static void CalcFlat(double principal, double annualRate, int months, LoanResult result)
        {
            double totalInterest = principal * (annualRate / 100) * (months / 12.0);
            result.MonthlyPayment = (principal + totalInterest) / months;
            double balance = principal;

            // Loop + ใส่ข้อมูลเข้า array ทุกช่อง
            for (int i = 0; i < months; i++)
            {
                result.InterestArr[i] = totalInterest / months;
                result.PrincipalArr[i] = principal / months;
                balance -= result.PrincipalArr[i];
                result.BalanceArr[i] = balance < 0.005 ? 0 : balance;
            }
        }

        // ดอกเบี้ยแบบลดต้นลดดอก
        private static void CalcEffective(double principal, double annualRate, int months, LoanResult result)
        {
            double r = annualRate / 12 / 100;   // ดอกเบี้ยต่อเดือน
            if (r == 0)
                result.MonthlyPayment = principal / months;
            else
                result.MonthlyPayment = principal * r / (1 - Math.Pow(1 + r, -months));

            double balance = principal;

            for (int i = 0; i < months; i++)
            {
                result.InterestArr[i] = balance * r;
                result.PrincipalArr[i] = result.MonthlyPayment - result.InterestArr[i];
                balance -= result.PrincipalArr[i];
                result.BalanceArr[i] = balance < 0.005 ? 0 : balance;
            }
        }

        // รวมค่าใน array
        public static double SumArray(double[] arr)
        {
            double sum = 0;
            for (int i = 0; i < arr.Length; i++)
                sum += arr[i];
            return sum;
        }

        // switch case: แปลงรหัสเป็นชื่อประเภท
        public static string TypeName(int loanType)
        {
            switch (loanType)
            {
                case 1: return "Flat";
                case 2: return "ลดต้นลดดอก";
                default: return "ไม่ทราบ";
            }
        }
    }
}
