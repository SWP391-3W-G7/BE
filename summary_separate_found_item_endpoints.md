I have successfully updated the Found Items Controller to provide separate endpoints for creating found items based on user roles, as requested.

Here is a summary of the changes:

**1. API Changes (`LostFoundApi/Controllers/FoundItemsController.cs`):**
-   **`POST /api/found-items/staff` endpoint**:
    -   Authorized for `[Authorize(Roles = "Staff")]`.
    -   Allows Staff members to create found item records.
    -   Explicitly sets the initial status to `FoundItemStatus.Stored`.
-   **`POST /api/found-items/report` endpoint**:
    -   Authorized for `[Authorize(Roles = "User,Security Officer")]`.
    -   Allows Students ("User" role) and Security Officers to report found items.
    -   Explicitly sets the initial status to `FoundItemStatus.Open`.
-   The previous single `POST /api/found-items` endpoint with conditional logic based on roles has been replaced by these two distinct endpoints.

The project builds successfully with these changes, and the new functionality is now available.