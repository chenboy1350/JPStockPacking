# คู่มือการใช้งาน PIS API

## Base URL

| Environment | Base URL |
|---|---|
| Development | `https://192.168.2.13/SUT_JPWEBAPI/api/PIS` |
| Production | `https://192.168.2.237/JPWEBAPI/api/PIS` |

## Headers ที่ต้องส่งทุก Request

```
x-api-key: SmV3ZWx5UHJpbmNlc3NBUElLZXk
Content-Type: application/json
```

---

## Response Format

ทุก endpoint คืนค่าในรูปแบบ `BaseResponseModel`:

```json
{
  "code": 200,
  "isSuccess": true,
  "message": "string",
  "content": { ... }
}
```

---

## Endpoints

### Employee

---

#### GET `/Employee`
ดึงรายชื่อพนักงานทั้งหมด (ใช้ Cache)

**Response `content`:** `ResEmployeeModel[]`

```json
[
  {
    "employeeID": 1,
    "firstName": "สมชาย",
    "lastName": "ใจดี",
    "nickName": "ชาย",
    "isActive": true,
    "departmentID": 3,
    "departmentName": "แผนกบรรจุ",
    "createDate": "2024-01-01T00:00:00",
    "updateDate": "2024-01-01T00:00:00"
  }
]
```

---

#### GET `/AvailableEmployee`
ดึงรายชื่อพนักงานที่พร้อมทำงาน (ไม่ใช้ Cache)

**Response `content`:** `ResEmployeeModel[]` (เหมือน `/Employee`)

---

#### GET `/Department`
ดึงรายชื่อแผนกทั้งหมด (ใช้ Cache)

**Response `content`:** `DepartmentModel[]`

```json
[
  {
    "departmentID": 3,
    "departmentName": "แผนกบรรจุ"
  }
]
```

---

#### POST `/AddNewEmployee`
เพิ่มพนักงานใหม่

**Request Body:** `ResEmployeeModel`

```json
{
  "employeeID": 0,
  "firstName": "สมหญิง",
  "lastName": "รักงาน",
  "nickName": "หญิง",
  "isActive": true,
  "departmentID": 3,
  "departmentName": ""
}
```

**Response `content`:** ไม่มี (ดู `isSuccess`)

> หมายเหตุ: จะ clear cache `EmployeeList` อัตโนมัติ

---

#### PATCH `/EditEmployee`
แก้ไขข้อมูลพนักงาน

**Request Body:** `ResEmployeeModel` (ระบุ `employeeID` ที่ต้องการแก้ไข)

**Response `content`:** ไม่มี (ดู `isSuccess`)

> หมายเหตุ: จะ clear cache `EmployeeList` อัตโนมัติ

---

#### PATCH `/ToggleEmployeeStatus`
เปิด/ปิดสถานะพนักงาน

**Request Body:** `ResEmployeeModel` (ระบุ `employeeID`)

**Response `content`:** ไม่มี (ดู `isSuccess`)

> หมายเหตุ: จะ clear cache `EmployeeList` อัตโนมัติ

---

### User

---

#### GET `/GetAllUser`
ดึง User ทั้งหมด (ใช้ Cache)

**Response `content`:** `UserModel[]`

```json
[
  {
    "userID": 1,
    "employeeID": 5,
    "departmentID": 3,
    "username": "somchai",
    "password": "",
    "firstName": "สมชาย",
    "lastName": "ใจดี",
    "nickName": "ชาย",
    "departmentName": "แผนกบรรจุ",
    "isActive": true,
    "createDate": "2024-01-01T00:00:00",
    "updateDate": "2024-01-01T00:00:00"
  }
]
```

---

#### POST `/GetUser`
ดึง User แบบมี filter

**Request Body:** `ReqUserModel`

```json
{
  "userID": null,
  "username": null,
  "isActive": null
}
```

> ส่ง `null` = ไม่ filter field นั้น

**ตัวอย่าง filter เฉพาะ active:**

```json
{
  "userID": null,
  "username": null,
  "isActive": true
}
```

**Response `content`:** `UserModel[]`

---

#### POST `/ValidateApprover`
ตรวจสอบ username/password สำหรับ approver

**Request Body:**

```json
{
  "clientId": "somchai",
  "clientSecret": "password123",
  "audience": "SP"
}
```

**Response `content`:** `UserModel` (ถ้า validate ผ่าน)

