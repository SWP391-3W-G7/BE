# Lost and Found Tracking System (Backend API)

## Project Description
This project implements a comprehensive Lost and Found Tracking System designed for educational campuses. It provides a robust backend API to manage lost items, found items, claim requests, matching processes, and user roles (Student/User, Staff, Security Officer, Admin) across multiple campuses. The system aims to streamline the process of reporting, tracking, and returning lost property, enhancing efficiency and user experience.

## Key Features

### System Roles & Responsibilities

#### 1. User (Student)
Primary users who interact with the lost & found system for submitting and claiming items.
**Key Responsibilities:**
-   **Lost Item Report:** Create, update, or cancel lost item reports (providing details like title, description, location, time, images, and contact information).
-   **Found Item Report:** Create reports for items they have found (including description, location, time, and images). These reports are forwarded to Staff for verification.
-   **Claim Process:** Submit requests to claim items they believe are theirs, providing evidence (e.g., photos, timestamps, detailed descriptions, purchase receipts).
-   **Status Tracking:** View the status of their submitted lost item reports, found item reports, and claim requests. Receive notifications regarding potential matches, claim approvals/rejections, and item readiness for pickup.

#### 2. Staff (Dịch vụ Sinh viên – DVSV)
Responsible for managing physical lost and found items on campus and handling verification and return processes.
**Key Responsibilities:**
-   **Found Items Management:** Receive items from Security or other finders and create official Found Item records in the system (details, photos, category, campus, storage location). Initial status is "Stored." Update item status as needed (e.g., Stored → Claimed → Returned), and manage storage locations/dates.
-   **Claim/Lost Verification:** Review claim requests and lost item reports against actual items in storage. Contact users for further verification, request additional evidence, and make decisions to approve or reject claims.
-   **Return Process:** Arrange item pickups with users. Verify user identity (e.g., FPT Card) and claim approval. Record the successful return in the system (including return date, notes, staff ID, receiver ID, and found item ID).
-   **Conflict Resolution:** Handle cases where multiple users claim the same item or submit conflicting lost item reports. Compare information, request more evidence, and escalate to Security Officers for serious disputes.
-   **Reporting:** Access and view system reports and analytics, including total lost/found items, successful returns, false claims, conflict cases, and campus-level statistics.

#### 3. Security Officer (Bộ phận bảo vệ)
Responsible for initial intake and transfer of found items, assisting with verification, and supporting dispute resolution.
**Key Responsibilities:**
-   **Found Item Intake:** Receive found items from users. Log a temporary found item record in the system (initial status "Open"). Transfer the physical item to DVSV Staff at the respective campus.
-   **Verification Support:** Assist Staff in verifying lost/found locations and times based on reports. Provide investigative support for complex claim disputes.
-   **Report & Escalation:** Collaborate with Staff in managing claim disputes and escalating security-related incidents.

### Multi-Campus Support
The system is designed to support multiple campuses (e.g., Nguyễn Văn Cừ / NVH SV, SHTP / Saigon High-Tech Park), allowing roles to operate within their specific campus scope without hard-coded limitations:
-   **Users:** Can report lost items at any campus but found items at their current campus location.
-   **Staff:** Manage items exclusively for their assigned campus, with all reports and functionalities filtered accordingly.
-   **Security Officers:** Responsible for receiving and transferring items within their specific campus.

## Workflows

#### 1. Lost Item Workflow (User Reports a Lost Item)
-   **Start:** User loses an item.
-   User submits a Lost Item Report via the system (providing title/description, location, time, and evidence).
-   **System Action:** Creates a `LostItem` record with `Status = "Lost"`.
-   **Notification:** System notifies DVSV Staff of the relevant campus.
-   **Matching:** System attempts immediate auto-matching with stored found items.
-   **End:** Continues until a claim is made or a match occurs.

#### 2. Found Item Workflow (Item Found by User or Public)
-   **Start:** Someone finds an item.
-   The finder delivers the physical item to a Security Officer or DVSV Staff.

#### 3. Security Officer Workflow (Initial Intake)
-   **Security Action:** Security receives the found item.
-   **System Action:** Security logs a temporary Found Item Record in the system with `Status = "Open"`.
-   **Transfer:** Security passes the physical item to DVSV Staff.

