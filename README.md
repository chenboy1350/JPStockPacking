# JPStockPacking

ระบบจัดการสต็อกและการแพ็คสินค้าเครื่องประดับ สำหรับ Princess Jewelry

---

## ภาพรวม

JPStockPacking เป็นระบบ Web Application สำหรับจัดการกระบวนการแพ็คสินค้าเครื่องประดับแบบครบวงจร ตั้งแต่การนำเข้าออเดอร์ การรับสินค้า การมอบหมายงาน การแพ็ค ไปจนถึงการส่งสินค้าไปยังปลายทางต่าง ๆ

---

## ฟีเจอร์หลัก

| โมดูล | ฟังก์ชัน |
|-------|---------|
| **จัดการออเดอร์** | นำเข้า/ค้นหาออเดอร์จากระบบ JP, ติดตามสถานะ Lot |
| **จัดการการรับสินค้า** | บันทึกการรับสินค้า, แยกประเภท Sample/ปกติ |
| **วางแผนการแพ็ค** | กำหนดสินค้าที่จะแพ็ค, สร้างรายงานแผนงาน |
| **มอบหมายงาน** | มอบหมายสินค้าให้โต๊ะ/พนักงาน, ติดตามความคืบหน้า |
| **ติดตามการผลิต** | บันทึกของแตก/สูญหายระหว่างแพ็ค พร้อม workflow อนุมัติ |
| **จัดการสินค้าแพ็คแล้ว** | กำหนดจำนวนส่ง, เส้นทางไปยัง Store / Melt / Export / Showroom / Lost |
| **ตรวจสอบใบแจ้งหนี้** | เปรียบเทียบ Invoice กับสต็อกจริง, ออกรายงาน |
| **จัดการผู้ใช้งาน** | CRUD User, กำหนดสิทธิ์การเข้าถึงรายฟีเจอร์ |
| **ประเภทสินค้า** | สร้างและจัดการ Product Type |
| **ตั้งค่าระบบ** | ปรับ % tolerance การส่งสินค้า |

---

## Tech Stack

- **Runtime**: .NET 9.0
- **Framework**: ASP.NET Core MVC + Razor Views
- **ORM**: Entity Framework Core 9 + Dapper
- **Database**: SQL Server (4 ฐานข้อมูล)
- **UI**: AdminLTE + Bootstrap + FontAwesome
- **PDF**: QuestPDF
- **Image**: SkiaSharp
- **Logging**: Serilog
- **Auth**: Cookie-based Authentication

---

## โครงสร้างโปรเจกต์

```
JPStockPacking/
├── Controllers/
│   ├── AuthController.cs        # Login / Logout
│   └── HomeController.cs        # Controller หลักของระบบ
├── Services/
│   ├── Interface/               # Service interfaces
│   ├── Implement/               # Service implementations
│   ├── Helper/                  # Utilities และ Enums
│   └── Middleware/              # Token refresh middleware
├── Data/
│   ├── JPDbContext/             # ฐานข้อมูล JP (ออเดอร์, ลูกค้า)
│   ├── SPDbContext/             # ฐานข้อมูล Stock Packing (core)
│   ├── BMDbContext/             # ฐานข้อมูล Best Manage
│   └── SWDbContext/             # ฐานข้อมูล Showroom
├── Models/                      # DTOs และ View Models
├── Views/
│   ├── Auth/                    # หน้า Login
│   ├── Home/                    # Dashboard หลัก
│   ├── Partial/                 # Partial views แยกตามโมดูล
│   └── Shared/                  # Layout templates
├── wwwroot/                     # Static assets (CSS, JS, fonts, images)
├── docs/                        # เอกสารประกอบ
│   ├── spdb-er-diagram.md       # ER Diagram
│   ├── service-class-diagram.md # Class Diagram
│   ├── pis-api-guide.md         # คู่มือ PIS API
│   └── user-manual.md           # คู่มือการใช้งาน
└── Program.cs                   # DI Container และ Middleware setup
```

---

## ฐานข้อมูล

ระบบเชื่อมต่อกับ 4 ฐานข้อมูล SQL Server:

| Key | ฐานข้อมูล | หน้าที่ |
|-----|-----------|--------|
| `JPDBEntries` | JP Database | ข้อมูลออเดอร์, ลูกค้า, Invoice |
| `SPDBEntries` | Stock Packing DB | ข้อมูลหลักของระบบ |
| `BMDBEntries` | Best Manage DB | ข้อมูลจาก Best Manage |
| `SWDBEntries` | Showroom DB | ข้อมูล Showroom |

---

## การติดตั้งและรัน

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server
- Visual Studio 2022+ หรือ VS Code

### ตั้งค่า Connection Strings

แก้ไขไฟล์ `appsettings.Development.json` (dev) หรือ `appsettings.Production.json` (prod):

```json
{
  "ConnectionStrings": {
    "JPDBEntries": "<JP connection string>",
    "SPDBEntries": "<Stock Packing connection string>",
    "BMDBEntries": "<Best Manage connection string>",
    "SWDBEntries": "<Showroom connection string>"
  },
  "AppSettings": {
    "AppVersion": "1.0.1a",
    "UseByPass": false
  },
  "SendQtySettings": {
    "Percentage": 2,
    "ReportApprover": 11
  },
  "ApiSettings": {
    "APIKey": "<PIS API Key>",
    "AccessToken": "<Token endpoint>",
    "Employee": "<Employee endpoint>"
  }
}
```

### Build

```bash
dotnet build JPStockPacking.sln
```

### รัน (Development)

```bash
dotnet run --project JPStockPacking/JPStockPacking.csproj --launch-profile Development
```

### Publish (Production)

```bash
dotnet publish JPStockPacking/JPStockPacking.csproj -c Release -o ./publish
```

---

## เอกสารเพิ่มเติม

- [ER Diagram](docs/spdb-er-diagram.md)
- [Service Class Diagram](docs/service-class-diagram.md)
- [PIS API Guide](docs/pis-api-guide.md)
- [คู่มือการใช้งาน](docs/user-manual.md)

---

## License

[MIT License](LICENSE.txt)
