# Document Requirements API

This API allows you to manage document requirements for batches and retrieve requirements for projects. When you set a requirement for a batch, it automatically creates document requests for all projects associated with that batch.

## Endpoints

### 1. Get Document Requirements for Batch (Admin View)
**GET** `/api/documentrequirements/batch/{batchId}`

Returns detailed document requirements for a batch (admin view with project details).

**Response:**
```json
{
  "succeeded": true,
  "message": "Retrieved 2 document requirements for batch 5",
  "data": [
    {
      "documentTypeId": 1,
      "documentTypeName": "Business Requirements Document",
      "batchId": 5,
      "dueDate": "2024-12-31T23:59:59Z",
      "requestDate": "2024-01-15T10:00:00Z",
      "projectIds": [10, 11, 12],
      "totalProjectsAffected": 3
    },
    {
      "documentTypeId": 2,
      "documentTypeName": "Technical Specification",
      "batchId": 5,
      "dueDate": "2025-01-10T23:59:59Z",
      "requestDate": "2024-01-20T10:00:00Z",
      "projectIds": [10, 11, 12],
      "totalProjectsAffected": 3
    }
  ]
}
```

### 2. Get Document Requirements for Project (Project Lead Dropdown)
**GET** `/api/documentrequirements/project/{projectId}`

Returns document requirements for a specific project, optimized for dropdown selection with urgency indicators and submission status.

**Response:**
```json
{
  "succeeded": true,
  "message": "Retrieved 2 document requirements for project 'E-Commerce Platform'",
  "data": [
    {
      "documentRequestId": 15,
      "documentTypeId": 1,
      "documentTypeName": "Business Requirements Document",
      "documentTemplateUrl": "https://jbbaufdzfgglkjqiwveb.supabase.co/storage/v1/object/public/docs/uuid_template.pdf",
      "projectId": 10,
      "projectName": "E-Commerce Platform",
      "dueDate": "2024-12-20T23:59:59Z",
      "requestDate": "2024-01-15T10:00:00Z",
      "isOverdue": true,
      "daysUntilDue": -2,
      "urgencyLevel": "Overdue",
      "hasSubmission": false,
      "lastSubmissionDate": null
    },
    {
      "documentRequestId": 16,
      "documentTypeId": 2,
      "documentTypeName": "Technical Specification",
      "documentTemplateUrl": "https://jbbaufdzfgglkjqiwveb.supabase.co/storage/v1/object/public/docs/uuid_template2.pdf",
      "projectId": 10,
      "projectName": "E-Commerce Platform",
      "dueDate": "2024-12-25T23:59:59Z",
      "requestDate": "2024-01-20T10:00:00Z",
      "isOverdue": false,
      "daysUntilDue": 5,
      "urgencyLevel": "Medium",
      "hasSubmission": true,
      "lastSubmissionDate": "2024-12-18T14:30:00Z"
    }
  ]
}
```

**Urgency Levels:**
- `Overdue`: Past due date
- `Critical`: Due within 1 day
- `High`: Due within 3 days
- `Medium`: Due within 7 days
- `Low`: More than 7 days remaining

### 3. Set Document Requirement for Batch
**POST** `/api/documentrequirements`

Creates document requirements for all projects in a specified batch.

**Request Body:**
```json
{
  "documentTypeId": 1,
  "batchId": 5,
  "dueDate": "2024-12-31T23:59:59Z"
}
```

**Response:**
```json
{
  "succeeded": true,
  "message": "Document requirement set successfully for 3 projects in batch 5",
  "data": {
    "documentTypeId": 1,
    "documentTypeName": "Business Requirements Document",
    "batchId": 5,
    "dueDate": "2024-12-31T23:59:59Z",
    "requestDate": "2024-01-15T10:00:00Z",
    "projectIds": [10, 11, 12],
    "totalProjectsAffected": 3
  }
}
```

**Example using curl:**
```bash
curl -X POST "https://localhost:7001/api/documentrequirements" \
  -H "Content-Type: application/json" \
  -d '{
    "documentTypeId": 1,
    "batchId": 5,
    "dueDate": "2024-12-31T23:59:59Z"
  }'
```

### 4. Update Document Requirement Deadline
**PUT** `/api/documentrequirements`

Updates the due date for an existing document requirement across all projects in the batch.

**Request Body:**
```json
{
  "documentTypeId": 1,
  "batchId": 5,
  "dueDate": "2025-01-15T23:59:59Z"
}
```

**Response:**
```json
{
  "succeeded": true,
  "message": "Document requirement updated successfully for 3 projects in batch 5",
  "data": {
    "documentTypeId": 1,
    "documentTypeName": "Business Requirements Document",
    "batchId": 5,
    "dueDate": "2025-01-15T23:59:59Z",
    "requestDate": "2024-01-15T10:00:00Z",
    "projectIds": [10, 11, 12],
    "totalProjectsAffected": 3
  }
}
```

### 5. Delete Document Requirement for Batch
**DELETE** `/api/documentrequirements?documentTypeId={id}&batchId={id}`

Removes document requirements for all projects in the specified batch.

**Query Parameters:**
- `documentTypeId` (int): ID of the document type
- `batchId` (int): ID of the batch

**Response:**
```json
{
  "succeeded": true,
  "message": "Document requirement deleted successfully. Removed 3 document requests from batch 5",
  "data": true
}
```

## Frontend Integration Examples

### For Admin Dashboard (Batch Requirements):
```javascript
const getBatchDocumentRequirements = async (batchId) => {
  const response = await fetch(`/api/documentrequirements/batch/${batchId}`);
  const result = await response.json();
  
  if (result.succeeded) {
    console.log(`Found ${result.data.length} requirements for batch ${batchId}`);
    result.data.forEach(req => {
      console.log(`${req.documentTypeName}: ${req.totalProjectsAffected} projects affected`);
    });
  }
  
  return result;
};
```