#### 4. DVSV Staff Official Storage Workflow
-   **Start:** Staff receives the physical item (from Security or the finder).
-   **Staff Action:** Staff creates the Official Found Item Record in the system (including detailed descriptions, photos, category, campus, and precise storage location).
-   **System Action:** Sets `FoundItem.Status = "Stored"`.
-   **Staff Action:** Staff updates the item's status as needed (`Stored` → `Claimed` → `Returned`).

#### 5. Claim Request Workflow (User Requests the Item Back)
-   **Start:** User wants to retrieve a lost item.
-   User browses the list of found items in the system.
-   User submits a Claim Request (providing evidence like photos, timestamps, and item details).
-   **System Action:** Sets `Claim.Status = "Pending"`.

#### 6. Staff Claim/Lost Review Workflow
-   **Staff Action:** Staff reviews the submitted claim or lost item report:
    -   Evaluates provided evidence.
    -   Compares item details with existing records.
    -   Requests more information or additional evidence from the user if required.
-   **Staff Outcome:**
    -   **Approve:** Claim proceeds to the Return Workflow (`Claim.Status = "Approved"`).
    -   **Reject:** User is notified, and the claim process ends (`Claim.Status = "Rejected"`).
    -   **Conflict:** Claim proceeds to the Conflict Workflow (`Claim.Status = "Conflicted"`).

#### 7. Conflict Workflow
-   **Trigger:** Initiated when:
    -   More than one user claims or reports losing the same item, OR
    -   Submitted evidence for a claim is inconsistent or inconclusive.
-   **System Action:** Flags the item/claim as conflicted.
-   **Staff Action:** Staff manually compares all conflicting claim submissions, potentially engaging in direct communication with the users involved.
-   **Staff Resolution:** Staff performs a resolution:
    -   Approves one specific claim and rejects all others.
    -   Rejects all claims if insufficient evidence is provided by any party.
-   **Outcome:** The approved claim (if any) moves to the Return Workflow.

#### 8. Return Item Workflow (Approved Claim)
-   **Start:** Claim is approved.
-   **Notification:** System notifies the user of the approved claim, providing pickup time and DVSV office location.
-   **User Action:** User attends the pickup appointment and presents their ID card.
-   **Staff Action:** Staff verifies:
    -   User ID.
    -   That the claim is approved.
    -   The condition of the item.
-   **System Action:** Staff records a `ReturnRecord` in the system (including return date, notes, StaffUserID, ReceiverID, and `FoundItemId`).
-   **Status Update:** System updates `FoundItem.Status = "Returned"`. If the Found Item was linked to a Lost Item, `LostItem.Status` is updated to "Returned".

#### 9. Reporting & Administration Workflow
-   **Staff/Admin Access:** Authorized personnel can view various system reports and analytics:
    -   Total number of lost items.
    -   Total number of found items.
    -   Current inventory of items still in storage.
    -   Campus-level analytics and statistics.
-   **Admin Privileges:** Administrators have additional capabilities, such as:
    -   Adding new campuses to the system.
    -   Assigning staff and security officers to specific roles and campuses.

## API Endpoints

### Authentication
The API uses JWT (JSON Web Tokens) for authentication. Users must obtain a token by logging in, and then include this token in the `Authorization` header of subsequent requests as a Bearer token. Roles are used for authorization to restrict access to certain endpoints.

### AdminController (`/api/admin`)

-   **`POST /api/admin/campuses`**
    -   **Description:** Creates a new campus.
    -   **Authorization:** `Admin`
    -   **Request Body:** `CreateCampusRequest` (contains `CampusName`, `Address`, `StorageLocation`).
-   **`POST /api/admin/assign-role`**
    -   **Description:** Assigns a role and campus to a user.
    -   **Authorization:** `Admin`
    -   **Request Body:** `AssignRoleRequest` (contains `UserId`, `RoleId`, `CampusId`).

### CampusController (`/api/Campus`)

-   **`GET /api/Campus`**
    -   **Description:** Retrieves a list of all campuses.
    -   **Authorization:** None.
