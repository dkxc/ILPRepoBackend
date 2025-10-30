# Database Migration Required

## Schema Changes Summary

### 1. Documents Table Changes
- **Renamed Column:** `link` ? `template_link` (clarification of purpose)
- **New Column:** `type` - File type/format (PDF, Excel, Word, etc.)

### 2. DocumentRequest Table
- **New Column:** `file_url` - Stores uploaded file path for document requests

### 3. DocumentSubmission Table  
- **New Column:** `file_name` - Original filename of submitted document
- **New Column:** `file_type` - File type/format of submitted document

## Table Purposes Clarified

### Documents Table (Master Catalog)
```
Purpose: Master list of document types that exist in the system
Examples: BRD, Sprint Tracker, Test Cases, etc.
Key Fields:
- id: Unique identifier
- name: Document type name (e.g., "BRD", "Sprint Tracker")
- type: Expected file format (e.g., "PDF", "Excel")
- template_link: Downloadable template URL for this document type
```

### DocumentRequest Table (Assignment/Deadline)
```
Purpose: Assignment of a document type to a project/batch with deadline
Examples: "Project 1 needs to submit BRD by Jan 31"
Key Fields:
- id: Unique identifier
- project_id: Which project this request is for
- document_id: Which document type is requested (references Documents)
- due_date: Submission deadline
- request_date: When the request was created
- file_url: Optional reference file/example uploaded by admin
```

### DocumentSubmission Table (Actual Submission)
```
Purpose: Trainee's actual submission of a document
Examples: "Trainee John submitted BRD on Jan 25"
Key Fields:
- id: Unique identifier
- request_id: Which request this submission fulfills
- document_id: Which document type was submitted
- submission_link: Path to the uploaded submission file
- file_name: Original filename (e.g., "project1_brd_final.pdf")
- file_type: File format (e.g., "PDF", "Excel")
- submission_date: When the document was submitted
```

## Migration Commands

Run the following commands to create and apply the migrations:

```bash
# Navigate to the Infrastructure project directory
cd IlpRepoBackend.Infrastructure

# Create comprehensive migration for all changes
dotnet ef migrations add UpdateDocumentEntitiesStructure --startup-project ../IlpRepoBackend.Api

# Apply migration
dotnet ef database update --startup-project ../IlpRepoBackend.Api
```

## Manual SQL Migration (PostgreSQL)

If you prefer to run SQL manually:

```sql
-- 1. Add Type column to Documents table
ALTER TABLE documents 
ADD COLUMN type VARCHAR(50) NULL;

-- 2. Rename Link to TemplateLink in Documents table
ALTER TABLE documents 
RENAME COLUMN link TO template_link;

-- 3. Increase template_link column size
ALTER TABLE documents 
ALTER COLUMN template_link TYPE VARCHAR(500);

-- 4. Add FileUrl to DocumentRequest table
ALTER TABLE document_requests 
ADD COLUMN file_url VARCHAR(500) NULL;

-- 5. Add FileName to DocumentSubmission table
ALTER TABLE document_submissions 
ADD COLUMN file_name VARCHAR(255) NULL;

-- 6. Add FileType to DocumentSubmission table
ALTER TABLE document_submissions 
ADD COLUMN file_type VARCHAR(50) NULL;

-- 7. Increase submission_link column size
ALTER TABLE document_submissions 
ALTER COLUMN submission_link TYPE VARCHAR(500);
```

## Rollback

If you need to rollback these migrations:

```bash
dotnet ef database update <PreviousMigrationName> --startup-project ../IlpRepoBackend.Api
dotnet ef migrations remove --startup-project ../IlpRepoBackend.Api
```

## Data Population Examples

### Populate Documents (Master Catalog)
```sql
INSERT INTO documents (name, type, template_link, upload_date, created_at, updated_at)
VALUES 
    ('BRD', 'Word', 'https://example.com/templates/brd-template.docx', NOW(), NOW(), NOW()),
    ('Sprint Tracker', 'Excel', 'https://example.com/templates/sprint-tracker.xlsx', NOW(), NOW(), NOW()),
    ('Test Cases', 'Excel', 'https://example.com/templates/test-cases.xlsx', NOW(), NOW(), NOW()),
    ('Final Presentation', 'PowerPoint', 'https://example.com/templates/presentation.pptx', NOW(), NOW(), NOW()),
    ('Project Report', 'PDF', NULL, NOW(), NOW(), NOW());
```

### Common File Types Reference
- **PDF** - Portable Document Format
- **Word** - Microsoft Word Document (.doc, .docx)
- **Excel** - Microsoft Excel Spreadsheet (.xls, .xlsx)
- **PowerPoint** - Microsoft PowerPoint Presentation (.ppt, .pptx)
- **Text** - Plain text file (.txt)
- **Image** - Image files (.jpg, .png, .gif)
- **Video** - Video files (.mp4, .avi)
- **Archive** - Compressed files (.zip, .rar)

## Important Notes

1. **Template Link Clarification**: 
   - `Documents.template_link` = Downloadable template for trainees
   - `DocumentRequest.file_url` = Optional reference/example file from admin
   - `DocumentSubmission.submission_link` = Actual trainee submission

2. **File Type Tracking**:
   - `Documents.type` = Expected file type for this document
   - `DocumentSubmission.file_type` = Actual file type submitted
   - These can be used for validation

3. **Backwards Compatibility**:
   - All new columns are nullable
   - Existing data won't be affected
   - Update existing records as needed

## Post-Migration Tasks

After migration, consider:

1. **Update existing Documents records** with type and template_link
2. **Populate Documents table** with your standard document types
3. **Test file upload/download** functionality
4. **Verify API responses** include new fields
5. **Update frontend** to use new fields
