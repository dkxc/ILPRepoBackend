# Document Request API

This API allows you to manage document types with optional template file uploads to Supabase storage.

## Endpoints

### 1. Get All Document Types
**GET** `/api/documents`

Returns all document types in the system for editing purposes.

**Response:**
```json
{
  "succeeded": true,
  "message": "Document types retrieved successfully",
  "data": [
    {
      "id": 1,
      "name": "Business Requirements Document",
      "link": "https://jbbaufdzfgglkjqiwveb.supabase.co/storage/v1/object/public/docs/uuid_template.pdf",
      "uploadDate": "2024-01-01T10:00:00Z",
      "createdAt": "2024-01-01T10:00:00Z",
      "updatedAt": "2024-01-01T10:00:00Z"
    }
  ]
}
```

### 2. Create Document Type
**POST** `/api/documents`
**Content-Type:** `multipart/form-data`

Creates a new document type with optional template file upload.

**Form Data:**
- `name` (required, string): Document type name (max 100 characters)
- `templateFile` (optional, file): Template file to upload

**Example using curl:**
```bash
curl -X POST "https://localhost:7001/api/documents" \
  -H "Content-Type: multipart/form-data" \
  -F "name=Business Requirements Document" \
  -F "templateFile=@/path/to/template.pdf"
```

**Response:**
```json
{
  "succeeded": true,
  "message": "Document type created successfully",
  "data": {
    "id": 1,
    "name": "Business Requirements Document",
    "link": "https://jbbaufdzfgglkjqiwveb.supabase.co/storage/v1/object/public/docs/uuid_template.pdf",
    "createdAt": "2024-01-01T10:00:00Z"
  }
}
```

### 3. Update Document Type
**PUT** `/api/documents/{id}`
**Content-Type:** `multipart/form-data`

Updates an existing document type. Can update name and/or replace the template file.

**Form Data:**
- `name` (required, string): Document type name
- `templateFile` (optional, file): New template file to replace existing one

**Example using curl:**
```bash
curl -X PUT "https://localhost:7001/api/documents/1" \
  -H "Content-Type: multipart/form-data" \
  -F "name=Updated Business Requirements Document" \
  -F "templateFile=@/path/to/new_template.pdf"
```

**Response:**
```json
{
  "succeeded": true,
  "message": "Document type updated successfully",
  "data": {
    "id": 1,
    "name": "Updated Business Requirements Document",
    "link": "https://jbbaufdzfgglkjqiwveb.supabase.co/storage/v1/object/public/docs/new_uuid_template.pdf",
    "uploadDate": "2024-01-01T11:00:00Z",
    "createdAt": "2024-01-01T10:00:00Z",
    "updatedAt": "2024-01-01T11:00:00Z"
  }
}
```

## File Storage

- Files are uploaded to Supabase storage in the "docs" bucket
- Each file gets a unique name with UUID prefix to avoid conflicts
- The public URL is stored in the documents table link column
- When updating a document with a new file, the old file is replaced

## Authentication

Currently, the API endpoints do not require authentication for testing purposes.

## Error Handling

All endpoints return a consistent response format:
- `succeeded`: boolean indicating success/failure
- `message`: descriptive message
- `data`: response data (null on failure)

**Error Response Examples:**
```json
{
  "succeeded": false,
  "message": "Document type with name 'Business Requirements Document' already exists",
  "data": null
}
```

```json
{
  "succeeded": false,
  "message": "Document type with ID 999 not found",
  "data": null
}
```

## Frontend Integration

The frontend can use FormData to submit files:

### Creating a new document type:
```javascript
const formData = new FormData();
formData.append('name', 'Business Requirements Document');
formData.append('templateFile', fileInput.files[0]);

fetch('/api/documents', {
  method: 'POST',
  body: formData
})
.then(response => response.json())
.then(data => console.log(data));
```

### Updating an existing document type:
```javascript
const formData = new FormData();
formData.append('name', 'Updated Document Name');
formData.append('templateFile', fileInput.files[0]); // Optional

fetch(`/api/documents/${documentId}`, {
  method: 'PUT',
  body: formData
})
.then(response => response.json())
.then(data => console.log(data));