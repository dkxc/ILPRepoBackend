# Document Management Entity Structure

## Overview

The document management system consists of three main entities that work together to manage document types, requests, and submissions. This document clarifies the purpose and relationships between these entities.

## Entity Descriptions

### 1. Documents (Master Catalog)
**Purpose:** Defines what document types exist in the system (master list/catalog)

**Database Table:** `documents`

**Fields:**
- `id` - Unique identifier
- `name` - Document type name (e.g., "BRD", "Sprint Tracker", "Test Cases")
- `type` - Expected file format (e.g., "PDF", "Excel", "Word", "PowerPoint")  
- `template_link` - URL to downloadable template for this document type
- `upload_date` - When the document type was added to the system
- `created_at` - Record creation timestamp
- `updated_at` - Record update timestamp

**Example Records:**
```
id | name            | type       | template_link
---+----------------+------------+--------------------------------------------
1  | BRD            | Word       | https://templates.com/brd-template.docx
2  | Sprint Tracker | Excel      | https://templates.com/sprint-tracker.xlsx
3  | Test Cases     | Excel      | https://templates.com/test-cases.xlsx
4  | Final Report   | PDF        | https://templates.com/final-report.pdf
5  | Presentation   | PowerPoint | https://templates.com/presentation.pptx
```

**API Endpoints:**
- `GET /api/documents/types` - Get all document types (for dropdown)
- `GET /api/documents/{id}/link` - Get template download link

---

### 2. DocumentRequest (Assignment/Deadline)
**Purpose:** Assigns a document type to a project/batch with a deadline

**Database Table:** `document_requests`

**Fields:**
- `id` - Unique identifier
- `project_id` - Which project this request is for (FK to projects)
- `document_id` - Which document type is requested (FK to documents)
- `request_date` - When the request was created (auto-set)
- `due_date` - Submission deadline
- `file_url` - Optional reference/example file uploaded by admin
- `created_at` - Record creation timestamp
- `updated_at` - Record update timestamp

**Example Records:**
```
id | project_id | document_id | request_date | due_date   | file_url
---+------------+-------------+--------------+------------+---------------------------
1  | 10         | 1           | 2024-01-15   | 2024-01-31 | /uploads/brd-example.pdf
2  | 10         | 2           | 2024-02-01   | 2024-02-15 | NULL
3  | 11         | 1           | 2024-01-15   | 2024-01-31 | /uploads/brd-example.pdf
```

**Meaning:**
- Row 1: "Project 10 needs to submit a BRD (document_id=1) by Jan 31, 2024. Admin provided example file."
- Row 2: "Project 10 needs to submit Sprint Tracker (document_id=2) by Feb 15, 2024."
- Row 3: "Project 11 needs to submit a BRD (document_id=1) by Jan 31, 2024."

**API Endpoints:**
- `POST /api/documentrequests` - Create new document request
- `PUT /api/documentrequests/{id}` - Update document request

---

### 3. DocumentSubmission (Actual Submission)
**Purpose:** Records trainee's actual submission of a document

**Database Table:** `document_submissions`

**Fields:**
- `id` - Unique identifier
- `request_id` - Which request this submission fulfills (FK to document_requests)
- `document_id` - Which document type was submitted (FK to documents)
- `submission_link` - Path to the uploaded submission file
- `file_name` - Original filename (e.g., "project1_brd_final.pdf")
- `file_type` - Actual file format submitted (e.g., "PDF", "Excel")
- `submission_date` - When the document was submitted (auto-set)
- `created_at` - Record creation timestamp
- `updated_at` - Record update timestamp

**Example Records:**
```
id | request_id | document_id | submission_link           | file_name             | file_type | submission_date
---+------------+-------------+---------------------------+----------------------+-----------+------------------
1  | 1          | 1           | /uploads/sub/brd_001.pdf  | project1_brd_v3.pdf  | PDF       | 2024-01-25
2  | 2          | 2           | /uploads/sub/sprint.xlsx  | sprint_tracker.xlsx  | Excel     | 2024-02-10
3  | 1          | 1           | /uploads/sub/brd_002.pdf  | brd_revised.pdf      | PDF       | 2024-01-28
```

**Meaning:**
- Row 1: "Trainee submitted BRD for request #1 on Jan 25 as 'project1_brd_v3.pdf'"
- Row 2: "Trainee submitted Sprint Tracker for request #2 on Feb 10 as 'sprint_tracker.xlsx'"
- Row 3: "Trainee submitted revised BRD for request #1 on Jan 28 as 'brd_revised.pdf'"

**API Endpoints:**
- `POST /api/documentsubmissions` - Submit a document (to be implemented)
- `GET /api/documentsubmissions/{requestId}` - Get submissions for a request (to be implemented)

