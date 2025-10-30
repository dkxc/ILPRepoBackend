# IMPORTANT: Team Leader Authorization Required

## Critical Security Requirement

You mentioned that **ONLY Team Leaders** can:
1. Submit documents for their project
2. Edit/Delete document submissions
3. Update project details (links, tech stack)
4. Add/Remove teammates

## What Needs to Be Implemented

### 1. Add TraineeId to All Operations

**Current Problem:** APIs don't verify WHO is performing the action

**Solution:** Add `TraineeId` parameter to all operations and verify they are Team Leader

### 2. Authorization Check Before Each Operation

```csharp
// Pseudo-code for what needs to happen:
public async Task<Response> SubmitDocument(RequestDto dto)
{
    // 1. Get the project ID from the document request
    var projectId = await GetProjectId(dto.RequestId);
    
    // 2. Check if trainee is in project_team table
    var teamMember = await GetProjectTeamMember(projectId, dto.TraineeId);
    
    // 3. Check if their role is TeamLeader
    if (teamMember == null || teamMember.Role != "TeamLeader")
    {
        return Forbidden("Only Team Leader can submit documents");
    }
    
    // 4. Proceed with operation
    // ...
}
```

### 3. Database Query Needed

```sql
-- Check if trainee is Team Leader
SELECT * FROM project_team 
WHERE project_id = @projectId 
  AND trainee_id = @traineeId 
  AND role = 'TeamLeader';
```

## APIs That Need Authorization

### Document Submissions
- ? **POST /api/documentsubmissions** - Add `traineeId`, verify Team Leader
- ? **DELETE /api/documentsubmissions/{id}** - Verify submitter is Team Leader

### Project Management  
- ? **PUT /api/projects/{id}/links** - Add `traineeId`, verify Team Leader
- ? **PUT /api/projects/{id}/tech-stack** - Add `traineeId`, verify Team Leader
- ? **DELETE /api/projects/{id}/teammates/{traineeId}** - Add `requestingTraineeId`, verify Team Leader

## Implementation Steps

### Step 1: Update DTOs
Add `TraineeId` to all request DTOs:
```csharp
public class UploadDocumentSubmissionDto
{
    public int RequestId { get; set; }
    public int DocumentId { get; set; }
    public int TraineeId { get; set; }  // ? ADD THIS
    public IFormFile File { get; set; }
}
```

### Step 2: Create Authorization Helper
```csharp
public async Task<bool> IsTeamLeader(int traineeId, int projectId)
{
    var teamMember = await _dbContext.ProjectTeams
        .Where(pt => pt.ProjectId == projectId && pt.TraineeId == traineeId)
        .FirstOrDefaultAsync();
    
    return teamMember?.Role == ProjectRole.TeamLeader;
}
```

### Step 3: Add Checks to Handlers
```csharp
// In every handler, before processing:
var isTeamLeader = await IsTeamLeader(request.TraineeId, projectId);
if (!isTeamLeader)
{
    return new ApiResponse("Unauthorized: Only Team Leader can perform this action", 403);
}
```

### Step 4: Update Frontend
```javascript
// Frontend must send traineeId
const currentUser = getCurrentUser();

formData.append('TraineeId', currentUser.traineeId);
```

## What I Started to Implement

I created these files (but they have compilation errors due to missing context):
- `IlpRepoBackend.Application\Services\ProjectAuthorizationService.cs`
- `TEAM_LEADER_AUTHORIZATION.md`

**These need to be completed with:**
1. Proper `ProjectRole` enum reference
2. Full `IProjectRepository` interface implementation
3. Controller updates to pass `TraineeId`
4. Handler updates to call authorization service

## Recommended Approach

Since I don't have full context of your existing code, here's what YOU should do:

### 1. Add TraineeId Column Check
Make sure `project_team` table has a `role` column:
```sql
ALTER TABLE project_team ADD COLUMN role VARCHAR(50);
UPDATE project_team SET role = 'Member' WHERE role IS NULL;
```

### 2. Create Simple Helper Method
```csharp
private async Task<bool> IsTeamLeader(int traineeId, int projectId)
{
    return await _context.ProjectTeams
        .AnyAsync(pt => 
            pt.ProjectId == projectId && 
            pt.TraineeId == traineeId && 
            pt.Role == ProjectRole.TeamLeader);
}
```

### 3. Add to Each Handler
Before any document submission or project update:
```csharp
if (!await IsTeamLeader(request.TraineeId, projectId))
{
    return new ApiResponse<T>("Unauthorized", 403);
}
```

### 4. Update All DTOs
Add `TraineeId` to:
- `UploadDocumentSubmissionDto`
- `UpdateProjectLinksDto`
- `UpdateProjectTechStackDto`
- `RemoveTeammateDto`

## Security Implications

**Without this authorization:**
- ? Any trainee can submit documents for any project
- ? Any trainee can modify any project's details
- ? No accountability for who made changes

**With proper authorization:**
- ? Only Team Leader can submit/modify
- ? Clear audit trail of who did what
- ? Enforces team hierarchy
- ? Prevents unauthorized changes

## Next Steps

1. Review the `ProjectTeam` entity and ensure it has a `Role` property
2. Ensure `ProjectRole` enum exists with `TeamLeader` value
3. Implement the authorization helper method
4. Add checks to all handlers
5. Update all DTOs to include `TraineeId`
6. Test with both Team Leader and regular member accounts

The code I created is a **starting point** but needs to be integrated with your existing repository pattern and entity structure.
