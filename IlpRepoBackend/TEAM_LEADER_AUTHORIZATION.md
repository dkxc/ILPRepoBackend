# Team Leader Authorization Implementation

## Overview

Only **Team Leaders** can perform certain operations on their projects. This document explains the authorization requirements and implementation.

## Authorization Rules

### Who is a Team Leader?
A trainee is a Team Leader if:
1. They are in the project's team (`ProjectTeam` record exists)
2. Their role is `ProjectRole.TeamLeader`

### What Can Team Leaders Do?

#### Document Operations
- ? **Submit documents** for their project
- ? **Delete submissions** they created
- ? Regular trainees **CANNOT** submit or delete documents

#### Project Management Operations  
- ? **Update project links**
- ? **Update project tech stack**
- ? **Remove teammates** from the project
- ? Regular trainees **CANNOT** modify project details

## Implementation

### Authorization Service

**IProjectAuthorizationService** provides two key methods:

```csharp
public interface IProjectAuthorizationService
{
    // Check if trainee is Team Leader of the project
    Task<bool> IsTeamLeaderAsync(int traineeId, int projectId);
    
    // Check if trainee is in the project (any role)
    Task<bool> IsInProjectAsync(int traineeId, int projectId);
}
```

### How It Works

**Step 1: Query ProjectTeam**
```csharp
var projectTeam = await _projectRepository.GetProjectTeamMemberAsync(projectId, traineeId);
```

**Step 2: Check Role**
```csharp
if (projectTeam == null)
    return false; // Not in project

return projectTeam.Role == ProjectRole.TeamLeader;
```

## API Changes

### All Updated Endpoints Now Require `TraineeId`

#### 1. Upload Document Submission
**Before:**
```json
POST /api/documentsubmissions
{
  "requestId": 5,
  "documentId": 1,
  "file": [upload]
}
```

**After:**
```json
POST /api/documentsubmissions
{
  "requestId": 5,
  "documentId": 1,
  "traineeId": 42,  ? REQUIRED: WHO is submitting
  "file": [upload]
}
```

**Authorization Check:**
- Validates traineeId is Team Leader of the project
- Returns 403 if not authorized

#### 2. Delete Document Submission
**Endpoint:** `DELETE /api/documentsubmissions/{id}`

**Authorization Check:**
- Gets the submission record
- Checks if the trainee who submitted is Team Leader
- Returns 403 if not authorized

#### 3. Update Project Links
**Before:**
```json
PUT /api/projects/{id}/links
{
  "projectId": 10,
  "projectLinks": [...]
}
```

**After:**
```json
PUT /api/projects/{id}/links
{
  "projectId": 10,
  "traineeId": 42,  ? REQUIRED: WHO is making changes
  "projectLinks": [...]
}
```

**Authorization Check:**
- Validates traineeId is Team Leader of the project
- Returns 403 if not authorized

#### 4. Update Project Tech Stack
**After:**
```json
PUT /api/projects/{id}/tech-stack
{
  "projectId": 10,
  "traineeId": 42,  ? REQUIRED
  "techStack": ["React", "Node.js"]
}
```

#### 5. Remove Teammate
**After:**
```json
DELETE /api/projects/{id}/teammates/{traineeId}
?requestingTraineeId=42  ? REQUIRED: WHO is removing
```

## Error Responses

### 403 Forbidden - Not Team Leader

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

**Project Updates:**
```json
{
  "success": false,
  "message": "Unauthorized: Only the Team Leader can update project details",
  "statusCode": 403,
  "data": null
}
```

## Database Requirements

### ProjectTeam Table
```sql
CREATE TABLE project_team (
    project_id INTEGER NOT NULL,
    trainee_id INTEGER NOT NULL,
    role VARCHAR(50) NOT NULL,  -- 'TeamLeader', 'Member', etc.
    created_at TIMESTAMP,
    updated_at TIMESTAMP,
    PRIMARY KEY (project_id, trainee_id)
);
```

