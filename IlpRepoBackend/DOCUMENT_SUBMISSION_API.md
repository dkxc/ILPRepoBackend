# Document Submission API

## Endpoints

### 1. Upload Document Submission
**Endpoint:** `POST /api/documentsubmissions`

**Description:** Uploads a document submission file for a document request. The system validates the request, checks the deadline, saves the file, and creates a submission record.

**Content-Type:** `multipart/form-data`

**Request Parameters:**
- `RequestId` (int, required) - The document request ID this submission is for
- `DocumentId` (int, required) - The document type ID (must match the request)
- `File` (IFormFile, required) - The file to upload

**Request Example (Form Data):**
```
RequestId: 5
DocumentId: 1
File: [file upload - required]
```

**Success Response:**
```json
{
  "success": true,
  "message": "Document submitted successfully",
  "statusCode": 200,
  "data": {
    "id": 123,
    "submissionLink": "/uploads/submissions/abc123_project_brd.pdf",
    "fileName": "project1_brd_final.pdf",
    "fileType": "PDF",
    "documentName": "BRD",
    "submissionDate": "2024-01-25T14:30:00Z"
  }
}
```

**Response Properties:**
- `id` (int) - Created submission identifier
- `submissionLink` (string) - Path to download the submitted file
- `fileName` (string) - Original filename of the submission
- `fileType` (string) - Detected file format (PDF, Excel, Word, etc.)
- `documentName` (string) - Type of document submitted
- `submissionDate` (DateTime) - When the document was submitted (auto-set)

**Validation Rules:**
1. **Request ID must be valid** - Document request must exist
2. **Document ID must match request** - The document type must match what was requested
3. **File is required** - Cannot submit without a file
4. **Deadline check** - Submission is rejected if past the due date
5. **File type detection** - Automatically detects file type from extension

**Error Responses:**

**File Required:**
```json
{
  "success": false,
  "message": "File is required",
  "statusCode": 400,
  "data": null
}
```

**Document Request Not Found:**
```json
{
  "success": false,
  "message": "Document Request with id (999) was not found.",
  "statusCode": 404,
  "data": null
}
```

**Document ID Mismatch:**
```json
{
  "success": false,
  "message": "Document ID does not match the document request",
  "statusCode": 400,
  "data": null
}
```

**Past Deadline:**
```json
{
  "success": false,
  "message": "Submission deadline has passed. Due date was 2024-01-31",
  "statusCode": 400,
  "data": null
}
```

**File Upload Failed:**
```json
{
  "success": false,
  "message": "File upload failed: [error details]",
  "statusCode": 500,
  "data": null
}
```

**How It Works:**
1. Validates file is provided
2. Saves file to server storage (`uploads/submissions/`)
3. Generates unique filename with GUID prefix
4. Detects file type from extension
5. Validates document request exists
6. Validates document type matches request
7. Checks if deadline has passed
8. Creates DocumentSubmission record
9. Returns submission details with download link
10. If any validation fails after file upload, the uploaded file is deleted

**File Type Detection:**
The system automatically detects file type based on extension:
- `.pdf` ? "PDF"
- `.doc`, `.docx` ? "Word"
- `.xls`, `.xlsx` ? "Excel"
- `.ppt`, `.pptx` ? "PowerPoint"
- `.txt` ? "Text"
- `.jpg`, `.jpeg`, `.png`, `.gif` ? "Image"
- `.zip`, `.rar` ? "Archive"

### 2. Get Document Submission
**Endpoint:** `GET /api/documentsubmissions/{id}`

**Description:** Retrieves document submission details including download link, filename, file type, document name, and upload date.

**Parameters:**
- `id` (int, path) - The document submission identifier

**Success Response:**
```json
{
  "success": true,
  "message": "Document submission retrieved successfully",
  "statusCode": 200,
  "data": {
    "id": 123,
    "submissionLink": "/uploads/submissions/abc123_project_brd.pdf",
    "fileName": "project1_brd_final.pdf",
    "fileType": "PDF",
    "documentName": "BRD",
    "submissionDate": "2024-01-25T14:30:00Z"
  }
}
```

**Response Properties:**
- `id` (int) - Submission identifier
- `submissionLink` (string, nullable) - Path to download the submitted file
- `fileName` (string, nullable) - Original filename of the submission
- `fileType` (string, nullable) - File format (PDF, Excel, Word, etc.)
- `documentName` (string) - Type of document submitted (BRD, Sprint Tracker, etc.)
- `submissionDate` (DateTime) - When the document was submitted

