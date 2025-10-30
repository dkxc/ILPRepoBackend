# Project API Documentation

## Overview
This API provides endpoints to retrieve and update project details including project information, batch names, trainee list, number of trainees, tech stack, and project links. It also allows for team management operations such as removing teammates from projects and managing document requests with file uploads. Additional utility endpoints are provided for retrieving tech stack details, link types, and document types for dropdown population.

## Architecture
The implementation follows the **CQRS (Command Query Responsibility Segregation)** pattern with Clean Architecture principles.

## API Endpoints

### Project Management

### 1. Get Project Details
**Endpoint:** `GET /api/projects/{id}`

**Description:** Retrieves detailed information about a specific project.

### 2. Get Project Tech Stack Details
**Endpoint:** `GET /api/projects/{id}/tech-stack`

**Description:** Retrieves the technology stack details for a specific project. Useful for populating edit forms or displaying tech stack information separately.

### 3. Update Project Links
**Endpoint:** `PUT /api/projects/{id}/links`

**Description:** Updates project links for a specific project.

### 4. Update Project Technology Stack
**Endpoint:** `PUT /api/projects/{id}/tech-stack`

**Description:** Updates the technology stack for a specific project.

### 5. Remove Teammate from Project
**Endpoint:** `DELETE /api/projects/{id}/teammates/{traineeId}`

**Description:** Removes a teammate (trainee) from a specific project.

### Link Management

### 6. Get All Link Types
**Endpoint:** `GET /api/links/types`

**Description:** Retrieves all available link types for populating dropdown menus.

### Document Management

### 7. Get All Document Types
**Endpoint:** `GET /api/documents/types`

**Description:** Retrieves all available document types/names and their file formats for populating dropdown menus. This endpoint is typically used when creating document requests to show users what types of documents are available and what format they should be in.

**Success Response:**
```json
{
  "success": true,
  "message": "Document types retrieved successfully",
  "statusCode": 200,
  "data": [
    {
      "id": 1,
      "name": "Project Proposal",
      "type": "PDF"
    },
    {
      "id": 2,
      "name": "Technical Specification",
      "type": "Word"
    },
    {
      "id": 3,
      "name": "Final Report",
      "type": "PDF"
    },
    {
      "id": 4,
      "name": "Presentation Slides",
      "type": "PowerPoint"
    },
    {
      "id": 5,
      "name": "Budget Spreadsheet",
      "type": "Excel"
    }
  ]
}
```

**Response Properties:**
- `id` (int) - Document type identifier (use this as DocumentId when creating document requests)
- `name` (string) - Display name for the document type
- `type` (string, nullable) - File format/type (e.g., "PDF", "Word", "Excel", "PowerPoint")

**Common File Types:**
- **PDF** - Portable Document Format
- **Word** - Microsoft Word Document
- **Excel** - Microsoft Excel Spreadsheet
- **PowerPoint** - Microsoft PowerPoint Presentation
- **Text** - Plain Text File
- **Image** - Image files (JPG, PNG, etc.)
- **Video** - Video files
- **Archive** - Compressed files (ZIP, RAR, etc.)

**Use Cases:**
- Populate "Document Type" dropdown in create document request forms
- Display document type and expected file format
- Guide users on what file format to submit
- Validate document type selection
- Show file format badges/icons in UI

**Example Usage in Form:**
```html
<select name="documentType">
  <option value="">Select Document Type</option>
  <!-- Loop through API response data -->
  <option value="1">Project Proposal (PDF)</option>
  <option value="2">Technical Specification (Word)</option>
  <option value="3">Final Report (PDF)</option>
  <option value="4">Presentation Slides (PowerPoint)</option>
  <option value="5">Budget Spreadsheet (Excel)</option>
</select>
```