**Example Data:**
```sql
-- Project 10 team
INSERT INTO project_team VALUES (10, 42, 'TeamLeader', NOW(), NOW());  -- Trainee 42 is leader
INSERT INTO project_team VALUES (10, 43, 'Member', NOW(), NOW());
INSERT INTO project_team VALUES (10, 44, 'Member', NOW(), NOW());

-- Only trainee 42 can submit docs, update project details
```

## Testing Scenarios

### Scenario 1: Team Leader Submits Document ?
```
Given: Trainee 42 is Team Leader of Project 10
When: Trainee 42 submits BRD
Then: Submission succeeds
```

### Scenario 2: Regular Member Tries to Submit ?
```
Given: Trainee 43 is Member (not leader) of Project 10
When: Trainee 43 tries to submit BRD
Then: Returns 403 Forbidden
```

### Scenario 3: Trainee from Different Project ?
```
Given: Trainee 55 is Team Leader of Project 11 (not Project 10)
When: Trainee 55 tries to submit doc for Project 10
Then: Returns 403 Forbidden
```

### Scenario 4: Team Leader Updates Project ?
```
Given: Trainee 42 is Team Leader of Project 10
When: Trainee 42 updates project links
Then: Update succeeds
```

### Scenario 5: Regular Member Tries to Update Project ?
```
Given: Trainee 43 is Member of Project 10
When: Trainee 43 tries to update project tech stack
Then: Returns 403 Forbidden
```

## Code Flow Example

### Document Submission with Authorization

```csharp
public async Task<ApiResponse> Handle(UploadDocumentSubmissionCommand request)
{
    // 1. Get document request
    var documentRequest = await GetDocumentRequest(request.RequestId);
    
    // 2. Get project ID from request
    var projectId = documentRequest.ProjectId;
    
    // 3. AUTHORIZATION: Check if trainee is Team Leader
    var isTeamLeader = await _authService.IsTeamLeaderAsync(
        request.TraineeId, 
        projectId
    );
    
    if (!isTeamLeader)
    {
        return Forbidden("Only Team Leader can submit");
    }
    
    // 4. Proceed with submission
    // ...
}
```

## Migration Impact

### Existing Code
If you have existing submissions/updates without TraineeId:
1. Add TraineeId to all requests
2. Frontend must pass the logged-in trainee's ID
3. API validates the trainee is authorized

### New Code
All new operations require:
- TraineeId in request
- Authorization check before processing
- Clear 403 errors if unauthorized

## Frontend Integration

### Get Current User's TraineeId
```javascript
// Assume you have current user context
const currentUser = getCurrentUser();
const traineeId = currentUser.traineeId;

// Submit document
const formData = new FormData();
formData.append('RequestId', requestId);
formData.append('DocumentId', documentId);
formData.append('TraineeId', traineeId);  // ? Add this
formData.append('File', file);

await fetch('/api/documentsubmissions', {
  method: 'POST',
  body: formData
});
```

### Handle 403 Errors
```javascript
const response = await fetch('/api/documentsubmissions', {
  method: 'POST',
  body: formData
});

const data = await response.json();

if (response.status === 403) {
  alert('You must be the Team Leader to submit documents');
  return;
}

if (data.success) {
  alert('Document submitted successfully!');
}
```

## Summary

**What Changed:**
- Added `IProjectAuthorizationService` for role checks
- All document submission/deletion requires Team Leader
- All project update operations require Team Leader
- Added `TraineeId` to all operation DTOs/Commands
- Returns 403 Forbidden if not authorized

**Why It Matters:**
- **Security**: Only authorized users can modify projects
- **Accountability**: Track who made changes
- **Team Structure**: Enforces team hierarchy
- **Data Integrity**: Prevents unauthorized modifications

**Next Steps:**
1. Update all API calls to include `TraineeId`
2. Implement similar checks for other operations
3. Add UI indicators showing who is Team Leader
4. Show/hide buttons based on user role
