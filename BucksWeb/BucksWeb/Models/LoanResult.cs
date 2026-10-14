namespace BucksWeb.Models
{
    // โมดูลข้อมูล: เก็บผลลัพธ์การคำนวณ (ใช้ Array 1 มิติเก็บผลรายงวด)
    public class LoanResult
    {
        public double MonthlyPayment;
        public double TotalInterest;
        public double TotalPaid;
        public double[] InterestArr;    // ดอกเบี้ยแต่ละงวด
        public double[] PrincipalArr;   // เงินต้นแต่ละงวด
        public double[] BalanceArr;     // ยอดคงเหลือแต่ละงวด
    }
}
