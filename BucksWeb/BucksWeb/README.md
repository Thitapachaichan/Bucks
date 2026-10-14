# Bucks

ระบบคำนวณและวางแผนดอกเบี้ยเงินกู้ / ผ่อนชำระ เขียนด้วย C# (ASP.NET Core Razor Pages)

> Loan interest calculator and installment planner built with C# and ASP.NET Core.

โปรเจกต์กลุ่มวิชา Computer Programming Section 3 (Final Project)

## ทำอะไรได้บ้าง

- คำนวณค่างวดต่อเดือน รองรับดอกเบี้ยแบบ **Flat** และ **ลดต้นลดดอก**
- แสดงตารางผ่อนชำระทุกงวด และสรุปรายปี
- จำลองการจ่ายเพิ่มต่อเดือน ว่าปิดหนี้เร็วขึ้นกี่งวดและประหยัดดอกเบี้ยเท่าไร
- แสดงดอกเบี้ยที่แท้จริงต่อปีของแบบ Flat
- บันทึกประวัติการคำนวณลงไฟล์ `history.txt` และอ่านกลับมาแสดง
- ดาวน์โหลดตารางผ่อนชำระเป็นไฟล์ CSV

## ตัวอย่างผลลัพธ์

เงินต้น 100,000 บาท / ดอกเบี้ย 12% ต่อปี / 24 งวด

| ประเภท | ค่างวดต่อเดือน | ดอกเบี้ยรวม |
|---|---|---|
| Flat | 5,166.67 บาท | 24,000.00 บาท |
| ลดต้นลดดอก | 4,707.35 บาท | 12,976.33 บาท |

## วิธีรัน

ต้องติดตั้ง [.NET SDK](https://dotnet.microsoft.com/download) ก่อน

```bash
git clone <ใส่ลิงก์ repo ของกลุ่ม>
cd BucksWeb
dotnet run
```

เปิดลิงก์ที่แสดงใน Console (เช่น `http://localhost:5000`) ถ้า `dotnet --version` ไม่ใช่เวอร์ชัน 8 ให้แก้ `<TargetFramework>` ในไฟล์ `BucksWeb.csproj`

## โครงสร้างโปรเจกต์ (Modular Programming)

```
BucksWeb/
├── Program.cs                  # Main() จุดเริ่มต้นโปรแกรม
├── Models/
│   └── LoanResult.cs           # โครงเก็บผลลัพธ์ (array 1 มิติ)
├── Services/
│   ├── LoanValidator.cs        # ตรวจข้อมูล (if-else)
│   ├── LoanCalculator.cs       # คำนวณหลัก (switch, loop, array)
│   ├── LoanAnalyzer.cs         # สรุปรายปี (2D array), จำลองจ่ายเพิ่ม
│   └── FileService.cs          # เขียน/อ่านไฟล์ และสร้าง CSV
├── Pages/
│   ├── Index.cshtml            # หน้าเว็บ
│   └── Index.cshtml.cs         # รวมทุกโมดูลเข้าด้วยกัน (Integration)
└── docs/
    └── ALGORITHM.md            # วิเคราะห์ปัญหา, pseudocode, flowchart
```

## ความรู้จาก Chapters 1-10 ที่ใช้

| บทที่ | หัวข้อ | ใช้ที่ไหน |
|---|---|---|
| 1 | Problem solving & algorithm | `docs/ALGORITHM.md` วิเคราะห์ Input/Process/Output, pseudocode, flowchart |
| 2 | if-else | `LoanValidator.Validate()`, แจ้งเตือนค่างวดสูงใน `Index.cshtml.cs` |
| 3 | switch case | `LoanCalculator.Calculate()` เลือกสูตร, `TypeName()` |
| 4 | Functions / methods | ทุกขั้นตอนแยกเป็นฟังก์ชัน เรียกจาก `Main()` และ `IndexModel` |
| 5 | Loop (for / while) | `for` วนทุกงวด, `while` ใน `SimulateExtra()`, nested `for` ใน `BuildYearlySummary()` |
| 6 | Array (1D) | `InterestArr`, `PrincipalArr`, `BalanceArr` ใส่ค่าครบทุกช่อง |
| 7 | 2D array | `double[,]` สรุปรายปี และ `string[,]` ตารางประวัติ |
| 8 | File (I/O) | `FileService` เขียน/อ่าน `history.txt` และสร้าง CSV |
| 9 | Modular programming | แยกเป็นโมดูล Models, LoanValidator, LoanCalculator, LoanAnalyzer, FileService |
| 10 | Integration | `IndexModel` นำทุกโมดูลมารวมเป็นระบบเดียว |

## สูตรที่ใช้

- **Flat**: ดอกเบี้ยรวม = เงินต้น × อัตรา% × (งวด ÷ 12), ค่างวด = (เงินต้น + ดอกเบี้ยรวม) ÷ งวด
- **ลดต้นลดดอก**: r = อัตรา% ÷ 12 ÷ 100, ค่างวด = P·r ÷ (1 − (1+r)^−n), ดอกเบี้ยแต่ละงวด = ยอดคงเหลือ × r

## สมาชิกกลุ่ม

| ชื่อ | หน้าที่ |
|---|---|
| [ชื่อสมาชิกคนที่ 1] | สูตรคำนวณและ array |
| [ชื่อสมาชิกคนที่ 2] | ตรวจ input และหน้าเว็บ |
| [ชื่อสมาชิกคนที่ 3] | ส่วนต่อยอด ไฟล์ และการทดสอบ |