> ถ้า user ไม่มีสิทธิ์ approve จะ throw exception `"User does not have permission to approve"`

---

#### POST `/AddNewUser`
เพิ่ม User ใหม่

**Request Body:** `UserModel`

```json
{
  "userID": 0,
  "employeeID": 5,
  "departmentID": 3,
  "username": "somchai",
  "password": "password123",
  "firstName": "สมชาย",
  "lastName": "ใจดี",
  "nickName": "ชาย",
  "departmentName": "",
  "isActive": true
}
```

**Response `content`:** ไม่มี (ดู `isSuccess`)

> หมายเหตุ: จะ clear cache `UserList` อัตโนมัติ

---

#### PATCH `/EditUser`
แก้ไขข้อมูล User

**Request Body:** `UserModel` (ระบุ `userID` ที่ต้องการแก้ไข)

**Response `content`:** ไม่มี (ดู `isSuccess`)

> หมายเหตุ: จะ clear cache `UserList` อัตโนมัติ

---

#### PATCH `/ToggleUserStatus`
เปิด/ปิดสถานะ User

**Request Body:** `UserModel` (ระบุ `userID`)

**Response `content`:** ไม่มี (ดู `isSuccess`)

> หมายเหตุ: จะ clear cache `UserList` อัตโนมัติ

---

## Cache Keys

| Cache Key | Endpoint | หมดอายุ |
|---|---|---|
| `EmployeeList` | `/Employee` | default |
| `DepartmentList` | `/Department` | default |
| `UserList` | `/GetAllUser` | default |

การ Add / Edit / Toggle จะ **clear cache** ของ group นั้นทันที

---

## การใช้งานผ่าน IPISService ใน C\#

inject `IPISService` แล้วเรียกใช้งาน:

```csharp
public class MyController : Controller
{
    private readonly IPISService _pisService;

    public MyController(IPISService pisService)
    {
        _pisService = pisService;
    }

    // ดึงพนักงานทั้งหมด
    var employees = await _pisService.GetEmployeeAsync();

    // ดึงพนักงานที่พร้อมทำงาน
    var available = await _pisService.GetAvailableEmployeeAsync();

    // ดึงแผนก
    var departments = await _pisService.GetDepartmentAsync();

    // ดึง User พร้อม filter
    var users = await _pisService.GetUser(new ReqUserModel { IsActive = true });

    // Validate Approver
    var approver = await _pisService.ValidateApproverAsync("username", "password");

    // เพิ่มพนักงาน
    var result = await _pisService.AddNewEmployee(new ResEmployeeModel { ... });

    // แก้ไขพนักงาน
    var result = await _pisService.EditEmployee(employeeModel);

    // Toggle สถานะ
    var result = await _pisService.ToggleEmployeeStatus(employeeModel);
}
```

---

## Models Reference

### ResEmployeeModel

| Field | Type | หมายเหตุ |
|---|---|---|
| `EmployeeID` | `int` | PK (ส่ง 0 เมื่อ Add) |
| `FirstName` | `string` | ชื่อจริง |
| `LastName` | `string` | นามสกุล |
| `NickName` | `string` | ชื่อเล่น |
| `IsActive` | `bool` | สถานะ |
| `DepartmentID` | `int` | รหัสแผนก |
| `DepartmentName` | `string` | ชื่อแผนก (read-only) |
| `CreateDate` | `DateTime` | |
| `UpdateDate` | `DateTime` | |

### UserModel

| Field | Type | หมายเหตุ |
|---|---|---|
| `UserID` | `int` | PK (ส่ง 0 เมื่อ Add) |
| `EmployeeID` | `int` | FK → Employee |
| `DepartmentID` | `int` | FK → Department |
| `Username` | `string` | |
| `Password` | `string` | |
| `FirstName` | `string` | |
| `LastName` | `string` | |
| `NickName` | `string` | |
| `DepartmentName` | `string` | read-only |
| `IsActive` | `bool` | |
| `CreateDate` | `DateTime` | |
| `UpdateDate` | `DateTime` | |

### ReqUserModel (filter)

| Field | Type | หมายเหตุ |
|---|---|---|
| `UserID` | `int?` | null = ไม่ filter |
| `Username` | `string?` | null = ไม่ filter |
| `IsActive` | `bool?` | null = ไม่ filter |

### DepartmentModel

| Field | Type |
|---|---|
| `DepartmentID` | `int` |
| `DepartmentName` | `string` |