**Error Response (Not Found):**
```json
{
  "success": false,
  "message": "Document Submission with id (999) was not found.",
  "statusCode": 404,
  "data": null
}
```

**Use Cases:**
- Display submission details to trainee/admin
- Provide download link for submitted document
- Show submission history
- Verify submission information

### 3. Delete Document Submission
**Endpoint:** `DELETE /api/documentsubmissions/{id}`

**Description:** Deletes a document submission record and its associated file from the server.

**Parameters:**
- `id` (int, path) - The document submission identifier

**Success Response:**
```json
{
  "success": true,
  "message": "Document submission 'project1_brd_final.pdf' deleted successfully",
  "statusCode": 200,
  "data": true
}
```

**Error Response (Not Found):**
```json
{
  "success": false,
  "message": "Document Submission with id (999) was not found.",
  "statusCode": 404,
  "data": false
}
```

**Behavior:**
1. Verifies submission exists in database
2. Deletes database record
3. Attempts to delete physical file from server
4. Returns success even if file deletion fails (database record is removed)

**Important Notes:**
- This action is **permanent** and cannot be undone
- Both the database record and physical file are deleted
- If the file has already been deleted from disk, the operation still succeeds
- Trainee may need to resubmit if document is required

## Usage Examples

### Upload Document Submission

#### Using cURL
```bash
curl -X POST "https://localhost:5001/api/documentsubmissions" \
  -H "accept: application/json" \
  -F "RequestId=5" \
  -F "DocumentId=1" \
  -F "File=@/path/to/project_brd.pdf"
```

#### Using C# HttpClient
```csharp
var client = new HttpClient();
var content = new MultipartFormDataContent();

content.Add(new StringContent("5"), "RequestId");
content.Add(new StringContent("1"), "DocumentId");

// Add file
using var fileStream = File.OpenRead("project_brd.pdf");
var fileContent = new StreamContent(fileStream);
fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
content.Add(fileContent, "File", "project_brd.pdf");

var response = await client.PostAsync("https://localhost:5001/api/documentsubmissions", content);
var result = await response.Content.ReadAsStringAsync();
```

#### Using JavaScript/Fetch
```javascript
// Get file from input
const fileInput = document.getElementById('fileInput');
const file = fileInput.files[0];

if (!file) {
  alert('Please select a file');
  return;
}

// Create form data
const formData = new FormData();
formData.append('RequestId', '5');
formData.append('DocumentId', '1');
formData.append('File', file);

// Upload
fetch('https://localhost:5001/api/documentsubmissions', {
  method: 'POST',
  body: formData
})
.then(response => response.json())
.then(data => {
  if (data.success) {
    console.log('Uploaded successfully!');
    console.log('Submission ID:', data.data.id);
    console.log('Download link:', data.data.submissionLink);
  } else {
    alert(`Upload failed: ${data.message}`);
  }
})
.catch(error => {
  console.error('Error:', error);
  alert('An error occurred during upload');
});
```

#### Using React with Progress
```javascript
import { useState } from 'react';

function DocumentUploadForm({ requestId, documentId, documentName }) {
  const [file, setFile] = useState(null);
  const [uploading, setUploading] = useState(false);
  const [uploadResult, setUploadResult] = useState(null);

  const handleFileChange = (e) => {
    setFile(e.target.files[0]);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!file) {
      alert('Please select a file');
      return;
    }

    setUploading(true);
    setUploadResult(null);

    const formData = new FormData();
    formData.append('RequestId', requestId);
    formData.append('DocumentId', documentId);
    formData.append('File', file);

    try {
      const response = await fetch('https://localhost:5001/api/documentsubmissions', {
        method: 'POST',
        body: formData
      });

      const data = await response.json();

      if (data.success) {
        setUploadResult({
          success: true,
          message: 'Document uploaded successfully!',
          submission: data.data
        });
        setFile(null);
      } else {
        setUploadResult({
          success: false,
          message: data.message
        });
      }
    } catch (error) {
      setUploadResult({
        success: false,
        message: 'Network error occurred'
      });
    } finally {
      setUploading(false);
    }
  };

  return (
    <div className="upload-form">
      <h3>Submit {documentName}</h3>
      
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label>Select File:</label>
          <input 
            type="file" 
            onChange={handleFileChange}
            accept=".pdf,.doc,.docx,.xls,.xlsx"
            disabled={uploading}
          />
          {file && <p>Selected: {file.name}</p>}
        </div>

        <button 
          type="submit" 
          disabled={!file || uploading}
          className="btn btn-primary"
        >
          {uploading ? 'Uploading...' : 'Submit Document'}
        </button>
      </form>

      {uploadResult && (
        <div className={`alert ${uploadResult.success ? 'alert-success' : 'alert-danger'}`}>
          <p>{uploadResult.message}</p>
          {uploadResult.success && uploadResult.submission && (
            <div>
              <p>Submission ID: {uploadResult.submission.id}</p>
              <p>Submitted at: {new Date(uploadResult.submission.submissionDate).toLocaleString()}</p>
              <a 
                href={uploadResult.submission.submissionLink}
                download={uploadResult.submission.fileName}
              >
                Download your submission
              </a>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
```