**Enhanced UI Example:**
```html
<select id="documentTypeDropdown">
  <option value="">Select Document Type</option>
  <!-- Populated via JavaScript with type badges -->
</select>

<script>
fetch('/api/documents/types')
  .then(res => res.json())
  .then(data => {
    const select = document.getElementById('documentTypeDropdown');
    data.data.forEach(doc => {
      const option = document.createElement('option');
      option.value = doc.id;
      option.textContent = `${doc.name}${doc.type ? ' (' + doc.type + ')' : ''}`;
      select.appendChild(option);
    });
  });
</script>
```

### 8. Get Document Template Link
**Endpoint:** `GET /api/documents/{id}/link`

**Description:** Retrieves the template download link for a specific document type, including the file format information.

**Parameters:**
- `id` (int, path) - The document type identifier

**Success Response:**
```json
{
  "success": true,
  "message": "Document link retrieved successfully",
  "statusCode": 200,
  "data": {
    "id": 1,
    "name": "Project Proposal",
    "type": "PDF",
    "link": "https://example.com/templates/project-proposal-template.pdf"
  }
}
```

**Response Properties:**
- `id` (int) - Document type identifier
- `name` (string) - Document type name
- `type` (string, nullable) - File format/type (e.g., "PDF", "Word", "Excel")
- `link` (string, nullable) - URL to the template file for download

**Error Response (Document Not Found):**
```json
{
  "success": false,
  "message": "Document with id (999) was not found.",
  "statusCode": 404,
  "data": null
}
```

**Use Cases:**
- Provide download button/link for document templates
- Display template availability with file type information
- Direct users to downloadable resources
- Show file format icon/badge next to download button

**Example Usage with File Type Badge:**
```html
<div id="templateSection"></div>

<script>
async function loadTemplateInfo(documentId) {
  const response = await fetch(`/api/documents/${documentId}/link`);
  const data = await response.json();
  
  if (data.success && data.data.link) {
    const section = document.getElementById('templateSection');
    section.innerHTML = `
      <div class="template-download">
        <span class="file-type-badge">${data.data.type || 'File'}</span>
        <span>${data.data.name}</span>
        <button onclick="window.open('${data.data.link}', '_blank')">
          ?? Download Template
        </button>
      </div>
    `;
  }
}
</script>

<style>
.file-type-badge {
  background: #007bff;
  color: white;
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 0.8em;
  margin-right: 8px;
}
</style>
```

### Document Request Management

### 9. Create Document Request
**Endpoint:** `POST /api/documentrequests`

**Description:** Creates a document request with optional file upload. The system automatically finds the project ID based on the batch ID.

### 10. Update Document Request
**Endpoint:** `PUT /api/documentrequests/{id}`

**Description:** Updates a document request with optional file replacement. Automatically deletes old file when a new file is uploaded or when explicitly requested.

## Response Status Codes

- **200 OK** - Request successful
- **400 Bad Request** - Invalid request data or validation errors
- **404 Not Found** - Resource with specified ID not found
- **500 Internal Server Error** - An error occurred while processing the request

## Usage Examples

### Get All Document Types with File Types

#### Using cURL
```bash
curl -X GET "https://localhost:5001/api/documents/types" \
  -H "accept: application/json"
```

#### Using C# HttpClient
```csharp
var client = new HttpClient();
var response = await client.GetAsync("https://localhost:5001/api/documents/types");
var content = await response.Content.ReadAsStringAsync();
var result = JsonSerializer.Deserialize<ApiResponse<List<DocumentTypeDto>>>(content);

// Populate dropdown with type information
foreach (var docType in result.Data)
{
    var displayText = $"{docType.Name}";
    if (!string.IsNullOrEmpty(docType.Type))
    {
        displayText += $" ({docType.Type})";
    }
    Console.WriteLine($"{docType.Id}: {displayText}");
}
```