### For Project Lead Dropdown:
```javascript
const populateProjectDocumentDropdown = async (projectId) => {
  const response = await fetch(`/api/documentrequirements/project/${projectId}`);
  const result = await response.json();
  
  if (result.succeeded) {
    const dropdown = document.getElementById('documentRequirements');
    dropdown.innerHTML = '<option value="">Select a document to submit</option>';
    
    result.data.forEach(req => {
      const option = document.createElement('option');
      option.value = req.documentRequestId;
      option.textContent = `${req.documentTypeName} (${req.urgencyLevel})`;
      option.className = req.urgencyLevel.toLowerCase();
      
      // Add urgency and submission status indicators
      if (req.isOverdue) {
        option.textContent += ' - OVERDUE';
        option.style.color = 'red';
      } else if (req.urgencyLevel === 'Critical') {
        option.style.color = 'orange';
      }
      
      if (req.hasSubmission) {
        option.textContent += ' ? SUBMITTED';
        option.style.fontWeight = 'bold';
      }
      
      // Add template link and document request ID as data attributes
      if (req.documentTemplateUrl) {
        option.dataset.templateUrl = req.documentTemplateUrl;
      }
      option.dataset.documentRequestId = req.documentRequestId;
      option.dataset.dueDate = req.dueDate;
      
      dropdown.appendChild(option);
    });
  }
};

// Usage
populateProjectDocumentDropdown(10);

// When user selects a document, show template download link and submission details
document.getElementById('documentRequirements').addEventListener('change', (e) => {
  const selectedOption = e.target.selectedOptions[0];
  const templateUrl = selectedOption?.dataset.templateUrl;
  const documentRequestId = selectedOption?.dataset.documentRequestId;
  const dueDate = selectedOption?.dataset.dueDate;
  
  if (templateUrl) {
    document.getElementById('templateLink').href = templateUrl;
    document.getElementById('templateLink').style.display = 'inline';
  }
  
  if (documentRequestId) {
    // Store for submission
    document.getElementById('submissionForm').dataset.documentRequestId = documentRequestId;
  }
  
  if (dueDate) {
    document.getElementById('dueDateInfo').textContent = `Due: ${new Date(dueDate).toLocaleDateString()}`;
  }
});
```

### With Enhanced Styling:
```css
.overdue { color: #dc3545; font-weight: bold; }
.critical { color: #fd7e14; font-weight: bold; }
.high { color: #ffc107; }
.medium { color: #17a2b8; }
.low { color: #28a745; }

/* Submitted items styling */
option[data-has-submission="true"] {
  background-color: #e8f5e8;
  font-style: italic;
}
```

### Project-specific Requirements with Filters:
```javascript
const getProjectRequirementsWithFilters = async (projectId, filters = {}) => {
  const response = await fetch(`/api/documentrequirements/project/${projectId}`);
  const result = await response.json();
  
  if (result.succeeded) {
    let requirements = result.data;
    
    // Filter by urgency
    if (filters.urgency) {
      requirements = requirements.filter(req => req.urgencyLevel === filters.urgency);
    }
    
    // Filter by submission status
    if (filters.showOnlyPending) {
      requirements = requirements.filter(req => !req.hasSubmission);
    }
    
    // Filter by overdue
    if (filters.showOnlyOverdue) {
      requirements = requirements.filter(req => req.isOverdue);
    }
    
    return {
      ...result,
      data: requirements
    };
  }
  
  return result;
};

// Usage examples
await getProjectRequirementsWithFilters(10, { showOnlyPending: true });
await getProjectRequirementsWithFilters(10, { urgency: 'Critical' });
await getProjectRequirementsWithFilters(10, { showOnlyOverdue: true });
```

## Key Differences Between Endpoints

### Batch Endpoint (`/batch/{batchId}`):
- **Purpose**: Admin view to see all requirements set for a batch
- **Data**: Aggregated view showing which document types are required and how many projects are affected
- **Use Case**: Managing requirements at batch level, seeing overview of all projects

### Project Endpoint (`/project/{projectId}`):
- **Purpose**: Project lead view for document submission
- **Data**: Individual document requests with submission status and urgency
- **Use Case**: Dropdown selection for document submission, tracking individual project progress

## How It Works

### Batch to Project Relationship
The system automatically finds all projects associated with a batch by:
1. Finding all trainees in the specified batch
2. Finding all projects where those trainees are team members
3. Creating/updating/deleting document requests for those projects

### Project-Specific Data
For project endpoints:
1. Retrieves all `DocumentRequest` records for the specific project
2. Includes submission status from `DocumentSubmission` records
3. Provides urgency calculations and template access
4. Optimized for dropdown selection and submission workflows

## Error Handling

**Common Error Responses:**

**Project Not Found:**
```json
{
  "succeeded": false,
  "message": "Project with ID 999 not found",
  "data": null
}
```

**Document Type Not Found:**
```json
{
  "succeeded": false,
  "message": "Document type with ID 999 not found",
  "data": null
}
```

**No Projects in Batch:**
```json
{
  "succeeded": false,
  "message": "No projects found for batch ID 5",
  "data": null
}
```

**No Requirements for Project:**
```json
{
  "succeeded": true,
  "message": "No document requirements found for this project",
  "data": []
}
```

## Authentication

Currently, the API endpoints do not require authentication for testing purposes.

## Database Impact

- **GET**: Read-only operations, no database modifications
- **CREATE**: Adds multiple `DocumentRequest` records (one per project in batch)
- **UPDATE**: Modifies `DueDate` and `UpdatedAt` fields for existing records
- **DELETE**: Removes all `DocumentRequest` records for the batch/document type combination