-   **`GET /api/Campus/enum-values`**
    -   **Description:** Retrieves a list of campus enum values, their IDs, names, and descriptions.
    -   **Authorization:** None.

### CategoriesController (`/api/categories`)

-   **`GET /api/categories`**
    -   **Description:** Retrieves a list of all item categories.
    -   **Authorization:** None (Accessible to all authenticated users).

### ClaimRequestsController (`/api/claim-requests`)

-   **`POST /api/claim-requests`**
    -   **Description:** Creates a new claim request for a found item.
    -   **Authorization:** Authenticated users.
    -   **Request Body:** `CreateClaimRequest` (Form data, including files for evidence images).
-   **`GET /api/claim-requests/my-claims`**
    -   **Description:** Retrieves all claim requests submitted by the authenticated user.
    -   **Authorization:** Authenticated users.
-   **`GET /api/claim-requests/{id}`**
    -   **Description:** Retrieves details for a specific claim request.
    -   **Authorization:** Authenticated users (Owner, Admin, Staff).
-   **`GET /api/claim-requests`**
    -   **Description:** Retrieves a list of all claim requests.
    -   **Authorization:** `Staff`, `Admin`
-   **`PUT /api/claim-requests/{id}`**
    -   **Description:** Updates an existing claim request.
    -   **Authorization:** Authenticated users (Owner).
    -   **Request Body:** `UpdateClaimRequest` (Form data).
-   **`PATCH /api/claim-requests/{id}/status`**
    -   **Description:** Changes the status of a specific claim request (e.g., "Pending" to "Approved", "Rejected", "Returned").
    -   **Authorization:** `Admin`, `Staff`
    -   **Request Body:** `status` (Query parameter, `ClaimStatus` enum string).
-   **`PUT /api/claim-requests/{claimId}/conflict`**
    -   **Description:** Marks a specific claim request as "Conflicted".
    -   **Authorization:** `Admin`, `Staff`
-   **`POST /api/claim-requests/{claimId}/evidence`**
    -   **Description:** Allows a user to add new evidence (images/description) to an existing claim request.
    -   **Authorization:** `User`, `Security Officer` (Owner of the claim or Security Officer).
    -   **Request Body:** `AddEvidenceRequest` (Form data, including files for images).

### FoundItemsController (`/api/found-items`)

-   **`GET /api/found-items`**
    -   **Description:** Retrieves a list of all found items.
    -   **Authorization:** None (Accessible to all authenticated users).
-   **`POST /api/found-items/staff`**
    -   **Description:** Creates a new found item record. Initial status is "Stored".
    -   **Authorization:** `Staff`
    -   **Request Body:** `CreateFoundItemRequest` (Form data, including files for images).
-   **`POST /api/found-items/public`**
    -   **Description:** Allows a User (Student) or Security Officer to report a new found item. Initial status is "Open".
    -   **Authorization:** `User`, `Security Officer`
    -   **Request Body:** `CreateFoundItemRequest` (Form data, including files for images).
-   **`PUT /api/found-items/{id}/status`**
    -   **Description:** Updates the status of a specific found item (e.g., from "Open" to "Stored", or "Stored" to "Claimed", "Returned").
    -   **Authorization:** `Staff`, `Security Officer`
    -   **Request Body:** `UpdateFoundItemStatusRequest` (JSON, contains `Status` string).
-   **`GET /api/found-items/campus`**
    -   **Description:** Retrieves all found items associated with the authenticated Staff user's campus.
    -   **Authorization:** `Staff`
-   **`GET /api/found-items/campus/open`**
    -   **Description:** Retrieves all found items on the authenticated Staff user's campus that have an "Open" status. Intended for Staff to review newly reported items.
    -   **Authorization:** `Staff`
-   **`GET /api/found-items/{id}/details`**
    -   **Description:** Retrieves detailed information about a specific found item, including associated claim requests and matched lost items.
    -   **Authorization:** `Staff`, `Admin`
-   **`GET /api/found-items/{id}/user-details`**
    -   **Description:** Retrieves details for a specific found item, intended for a general user view.
    -   **Authorization:** `User`, `Security Officer`, `Staff`, `Admin`