#### Using Postman
1. Set method to POST
2. URL: `https://localhost:5001/api/documentsubmissions`
3. Go to Body tab
4. Select "form-data"
5. Add key-value pairs:
   - RequestId: 5
   - DocumentId: 1
   - File: (click dropdown, select "File", then choose file)
6. Click Send

### Complete Submission Workflow

```javascript
// Step 1: Get available document requests
async function getDocumentRequests(projectId) {
  const response = await fetch(`/api/documentrequests/project/${projectId}`);
  return await response.json();
}

// Step 2: Show upload form for selected request
function DocumentSubmissionWorkflow({ projectId }) {
  const [requests, setRequests] = useState([]);
  const [selectedRequest, setSelectedRequest] = useState(null);

  useEffect(() => {
    getDocumentRequests(projectId).then(data => {
      if (data.success) {
        setRequests(data.data);
      }
    });
  }, [projectId]);

  return (
    <div>
      <h2>Document Submissions</h2>
      
      <div className="requests-list">
        {requests.map(req => (
          <div key={req.id} className="request-item">
            <h4>{req.documentName}</h4>
            <p>Due: {new Date(req.dueDate).toLocaleDateString()}</p>
            <button onClick={() => setSelectedRequest(req)}>
              Submit Document
            </button>
          </div>
        ))}
      </div>

      {selectedRequest && (
        <DocumentUploadForm
          requestId={selectedRequest.id}
          documentId={selectedRequest.documentId}
          documentName={selectedRequest.documentName}
        />
      )}
    </div>
  );
}
```

## Response Status Codes

- **200 OK** - Upload successful
- **400 Bad Request** - Invalid request data, validation errors, or past deadline
- **404 Not Found** - Document request or document type not found
- **500 Internal Server Error** - File upload failed or other server error

## File Storage

### Storage Location
```
wwwroot/uploads/submissions/
```

### File Naming Pattern
```
{GUID}_{original-filename}
```

### Example
```
abc123-def456-ghi789_project_brd_final.pdf
```

### Access URL
```
/uploads/submissions/abc123-def456-ghi789_project_brd_final.pdf
```

## Testing with Swagger

1. Run the application
2. Navigate to `https://localhost:{port}/swagger`
3. Locate `POST /api/documentsubmissions`
4. Click "Try it out"
5. Fill in RequestId and DocumentId
6. Click "Choose File" to select a file
7. Click "Execute"

## Notes

- Files are stored with GUID-prefixed names to prevent conflicts
- File type is automatically detected from extension
- Submissions past the deadline are rejected
- If submission fails after file upload, the file is automatically deleted
- Maximum file size is controlled by server settings (default 30MB in ASP.NET Core)
- The system validates that the document type matches the request
- Submission date is automatically set to current UTC time
- Files are stored in `uploads/submissions/` directory
- Static file serving must be enabled in Program.cs

## Security Considerations

1. **File Size Limits** - Consider adding explicit file size validation
2. **File Type Validation** - Validate file extensions and MIME types
3. **Virus Scanning** - Consider integrating antivirus scanning for production
4. **Authorization** - Add authentication/authorization to restrict who can submit
5. **Rate Limiting** - Implement rate limiting to prevent abuse
6. **File Name Sanitization** - Original filenames are preserved but storage uses GUIDs