---

## Link/URL Field Clarification

### Why Three Different "Link" Fields?

#### 1. `Documents.template_link`
- **Purpose:** Link to downloadable TEMPLATE for this document type
- **Set by:** Admin/System
- **Used for:** Providing trainees with a standard template to fill out
- **Example:** "https://templates.com/brd-template.docx"
- **Usage:** Trainee downloads this before starting work

#### 2. `DocumentRequest.file_url`
- **Purpose:** Optional REFERENCE/EXAMPLE file provided by admin for this specific request
- **Set by:** Admin (optional)
- **Used for:** Giving trainees an example or additional reference
- **Example:** "/uploads/examples/sample-brd.pdf"
- **Usage:** "Here's an example of how a good BRD looks for this project"

#### 3. `DocumentSubmission.submission_link`
- **Purpose:** Path to the ACTUAL SUBMISSION file uploaded by trainee
- **Set by:** System (during upload)
- **Used for:** Storing and retrieving trainee submissions
- **Example:** "/uploads/submissions/project1_brd_20240125_001.pdf"
- **Usage:** Admin/reviewers download this to review trainee work

### Visual Flow

```
1. Documents.template_link
   ? (Trainee downloads template)
   
2. Trainee fills out template
   ?
   
3. DocumentRequest.file_url (optional reference)
   ? (Trainee references example if needed)
   
4. Trainee submits completed document
   ?
   
5. DocumentSubmission.submission_link (stored submission)
```

---

## Relationships

### Documents ? DocumentRequest (One-to-Many)
- One document type (e.g., "BRD") can have many requests across different projects
- Example: Multiple projects all need to submit BRDs

### Documents ? DocumentSubmission (One-to-Many)
- One document type can have many submissions
- Example: Many trainees submitting BRDs for their projects

### DocumentRequest ? DocumentSubmission (One-to-Many)
- One request can have multiple submissions
- Example: Trainee submits, then resubmits revised version

### Project ? DocumentRequest (One-to-Many)
- One project can have many document requests
- Example: Project 10 needs to submit BRD, Sprint Tracker, Test Cases, etc.

---

## Workflow Example

### Complete Document Lifecycle

**Step 1: Admin Setup (Documents Table)**
```sql
INSERT INTO documents (name, type, template_link)
VALUES ('BRD', 'Word', 'https://templates.com/brd-template.docx');
```

**Step 2: Create Request (DocumentRequest Table)**
```sql
INSERT INTO document_requests (project_id, document_id, due_date, file_url)
VALUES (10, 1, '2024-01-31', '/uploads/examples/good-brd.pdf');
```
*Meaning: "Project 10, submit a BRD by Jan 31. Here's a good example."*

**Step 3: Trainee Downloads Template**
- Trainee calls: `GET /api/documents/1/link`
- Gets: `template_link = "https://templates.com/brd-template.docx"`
- Downloads and fills out template

**Step 4: Trainee Reviews Example (Optional)**
- Trainee downloads `file_url` from DocumentRequest
- Reviews example for guidance

**Step 5: Trainee Submits (DocumentSubmission Table)**
```sql
INSERT INTO document_submissions (request_id, document_id, submission_link, file_name, file_type)
VALUES (1, 1, '/uploads/submissions/brd_001.pdf', 'project1_brd_final.pdf', 'PDF');
```

**Step 6: Review**
- Admin/Reviewer retrieves submission via `submission_link`
- Reviews and provides feedback

**Step 7: Resubmission (If Needed)**
```sql
INSERT INTO document_submissions (request_id, document_id, submission_link, file_name, file_type)
VALUES (1, 1, '/uploads/submissions/brd_002.pdf', 'project1_brd_revised.pdf', 'PDF');
```

---

## File Type Tracking

### Documents.type (Expected)
- What file type SHOULD be submitted
- Example: "PDF", "Excel", "Word"
- Used for validation and user guidance

### DocumentSubmission.file_type (Actual)
- What file type WAS actually submitted
- Example: "PDF", "Excel", "Word"
- Can be compared with Documents.type for validation

**Validation Example:**
```csharp
if (submission.FileType != document.Type) {
    // Warning: Expected PDF but got Word document
}
```

---

## Summary

| Entity            | Purpose                           | Link Field       | Link Purpose                    |
|-------------------|-----------------------------------|------------------|---------------------------------|
| Documents         | Catalog of document types         | template_link    | Downloadable template           |
| DocumentRequest   | Assignment with deadline          | file_url         | Optional reference/example      |
| DocumentSubmission| Trainee's actual submission       | submission_link  | Path to submitted file          |

**Key Point:** Having links in different tables is correct and necessary because each serves a different purpose in the document lifecycle.
