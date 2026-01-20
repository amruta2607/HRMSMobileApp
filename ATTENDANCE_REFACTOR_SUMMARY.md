# Attendance API Refactoring Summary

## Overview
The Attendance (Punch In / Punch Out) APIs have been refactored to use `UserId` instead of `EmployeeId` in the request body.

## Business Rules
- Client sends `UserId` in request body (not `EmployeeId`)
- System resolves `EmployeeId` by joining `Users` → `Employee` using `Users.UserId = Employee.SystemUserId`
- If no employee exists for the given `UserId`, a proper error is returned
- Client must never pass `EmployeeId` directly

## New Endpoints

### 1. Punch In (V2)
**Endpoint:** `POST /attendance/punch-in-v2`

**Request Body:**
```json
{
  "userId": 25,
  "punchTime": "2026-01-15T10:15:00Z",
  "attendanceDate": "2026-01-15T00:00:00Z",
  "longitude": 77.2090,
  "latitude": 28.6139
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Punch In Successful",
  "employeeId": 42
}
```

**Error Response (400 Bad Request):**
```json
{
  "success": false,
  "message": "No employee found for the specified UserId."
}
```

**Other Error Responses:**
- `"Punch In already done for today."` - When user has already punched in
- `"UserId is required."` - When userId is missing or invalid
- `"Punch In Failed"` - When database insert fails

### 2. Punch Out (V2)
**Endpoint:** `POST /attendance/punch-out-v2`

**Request Body:**
```json
{
  "userId": 25,
  "punchTime": "2026-01-15T18:30:00Z",
  "attendanceDate": "2026-01-15T00:00:00Z",
  "longitude": 77.2090,
  "latitude": 28.6139
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Punch Out Successful",
  "employeeId": 42
}
```

**Error Response (400 Bad Request):**
```json
{
  "success": false,
  "message": "No employee found for the specified UserId."
}
```

**Other Error Responses:**
- `"Cannot Punch Out — Punch In not done."` - When user hasn't punched in
- `"Punch Out already done."` - When user has already punched out
- `"UserId is required."` - When userId is missing or invalid

## Technical Implementation

### Architecture
- **Controller Layer:** `AttendanceController` - Handles HTTP requests/responses
- **Service Layer:** `AttendanceService` - Business logic and EmployeeId resolution
- **Repository Layer:** `AttendanceRepository` - Database operations

### Key Methods

#### 1. EmployeeId Resolution
```csharp
private async Task<int?> ResolveEmployeeIdFromUserIdAsync(int userId)
```
- Joins `Users` → `Employee` using `Users.UserId = Employee.SystemUserId`
- Returns `EmployeeId` if found, `null` otherwise
- Logs all operations for debugging

#### 2. Service Methods
- `PunchInByUserIdAsync(PunchInRequestV2 req)` - Handles punch-in with UserId
- `PunchOutByUserIdAsync(PunchOutRequestV2 req)` - Handles punch-out with UserId

### Data Models

#### PunchInRequestV2
```csharp
public class PunchInRequestV2
{
    public int UserId { get; set; }
    public DateTime PunchTime { get; set; }
    public DateTime AttendanceDate { get; set; }
    public double? Longitude { get; set; }
    public double? Latitude { get; set; }
}
```

#### PunchOutRequestV2
```csharp
public class PunchOutRequestV2
{
    public int UserId { get; set; }
    public DateTime PunchTime { get; set; }
    public DateTime AttendanceDate { get; set; }
    public double? Longitude { get; set; }
    public double? Latitude { get; set; }
}
```

#### PunchResponse
```csharp
public class PunchResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int? EmployeeId { get; set; }
}
```

## Security & Access Control
- Users can only punch in/out for themselves
- The system validates that `request.UserId == CurrentUserId`
- HR/TenantAdmin access can be extended if needed

## Database Schema
- **Users Table:** Primary Key = `UserId`
- **Employee Table:** Contains `SystemUserId` (FK → `Users.UserId`)
- **Punch/Attendance Table:** Stores `EmployeeId`

## Backward Compatibility
- Legacy endpoints (`/punch-in` and `/punch-out`) remain available
- Old endpoints use `EmployeeId` directly
- New endpoints use `UserId` and resolve `EmployeeId` internally

## Error Handling
All errors are properly handled with:
- Validation errors (missing/invalid UserId)
- Business rule violations (already punched in/out, no punch-in before punch-out)
- Database errors (employee not found, insert/update failures)
- Proper HTTP status codes (200, 400, 401, 404)

## Example Usage

### cURL Example - Punch In
```bash
curl -X POST "https://api.example.com/attendance/punch-in-v2" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "userId": 25,
    "punchTime": "2026-01-15T10:15:00Z",
    "attendanceDate": "2026-01-15T00:00:00Z"
  }'
```

### cURL Example - Punch Out
```bash
curl -X POST "https://api.example.com/attendance/punch-out-v2" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "userId": 25,
    "punchTime": "2026-01-15T18:30:00Z",
    "attendanceDate": "2026-01-15T00:00:00Z"
  }'
```

## Files Modified/Created

### New Files
- `Models/PunchInRequestV2.cs`
- `Models/PunchOutRequestV2.cs`
- `Models/PunchResponse.cs`

### Modified Files
- `Controllers/AttendanceController.cs` - Added new endpoints
- `Services/AttendanceService.cs` - Added resolution method and new service methods
- `Interfaces/IAttendanceService.cs` - Added new method signatures
- `Constants/Messages.cs` - Added error messages
- `Constants/LogMessages.cs` - Added log messages