-   **`PUT /api/found-items/{id}`**
    -   **Description:** Updates a specific found item.
    -   **Authorization:** Authenticated users.
    -   **Request Body:** `UpdateFoundItemDTO` (Form data).
-   **`DELETE /api/found-items/{id}`**
    -   **Description:** Deletes a specific found item by changing its status to "Closed".
    -   **Authorization:** Authenticated users.

### LostItemsController (`/api/lost-items`)

-   **`GET /api/lost-items`**
    -   **Description:** Retrieves a list of all lost items.
    -   **Authorization:** None (Accessible to all authenticated users).
-   **`GET /api/lost-items/{id}`**
    -   **Description:** Retrieves details for a specific lost item.
    -   **Authorization:** None (Accessible to all authenticated users).
-   **`POST /api/lost-items`**
    -   **Description:** Creates a new lost item report.
    -   **Authorization:** Authenticated users.
    -   **Request Body:** `CreateLostItemRequest` (Form data, including files for images).
-   **`PUT /api/lost-items/{id}`**
    -   **Description:** Updates an existing lost item report.
    -   **Authorization:** Authenticated users (presumably the creator).
    -   **Request Body:** `UpdateLostItemRequest` (Form data).
-   **`DELETE /api/lost-items/{id}`**
    -   **Description:** Deletes a specific lost item report.
    -   **Authorization:** Authenticated users (presumably the creator).
-   **`GET /api/lost-items/campus/{campusId}`**
    -   **Description:** Retrieves all lost items associated with a specific campus ID.
    -   **Authorization:** None (Accessible to all authenticated users).
-   **`GET /api/lost-items/category/{categoryId}`**
    -   **Description:** Retrieves all lost items belonging to a specific category ID.
    -   **Authorization:** None (Accessible to all authenticated users).
-   **`GET /api/lost-items/search?title={title}`**
    -   **Description:** Searches for lost items by title.
    -   **Authorization:** None (Accessible to all authenticated users).
-   **`PATCH /api/lost-items/{id}/status`**
    -   **Description:** Updates the status of a specific lost item.
    -   **Authorization:** `Admin`, `Staff`
    -   **Request Body:** `status` (Query parameter string, e.g., "Lost", "Returned").

### MatchingController (`/api/Matching`)

-   **`POST /api/Matching/lost-item/{lostItemId}/find-matches`**
    -   **Description:** Triggers the matching process to find potential found items for a specific lost item.
    -   **Authorization:** None (presumably triggered by system or specific internal role).
-   **`GET /api/Matching/found-item/{foundItemId}`**
    -   **Description:** Retrieves all `ItemMatch` records associated with a specific found item, including details of the found item, lost item, and related claim requests.
    -   **Authorization:** None (Accessible to all authenticated users).
-   **`GET /api/Matching/lost-item/{lostItemId}`**
    -   **Description:** Retrieves all `ItemMatch` records associated with a specific lost item, including details of the found item, lost item, and related claim requests.
    -   **Authorization:** None (Accessible to all authenticated users).
-   **`PUT /api/Matching/{matchId}/confirm`**
    -   **Description:** Confirms a specific `ItemMatch`, marking it as "Approved" and "Resolved". Updates associated Lost and Found item statuses to "Returned".
    -   **Authorization:** `Staff`, `Admin`
-   **`PUT /api/Matching/{matchId}/dismiss`**
    -   **Description:** Dismisses a specific `ItemMatch`.
    -   **Authorization:** `Staff`, `Admin`
-   **`PUT /api/Matching/{matchId}/conflict`**
    -   **Description:** Marks a specific `ItemMatch` as "Conflicted".
    -   **Authorization:** `Staff`, `Admin`

### SecurityController (`/api/security`)

-   **`GET /api/security/my-open-found-items`**
    -   **Description:** Retrieves a list of all found items with an "Open" status created by the authenticated Security Officer.
    -   **Authorization:** `Security Officer`
-   **`PUT /api/security/found-items/{id}/return`**
    -   **Description:** Updates the status of a specific found item to "Returned".
    -   **Authorization:** `Security Officer`
    -   **Request Body:** None (status is implicitly "Returned" in the endpoint logic).

