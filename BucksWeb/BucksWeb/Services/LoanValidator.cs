namespace BucksWeb.Services
{
    // โมดูลตรวจข้อมูล
    public static class LoanValidator
    {
        // if-else: ตรวจข้อมูลที่กรอก คืนข้อความผิดพลาด หรือ null ถ้าถูกต้อง
        public static string Validate(double principal, double annualRate, int months, int loanType)
        {
            if (principal <= 0)
                return "เงินต้นต้องมากกว่า 0";
            else if (annualRate < 0)
                return "ดอกเบี้ยต้องไม่ติดลบ";
            else if (months < 1 || months > 360)
                return "จำนวนงวดต้องอยู่ระหว่าง 1 - 360 เดือน";
            else if (loanType != 1 && loanType != 2)
                return "ประเภทดอกเบี้ยไม่ถูกต้อง";
            else
                return null;
        }
    }
}
