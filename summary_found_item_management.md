I have successfully updated the Found Items Controller and related services to implement the new found item creation and listing logic based on user roles and item status.

Here is a summary of the changes:

**1. DAL Changes:**
-   **`DAL/Models/Enum.cs`**: Added a new `Open = 3` status to the `FoundItemStatus` enum.
-   **`DAL/IRepositories/IFoundItemRepository.cs`**: Added `Task<List<FoundItem>> GetByCampusAsync(int campusId, string status);`.
-   **`DAL/Repositories/FoundItemRepository.cs`**: Implemented `GetByCampusAsync(int campusId, string status);` to filter found items by campus and status.

**2. BLL Changes:**
-   **`BLL/IServices/IFoundItemService.cs`**:
    -   Modified `CreateAsync` to accept an optional `initialStatus` parameter.
    -   Added `Task<List<FoundItemDto>> GetByCampusAsync(int campusId, string status);`.
-   **`BLL/Services/FoundItemService.cs`**:
    -   Updated `CreateAsync` implementation to use the `initialStatus` parameter (defaults to "Stored" if not provided).
    -   Implemented `GetByCampusAsync(int campusId, string status)` which calls the corresponding repository method.

**3. API Changes:**
-   **`LostFoundApi/Controllers/FoundItemsController.cs`**:
    -   **`POST /api/found-items` endpoint**:
        -   Authorization updated to `[Authorize(Roles = "Staff,User,Security Officer")]`.
        -   Logic added to dynamically determine the `initialStatus` based on the user's role:
            -   "Open" for "User" (Student) and "Security Officer" roles.
            -   "Stored" for "Staff" role.
        -   This `initialStatus` is passed to the `_foundItemService.CreateAsync()` method.
    -   **`GET /api/found-items/campus/open` endpoint**:
        -   A new endpoint added, authorized for `[Authorize(Roles = "User,Security Officer")]`.
        -   Allows Students and Security Officers to retrieve "Open" status found items on their campus.
        -   Calls `_foundItemService.GetByCampusAsync(user.CampusId.Value, FoundItemStatus.Open.ToString())`.
    -   Added `using DAL.Models;` to access the `FoundItemStatus` enum.

The project builds successfully with these changes, and the new functionality is now available.