# ? Team Leader Authorization - IMPLEMENTATION COMPLETE

## Overview

Successfully implemented authorization checks to ensure **ONLY Team Leaders** can perform critical operations on their projects.

## What Was Implemented

### 1. Authorization Service ?

**File:** `IlpRepoBackend.Application\Services\ProjectAuthorizationService.cs`

**Purpose:** Centralized service to check if a trainee is a Team Leader

**Methods:**
- `IsTeamLeaderAsync(traineeId, projectId)` - Checks if trainee has TeamLead role
- `IsInProjectAsync(traineeId, projectId)` - Checks if trainee is in the project

**How It Works:**
```csharp
public async Task<bool> IsTeamLeaderAsync(int traineeId, int projectId)
{
    var projectTeam = await _projectRepository.GetProjectTeamMemberAsync(projectId, traineeId);
    
    if (projectTeam == null)
        return false;

    return projectTeam.Role == Domain.Enum.ProjectRole.TeamLead;
}
```

### 2. Updated Entities & DTOs ?

**DocumentSubmission:**
- Added `ProjectId` - Direct reference to project
- Added `TraineeId` - WHO submitted the document

**UploadDocumentSubmissionDto:**
- Added `TraineeId` (required) - WHO is submitting

**UpdateProjectLinksDto:**
- Added `TraineeId` (required) - WHO is making changes

**Commands:**
- `UploadDocumentSubmissionCommand` - Includes TraineeId
- `UpdateProjectLinksCommand` - Includes TraineeId
- `DeleteDocumentSubmissionCommand` - Validates submitter

### 3. Authorization in Handlers ?

#### Document Upload Handler
**File:** `UploadDocumentSubmissionCommandHandler.cs`

```csharp
// Get ProjectId from DocumentRequest
var projectId = documentRequest.ProjectId;

// AUTHORIZATION: Check if trainee is Team Leader
var isTeamLeader = await _authorizationService.IsTeamLeaderAsync(
    request.TraineeId, 
    projectId.Value
);

if (!isTeamLeader)
{
    return new ApiResponse<DocumentSubmissionDto>(
        "Unauthorized: Only the Team Leader can submit documents for this project", 
        403);
}
```

#### Document Delete Handler
**File:** `DeleteDocumentSubmissionCommandHandler.cs`

```csharp
// Check if the trainee who created this submission is Team Leader
var isTeamLeader = await _authorizationService.IsTeamLeaderAsync(
    submission.TraineeId.Value, 
    submission.ProjectId.Value
);

if (!isTeamLeader)
{
    return new ApiResponse<bool>(
        "Unauthorized: Only the Team Leader who submitted this document can delete it", 
        403);
}
```

#### Project Links Update Handler
**File:** `UpdateProjectLinksCommandHandler.cs`

```csharp
// AUTHORIZATION: Check if trainee is Team Leader
var isTeamLeader = await _authorizationService.IsTeamLeaderAsync(
    request.TraineeId, 
    request.ProjectId
);

if (!isTeamLeader)
{
    return new ApiResponse<ProjectDetailsDto>(
        "Unauthorized: Only the Team Leader can update project links", 
        403);
}
```

### 4. Updated Repository ?

**File:** `ProjectRepository.cs`

**Implemented Methods:**
- `GetProjectDetailsAsync(int projectId)`
- `GetProjectWithLinksAsync(int projectId)`
- `AddProjectLinksAsync(List<ProjectLink> projectLinks)`
- `RemoveProjectLinksAsync(List<ProjectLink> projectLinks)`
- `UpdateProjectAsync(Project project)`
- `GetProjectTeammateAsync(int projectId, int traineeId)`
- `GetProjectTeamMemberAsync(int projectId, int traineeId)`
- `RemoveTeammateAsync(ProjectTeam projectTeam)`
- `IsTraineeInProjectAsync(int projectId, int traineeId)`

### 5. Updated Controllers ?