#### Using JavaScript/Fetch with File Type Badges
```javascript
// Fetch document types and display with file type badges
fetch('https://localhost:5001/api/documents/types')
  .then(response => response.json())
  .then(data => {
    if (data.success) {
      const selectElement = document.getElementById('documentTypeDropdown');
      
      // Clear existing options
      selectElement.innerHTML = '<option value="">Select Document Type</option>';
      
      // Populate dropdown with type information
      data.data.forEach(docType => {
        const option = document.createElement('option');
        option.value = docType.id;
        
        // Include file type in display text
        let displayText = docType.name;
        if (docType.type) {
          displayText += ` (${docType.type})`;
        }
        option.textContent = displayText;
        
        // Store type as data attribute for later use
        option.setAttribute('data-type', docType.type || '');
        
        selectElement.appendChild(option);
      });
    }
  })
  .catch(error => console.error('Error:', error));
```

#### Using React with File Type Display
```javascript
import { useState, useEffect } from 'react';

function DocumentTypeDropdown({ onSelect }) {
  const [documentTypes, setDocumentTypes] = useState([]);
  const [selectedType, setSelectedType] = useState('');

  useEffect(() => {
    fetch('https://localhost:5001/api/documents/types')
      .then(response => response.json())
      .then(data => {
        if (data.success) {
          setDocumentTypes(data.data);
        }
      })
      .catch(error => console.error('Error:', error));
  }, []);

  const handleChange = (e) => {
    const value = e.target.value;
    setSelectedType(value);
    const selected = documentTypes.find(dt => dt.id === parseInt(value));
    if (onSelect) {
      onSelect(selected);
    }
  };

  const getFileTypeBadgeColor = (type) => {
    const colors = {
      'PDF': '#dc3545',
      'Word': '#0d6efd',
      'Excel': '#198754',
      'PowerPoint': '#fd7e14',
      'Text': '#6c757d'
    };
    return colors[type] || '#6c757d';
  };

  return (
    <div className="document-type-selector">
      <select 
        value={selectedType} 
        onChange={handleChange}
        className="form-control"
      >
        <option value="">Select Document Type</option>
        {documentTypes.map(docType => (
          <option key={docType.id} value={docType.id}>
            {docType.name} {docType.type && `(${docType.type})`}
          </option>
        ))}
      </select>
      
      {selectedType && (
        <div className="selected-info">
          {documentTypes.find(dt => dt.id === parseInt(selectedType))?.type && (
            <span 
              className="badge"
              style={{ 
                backgroundColor: getFileTypeBadgeColor(
                  documentTypes.find(dt => dt.id === parseInt(selectedType))?.type
                )
              }}
            >
              {documentTypes.find(dt => dt.id === parseInt(selectedType))?.type}
            </span>
          )}
        </div>
      )}
    </div>
  );
}
```

## Features

### Document Management Endpoints
1. **Document Types with File Format** - Retrieve all document types with their expected file formats
2. **Document Template Links** - Get download links for document templates
3. **File Type Display** - Show users what format is expected
4. **Dynamic Content** - Document types and formats managed in database
5. **Template Availability** - Check if templates exist before showing download option

### Document Types Endpoint Enhancements
- **File Type Information** - Returns the expected file format (PDF, Word, Excel, etc.)
- **User Guidance** - Helps users understand what format to submit
- **UI Enhancement** - Enables file type badges/icons in the interface
- **Validation Support** - Can validate uploaded file types against expected types
- **Alphabetical Order** - Returns document types sorted by name

### Document Link Endpoint Enhancements
- **File Type Context** - Includes file type in template download response
- **Better UX** - Users know what format the template will be in
- **Icon/Badge Support** - Frontend can display appropriate file type icons

## Workflow Examples

### Creating Document Request with File Type Awareness
1. **Fetch Document Types:** `GET /api/documents/types`
2. **Display with File Type:** Show document name and expected format (e.g., "Project Proposal (PDF)")
3. **User Selects Type:** User chooses document type knowing the expected format
4. **Get Template Link:** `GET /api/documents/{id}/link` - Returns template with type info
5. **Show Download with Type:** Display "Download PDF Template" button
6. **User Downloads:** User downloads the correctly formatted template
7. **User Uploads:** User uploads completed document in correct format
8. **Create Request:** `POST /api/documentrequests` with file