### ItemActionLogsController (`/api/item-action-logs`)

-   **`GET /api/item-action-logs/found-item/{foundItemId}`**
    -   **Description:** Retrieves all action logs for a specific found item.
    -   **Authorization:** `Staff`, `Admin`
-   **`GET /api/item-action-logs/lost-item/{lostItemId}`**
    -   **Description:** Retrieves all action logs for a specific lost item.
    -   **Authorization:** `Staff`, `Admin`
-   **`GET /api/item-action-logs/claim-request/{claimRequestId}`**
    -   **Description:** Retrieves all action logs for a specific claim request.
    -   **Authorization:** `Staff`, `Admin`

### CategoriesController (`/api/categories`)

-   **`GET /api/categories`**
    -   **Description:** Retrieves a list of all item categories.
    -   **Authorization:** None (Accessible to all authenticated users).

### UsersController (`/api/Users`)

-   **`POST /api/Users/register`**
    -   **Description:** Registers a new user.
    -   **Authorization:** None (Publicly accessible).
    -   **Request Body:** `UserRegisterDto` (includes username, email, password, full name, role ID, campus ID, phone number).
-   **`POST /api/Users/login`**
    -   **Description:** Authenticates a user and returns a JWT token.
    -   **Authorization:** None (Publicly accessible).
    -   **Request Body:** `UserLoginDto` (includes email, password).
-   **`PUT /api/Users/profile`**
    -   **Description:** Updates the profile of the authenticated user.
    -   **Authorization:** Authenticated users.
    -   **Request Body:** `UpdateUserProfileDto` (includes full name, email, phone number, campus ID).
-   **`PUT /api/Users/change-password`**
    -   **Description:** Changes the password of the authenticated user.
    -   **Authorization:** Authenticated users.
    -   **Request Body:** `ChangePasswordDto` (includes old password, new password).

### NotificationsController (`/api/notifications`)

-   **`GET /api/notifications?unreadOnly={unreadOnly}`**
    -   **Description:** Retrieves notifications for the authenticated user. Optionally filters for unread notifications.
    -   **Authorization:** Authenticated users.
    -   **Query Parameter:** `unreadOnly` (boolean, optional, default false).
-   **`GET /api/notifications/unread-count`**
    -   **Description:** Retrieves the count of unread notifications for the authenticated user.
    -   **Authorization:** Authenticated users.
-   **`PUT /api/notifications/{id}/read`**
    -   **Description:** Marks a specific notification as read.
    -   **Authorization:** Authenticated users.

### ReportsController (`/api/reports`)

-   **`GET /api/reports/dashboard`**
    -   **Description:** Retrieves dashboard statistics based on the authenticated user's role and campus.
    -   **Authorization:** Authenticated users (access determined by role and campus claims).

### ReturnRecordsController (`/api/return-records`)

-   **`POST /api/return-records`**
    -   **Description:** Creates a new return record.
    -   **Authorization:** `Staff`, `Admin`
    -   **Request Body:** `CreateReturnRecordRequest` (contains `FoundItemId`, `LostItemId`, `ReceiverId`, `ReturnDate`, `Note`).
-   **`GET /api/return-records`**
    -   **Description:** Retrieves all return records.
    -   **Authorization:** `Staff`, `Admin`
-   **`GET /api/return-records/{id}`**
    -   **Description:** Retrieves a specific return record by ID.
    -   **Authorization:** Authenticated users (Owner, Admin, Staff).
-   **`GET /api/return-records/my-returns`**
    -   **Description:** Retrieves all return records where the authenticated user is the receiver.
    -   **Authorization:** Authenticated users.
-   **`PUT /api/return-records/{id}`**
    -   **Description:** Updates an existing return record.
    -   **Authorization:** `Admin`, `Staff`
    -   **Request Body:** `UpdateReturnRecordRequest` (contains `Note`).

### UserActivityController (`/api/user-activity`)

-   **`GET /api/user-activity`**
    -   **Description:** Retrieves all claim requests and lost item requests made by the authenticated user, including their associated action logs.
    -   **Authorization:** Authenticated users.