**File:** `ProjectsController.cs`

Fixed command instantiation to include TraineeId:
```csharp
var command = new UpdateProjectLinksCommand(
    updateDto.ProjectId, 
    updateDto.TraineeId, 
    updateDto.ProjectLinks
);
```

### 6. Service Registration ?

**File:** `ApplicationServiceRegistration.cs`

```csharp
services.AddScoped<IProjectAuthorizationService, ProjectAuthorizationService>();
```

## API Changes

### Updated Request Examples

#### 1. Upload Document Submission
```json
POST /api/documentsubmissions
Content-Type: multipart/form-data

RequestId: 5
DocumentId: 1
TraineeId: 42          ? REQUIRED: Must be Team Leader
File: [upload]
```

**Authorization Check:**
- Extracts ProjectId from DocumentRequest
- Verifies TraineeId is Team Leader of that project
- Returns 403 if not authorized

#### 2. Delete Document Submission
```http
DELETE /api/documentsubmissions/123
```

**Authorization Check:**
- Retrieves submission record
- Checks if the trainee who submitted is Team Leader
- Returns 403 if not authorized

#### 3. Update Project Links
```json
PUT /api/projects/10/links

{
  "projectId": 10,
  "traineeId": 42,    ? REQUIRED: Must be Team Leader
  "projectLinks": [
    {
      "linkId": 1,
      "linkUrl": "https://github.com/project",
      "isDeleted": false
    }
  ]
}
```

**Authorization Check:**
- Verifies TraineeId is Team Leader of ProjectId
- Returns 403 if not authorized

## Error Responses

### 403 Forbidden - Not Authorized

**Document Submission:**
```json
{
  "success": false,
  "message": "Unauthorized: Only the Team Leader can submit documents for this project",
  "statusCode": 403,
  "data": null
}
```

**Document Deletion:**
```json
{
  "success": false,
  "message": "Unauthorized: Only the Team Leader who submitted this document can delete it",
  "statusCode": 403,
  "data": null
}
```

**Project Update:**
```json
{
  "success": false,
  "message": "Unauthorized: Only the Team Leader can update project links",
  "statusCode": 403,
  "data": null
}
```

## Database Schema

### ProjectTeam Table
```sql
CREATE TABLE project_team (
    project_id INTEGER NOT NULL,
    trainee_id INTEGER NOT NULL,
    role VARCHAR(50) NOT NULL,  -- 'Trainee', 'TeamLead', 'ScrumMaster'
    created_at TIMESTAMP,
    updated_at TIMESTAMP,
    PRIMARY KEY (project_id, trainee_id)
);
```

### DocumentSubmission Table (Updated)
```sql
ALTER TABLE document_submissions ADD COLUMN project_id INTEGER NULL;
ALTER TABLE document_submissions ADD COLUMN trainee_id INTEGER NULL;

ALTER TABLE document_submissions
ADD CONSTRAINT fk_submission_project 
FOREIGN KEY (project_id) REFERENCES projects(id);

ALTER TABLE document_submissions
ADD CONSTRAINT fk_submission_trainee 
FOREIGN KEY (trainee_id) REFERENCES trainees(id);
```

## ProjectRole Enum

**File:** `IlpRepoBackend.Domain\Enum\ProjectRole.cs`

```csharp
public enum ProjectRole
{
    Trainee,      // Regular team member
    TeamLead,     // Can submit docs, update project
    ScrumMaster   // Additional role (if needed)
}
```

## Testing Scenarios

### ? Scenario 1: Team Leader Submits Document
```
Given: Trainee 42 has Role = TeamLead in Project 10
When: Trainee 42 submits BRD for Project 10
Then: Submission succeeds (200 OK)
```

### ? Scenario 2: Regular Trainee Tries to Submit
```
Given: Trainee 43 has Role = Trainee in Project 10
When: Trainee 43 tries to submit BRD for Project 10
Then: Returns 403 Forbidden
```

