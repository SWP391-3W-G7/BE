I have successfully refactored the project to remove the `Staff` table and its dependencies, and the database has been updated with these changes.

Here is a summary of the work performed:

**1. DAL Changes:**
-   **Deleted Files**: `DAL/Models/Staff.cs`, `DAL/IRepositories/IStaffRepository.cs`, `DAL/Repositories/StaffRepository.cs`.
-   **`DAL/Models/User.cs`**:
    -   Removed `public virtual Staff Staff { get; set; }`.
    -   Added `public virtual ICollection<ReturnRecord> HandledReturnRecords { get; set; } = new List<ReturnRecord>();` to establish the inverse navigation for `ReturnRecord`s handled by a `User`.
-   **`DAL/Models/ReturnRecord.cs`**:
    -   Changed `public int? StaffId { get; set; }` to `public int? StaffUserId { get; set; }`.
    -   Changed `public virtual Staff Staff { get; set; }` to `public virtual User StaffUser { get; set; }`.
-   **`DAL/Models/Campus.cs`**:
    -   Removed `public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();`.
-   **`DAL/Models/LostFoundTrackingSystemContext.cs`**:
    -   Removed `public virtual DbSet<Staff> Staff { get; set; }`.
    -   Removed `modelBuilder.Entity<Staff>(...)` configuration.
    -   Updated `modelBuilder.Entity<ReturnRecord>(...)` to configure the new `StaffUserId` foreign key to the `User` table and its relationship (`FK_ReturnRecord_User_Staff`).
-   **Database Migration**:
    -   A new migration `RemoveStaffTable` was created to apply these schema changes.
    -   The migration was manually adjusted to comment out problematic `RenameIndex` operations.
    -   The migration was successfully applied to the database using `dotnet ef database update`.

**2. BLL Changes:**
-   **`BLL/Services/AdminService.cs`**:
    -   Removed dependency on `IStaffRepository`.
    -   Removed logic related to creating/updating `Staff` records in `AssignRoleAndCampusAsync`.
-   **`BLL/Services/ReturnRecordService.cs`**:
    -   Removed dependency on `IStaffRepository`.
    -   Updated `CreateReturnRecordAsync` to use `userId` directly for `StaffUserId`.
    -   Updated `MapToDto` to correctly retrieve `StaffName` and `StaffId` from the new `StaffUser` navigation property.
-   **`BLL/Services/ClaimRequestService.cs`**:
    -   Removed dependency on `IStaffRepository`.
    -   Updated `UpdateStatusAsync` to use `staffId` directly for `StaffUserId` when creating a `ReturnRecord`.
-   **`BLL/Services/LostItemService.cs`**:
    -   Removed dependency on `IStaffRepository`.
    -   Updated `UpdateStatusAsync` to use `staffId` directly for `StaffUserId` when creating a `ReturnRecord`.
-   **`BLL/IServices/IUserService.cs`**: Added `Task<UserDto> GetByIdAsync(int id);`.
-   **`BLL/Services/UserService.cs`**: Implemented `GetByIdAsync`.

**3. API Changes (Related to FoundItem Management):**
-   **`BLL/DTOs/FoundItemDTO/UpdateFoundItemStatusRequest.cs`**: Created a new DTO for updating only the status of a found item.
-   **`BLL/IServices/IFoundItemService.cs`**: Added `Task<FoundItemDto> UpdateStatusAsync(int id, UpdateFoundItemStatusRequest request);`.
-   **`BLL/Services/FoundItemService.cs`**:
    -   Implemented `UpdateStatusAsync`.
    -   Adjusted `CreateAsync` to set `StoredBy` to the `createdBy` user ID.
-   **`LostFoundApi/Controllers/FoundItemsController.cs`**:
    -   Injected `IUserService`.
    -   Added `POST /api/found-items` for staff to create found item records.
    -   Added `PUT /api/found-items/{id}/status` for staff to update a found item's status.
    -   Added `GET /api/found-items/campus` for staff to retrieve found items on their campus.
    -   All new staff-related endpoints are authorized for the "Staff" role.

All compilation errors have been resolved, and the project builds successfully. The database schema is now updated to reflect the removal of the Staff table.