### Enhanced Document Selection Form
```html
<form id="documentRequestForm">
  <div class="form-group">
    <label>Document Type:</label>
    <select id="documentType" onchange="handleDocumentTypeChange()">
      <option value="">Select Document Type</option>
      <!-- Populated dynamically -->
    </select>
    <small id="fileTypeHint" class="form-text text-muted"></small>
  </div>
  
  <div id="templateSection" style="display:none;">
    <button type="button" onclick="downloadTemplate()" class="btn btn-info">
      <span id="fileTypeIcon">??</span>
      <span id="downloadText">Download Template</span>
    </button>
  </div>
  
  <div class="form-group">
    <label>Upload Document:</label>
    <input type="file" id="fileUpload" accept="" />
    <small class="form-text text-muted">
      Please upload in <span id="expectedFormat">the correct</span> format
    </small>
  </div>
  
  <button type="submit" class="btn btn-primary">Submit Request</button>
</form>

<script>
let currentDocumentType = null;

function handleDocumentTypeChange() {
  const select = document.getElementById('documentType');
  const selectedOption = select.options[select.selectedIndex];
  const documentId = selectedOption.value;
  const fileType = selectedOption.getAttribute('data-type');
  
  if (documentId && fileType) {
    // Update UI with file type information
    document.getElementById('fileTypeHint').textContent = 
      `Expected format: ${fileType}`;
    document.getElementById('expectedFormat').textContent = fileType;
    
    // Set file input accept attribute
    const acceptMap = {
      'PDF': '.pdf',
      'Word': '.doc,.docx',
      'Excel': '.xls,.xlsx',
      'PowerPoint': '.ppt,.pptx',
      'Text': '.txt'
    };
    document.getElementById('fileUpload').accept = acceptMap[fileType] || '';
    
    // Load template information
    loadTemplateInfo(documentId, fileType);
  } else {
    document.getElementById('templateSection').style.display = 'none';
    document.getElementById('fileTypeHint').textContent = '';
  }
}

async function loadTemplateInfo(documentId, fileType) {
  const response = await fetch(`/api/documents/${documentId}/link`);
  const data = await response.json();
  
  if (data.success && data.data.link) {
    currentDocumentType = data.data;
    
    // Update download button text and icon
    const iconMap = {
      'PDF': '??',
      'Word': '??',
      'Excel': '??',
      'PowerPoint': '??',
      'Text': '??'
    };
    
    document.getElementById('fileTypeIcon').textContent = 
      iconMap[fileType] || '??';
    document.getElementById('downloadText').textContent = 
      `Download ${fileType} Template`;
    document.getElementById('templateSection').style.display = 'block';
  }
}

function downloadTemplate() {
  if (currentDocumentType && currentDocumentType.link) {
    window.open(currentDocumentType.link, '_blank');
  }
}
</script>
```

## Database Schema

### Documents Table
```sql
CREATE TABLE documents (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    type VARCHAR(50) NULL,  -- NEW COLUMN: File type (PDF, Word, Excel, etc.)
    link TEXT NULL,
    upload_date TIMESTAMP NOT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);
```

**Note:** A database migration is required to add the `type` column. See `MIGRATION_NOTES.md` for details.

## Testing with Swagger

1. Run the application
2. Navigate to `https://localhost:{port}/swagger`
3. Locate the document endpoints:
   - `GET /api/documents/types` - Get all document types with file formats
   - `GET /api/documents/{id}/link` - Get document template link with type
4. Click "Try it out" and "Execute" to see the type field in responses

## Notes

- The API uses PostgreSQL as the database (configured in connection string)
- All dates are in UTC and returned in ISO 8601 format
- Document types are managed in the `documents` table
- Document types are returned in alphabetical order
- The `type` field indicates the expected file format (PDF, Word, Excel, PowerPoint, etc.)
- File type information helps users understand submission requirements
- Frontend can use type information to display appropriate icons and validation
- Template links can be null if no template is available
- File type can be null for documents without a specific format requirement