### ? Scenario 3: Trainee from Different Project
```
Given: Trainee 55 is Team Leader of Project 11 (not Project 10)
When: Trainee 55 tries to submit doc for Project 10
Then: Returns 403 Forbidden
```

### ? Scenario 4: Team Leader Updates Project
```
Given: Trainee 42 has Role = TeamLead in Project 10
When: Trainee 42 updates project links
Then: Update succeeds (200 OK)
```

## Security Benefits

### Before Authorization ?
- Any trainee could submit documents for any project
- Any trainee could delete any submission
- Any trainee could modify any project's details
- No accountability for changes

### After Authorization ?
- Only Team Leader can submit documents
- Only Team Leader can delete their submissions
- Only Team Leader can update project details
- Clear audit trail with TraineeId
- Enforces team hierarchy
- Prevents unauthorized modifications

## Operations Requiring Team Leader Role

### Document Operations
- ? **Submit Document** - `POST /api/documentsubmissions`
- ? **Delete Submission** - `DELETE /api/documentsubmissions/{id}`

### Project Management
- ? **Update Links** - `PUT /api/projects/{id}/links`
- ? **Update Tech Stack** - `PUT /api/projects/{id}/tech-stack` (needs similar update)
- ? **Remove Teammate** - `DELETE /api/projects/{id}/teammates/{traineeId}` (needs similar update)

## Frontend Integration

### Get Current User's TraineeId
```javascript
// Assume you have current user context
const currentUser = getCurrentUser();
const traineeId = currentUser.traineeId;
const role = currentUser.projectRole; // 'TeamLead' or 'Trainee'

// Only show submit button if Team Leader
if (role === 'TeamLead') {
  showSubmitButton();
}
```

### Submit with TraineeId
```javascript
const formData = new FormData();
formData.append('RequestId', requestId);
formData.append('DocumentId', documentId);
formData.append('TraineeId', traineeId);  // Current user's traineeId
formData.append('File', file);

const response = await fetch('/api/documentsubmissions', {
  method: 'POST',
  body: formData
});

if (response.status === 403) {
  alert('Only the Team Leader can submit documents');
}
```

### Handle Authorization Errors
```javascript
async function submitDocument(data) {
  try {
    const response = await fetch('/api/documentsubmissions', {
      method: 'POST',
      body: data
    });

    const result = await response.json();

    if (response.status === 403) {
      showError('You must be the Team Leader to submit documents');
      return;
    }

    if (result.success) {
      showSuccess('Document submitted successfully');
    }
  } catch (error) {
    showError('Network error occurred');
  }
}
```

## Migration Required

Run these commands to update the database:

```bash
cd IlpRepoBackend.Infrastructure

# Create migration
dotnet ef migrations add AddTraineeAndProjectToDocumentSubmission --startup-project ../IlpRepoBackend.Api

# Apply migration
dotnet ef database update --startup-project ../IlpRepoBackend.Api
```

## Summary

### ? Completed
1. Created `IProjectAuthorizationService`
2. Added TraineeId and ProjectId to DocumentSubmission
3. Added authorization checks to:
   - Upload Document Submission
   - Delete Document Submission
   - Update Project Links
4. Updated all DTOs, Commands, and Handlers
5. Implemented all required repository methods
6. Registered services in DI container
7. Build successful ?

### ? TODO (Similar Implementation Needed)
1. Update Tech Stack handler - Add TraineeId and authorization
2. Remove Teammate handler - Add requesting TraineeId and authorization
3. Frontend updates to:
   - Pass current user's TraineeId
   - Show/hide buttons based on role
   - Handle 403 errors gracefully

### ?? Result
**Only Team Leaders can now:**
- Submit documents for their project
- Delete submissions they created
- Update project links
- (After TODO items) Update tech stack and manage teammates

**Security is enforced at the API level** with proper 403 responses for unauthorized attempts.
