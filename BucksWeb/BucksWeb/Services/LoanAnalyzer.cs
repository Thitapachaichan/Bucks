using BucksWeb.Models;

namespace BucksWeb.Services
{
    // โมดูลวิเคราะห์ผล (ส่วนต่อยอด)
    public static class LoanAnalyzer
    {
        // 2D array: สรุปรายปี แถว = ปี, คอลัมน์ 0 = ค่างวดรวม, 1 = ดอกเบี้ย, 2 = เงินต้น, 3 = คงเหลือสิ้นปี
        public static double[,] BuildYearlySummary(LoanResult result)
        {
            int months = result.InterestArr.Length;
            int years = (months + 11) / 12;
            double[,] summary = new double[years, 4];

            // nested loop: วนทีละปี แล้ววนทีละเดือนในปีนั้น
            for (int y = 0; y < years; y++)
            {
                int start = y * 12;
                int end = Math.Min(start + 12, months);
                for (int m = start; m < end; m++)
                {
                    summary[y, 0] += result.InterestArr[m] + result.PrincipalArr[m];
                    summary[y, 1] += result.InterestArr[m];
                    summary[y, 2] += result.PrincipalArr[m];
                }
                summary[y, 3] = result.BalanceArr[end - 1];
            }
            return summary;
        }

        // จำลองจ่ายเพิ่มต่อเดือน คืนค่า (จำนวนงวดที่ใช้ปิดหนี้, ดอกเบี้ยรวมใหม่)
        public static (int months, double totalInterest) SimulateExtra(
            double principal, double annualRate, double monthlyPayment, double extra)
        {
            double r = annualRate / 12 / 100;
            double balance = principal;
            double totalInterest = 0;
            int count = 0;

            while (balance > 0.005 && count < 1200)
            {
                double interest = balance * r;
                double pay = monthlyPayment + extra;
                if (pay > balance + interest)     // งวดสุดท้ายจ่ายแค่ที่เหลือ
                    pay = balance + interest;
                balance -= (pay - interest);
                totalInterest += interest;
                count++;
            }
            return (count, totalInterest);
        }

        // ดอกเบี้ยที่แท้จริงของ Flat: หาอัตราต่อเดือน r ที่ทำให้ค่างวดแบบลดต้นลดดอกเท่ากับ Flat (แบ่งครึ่งช่วง)
        public static double EstimateEffectiveRate(double principal, double monthlyPayment, int months)
        {
            double low = 0, high = 1, mid = 0;
            for (int k = 0; k < 70; k++)
            {
                mid = (low + high) / 2;
                double pay = principal * mid / (1 - Math.Pow(1 + mid, -months));
                if (pay < monthlyPayment)
                    low = mid;
                else
                    high = mid;
            }
            return mid * 12 * 100;   // % ต่อปี
        }
    }
}
