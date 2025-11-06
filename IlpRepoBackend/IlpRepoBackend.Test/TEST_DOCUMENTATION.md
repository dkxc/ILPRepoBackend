# IlpRepoBackend Test Suite Documentation

## Overview
This document provides comprehensive documentation for all test cases in the IlpRepoBackend test suite. Each handler is tested for both success and failure scenarios.

## Test Framework & Tools
- **xUnit**: Test framework
- **Moq**: Mocking framework for dependencies
- **Shouldly**: Fluent assertion library
- **AutoMapper**: For DTO mapping tests

## Test Coverage by Handler

### 1. Authentication Handlers

#### LoginUserQueryHandlerTests
**Location**: `Handlers/Auth/LoginUserQueryHandlerTests.cs`

**Success Scenarios:**
- ? `Handle_ValidCredentials_ReturnsSuccess`: Valid email and password return JWT token
- ? `Handle_DifferentRoles_ReturnsSuccessWithCorrectRole`: Tests Admin, Trainer, and Trainee roles

**Failure Scenarios:**
- ? `Handle_UserNotFound_ReturnsFailure`: Non-existent email returns error
- ? `Handle_InvalidPassword_ReturnsFailure`: Wrong password returns error
- ? `Handle_InactiveUser_ReturnsFailure`: Inactive account cannot login

**Business Rules Tested:**
- Password verification using BCrypt
- JWT token generation
- Role-based authentication
- Active account validation

---

#### ValidateTokenQueryHandlerTests
**Location**: `Handlers/Auth/ValidateTokenQueryHandlerTests.cs`

**Success Scenarios:**
- ? `Handle_ValidToken_ReturnsSuccess`: Valid JWT token passes validation

**Failure Scenarios:**
- ? `Handle_InvalidToken_ReturnsFailure`: Invalid token fails validation
- ? `Handle_EmptyToken_ReturnsFailure`: Empty token fails validation
- ? `Handle_NullToken_ReturnsFailure`: Null token fails validation

**Business Rules Tested:**
- JWT token validation
- Token expiration checking
- Token format validation

---

### 2. User Management Handlers

#### CreateUserCommandHandlerTests
**Location**: `Handlers/Users/CreateUserCommandHandlerTests.cs`

**Success Scenarios:**
- ? `Handle_ValidCommand_CreatesUser`: Valid user data creates user successfully
- ? `Handle_DifferentRoles_CreatesUserWithCorrectRole`: Tests all user roles (Admin, Trainer, Trainee)
- ? `Handle_PasswordIsHashed_BeforeStoringUser`: Password is hashed before storage

**Failure Scenarios:**
- ? `Handle_EmailExists_ThrowsInvalidOperationException`: Duplicate email throws error
- ? `Handle_UsernameExists_ThrowsInvalidOperationException`: Duplicate username throws error

**Business Rules Tested:**
- Email uniqueness validation
- Username uniqueness validation
- Password hashing with BCrypt
- Role assignment
- Active status setting
- Timestamp management (CreatedAt, UpdatedAt)

---

#### DeleteUserCommandHandlerTests
**Location**: `Handlers/Users/DeleteUserCommandHandlerTests.cs`

**Success Scenarios:**
- ? `Handle_ValidUserId_DeletesUser`: Valid user ID deletes successfully
- ? `Handle_VariousUserIds_CallsRepositoryWithCorrectId`: Tests multiple user IDs

**Failure Scenarios:**
- ? `Handle_InvalidUserId_ReturnsFalse`: Non-existent user ID returns false

**Business Rules Tested:**
- User existence validation
- Soft/hard delete handling
- Repository interaction

---

### 3. Batch Management Handlers

#### GetBatchQueryHandlerTests
**Location**: `Handlers/Batches/GetBatchQueryHandlerTests.cs`

**Success Scenarios:**
- ? `Handle_BatchesExist_ReturnsAllBatches`: Returns all batches with proper mapping
- ? `Handle_NoBatches_ReturnsEmptyList`: Empty database returns empty list
- ? `Handle_BatchesWithDifferentStatuses_ReturnsAllBatches`: Returns batches with NotStarted, Ongoing, Completed statuses

**Business Rules Tested:**
- Batch retrieval
- DTO mapping
- Multiple batch status handling
- Empty result handling

---

#### CreateBatchHandlerTests
**Location**: `Handlers/Batches/CreateBatchHandlerTests.cs`

**Success Scenarios:**
- ? `Handle_ValidCommand_CreatesBatch`: Creates batch with valid data
- ? `Handle_WithPhases_CreatesPhases`: Creates batch with multiple phases
- ? `Handle_WithValidDates_GeneratesTrainingSchedules`: Auto-generates training schedules for date range

**Status Calculation Tests:**
- ? `Handle_BatchNotStarted_StatusIsNotStarted`: Future start date = NotStarted
- ? `Handle_BatchOngoing_StatusIsOngoing`: Current date between start and end = Ongoing
- ? `Handle_BatchCompleted_StatusIsCompleted`: End date in past = Completed

**Edge Cases:**
- ? `Handle_WithoutDates_DoesNotGenerateTrainingSchedules`: Null dates skip schedule generation

**Business Rules Tested:**
- Automatic status calculation based on dates
- Batch-Phase relationship creation
- Training schedule auto-generation (8 hours per day)
- Batch type association
- Phase management

---

#### DeleteBatchHandlerTests
**Location**: `Handlers/Batches/DeleteBatchHandlerTests.cs`

**Success Scenarios:**
- ? `Handle_ValidBatchWithNoRelations_DeletesSuccessfully`: Batch with no dependencies deletes

**Failure Scenarios:**
- ? `Handle_BatchNotFound_ThrowsInvalidOperationException`: Non-existent batch throws error
- ? `Handle_BatchWithTrainees_ThrowsInvalidOperationException`: Cannot delete batch with trainees
- ? `Handle_BatchWithPhases_ThrowsInvalidOperationException`: Cannot delete batch with phases
- ? `Handle_BatchWithTrainingSchedules_ThrowsInvalidOperationException`: Cannot delete batch with schedules
- ? `Handle_BatchWithMultipleRelations_ThrowsExceptionForFirstViolation`: Multiple violations throw first error

**Business Rules Tested:**
- Referential integrity checking
- Cascade delete prevention
- Trainee association validation
- Phase association validation
- Training schedule association validation

---

### 4. Project Management Handlers

#### GetProjectByIdHandlerTests
**Location**: `Handlers/Projects/GetProjectByIdHandlerTests.cs`

**Success Scenarios:**
- ? `Handle_ValidProjectId_ReturnsProjectWithBatchInfo`: Returns project with batch details
- ? `Handle_ProjectWithNoTeams_ReturnsProjectWithoutBatchInfo`: Project without teams handled gracefully

**Failure Scenarios:**
- ? `Handle_ProjectNotFound_ReturnsFailure`: Non-existent project returns error
- ? `Handle_RepositoryThrowsException_ReturnsFailure`: Database errors handled gracefully

**Edge Cases:**
- ? `Handle_ProjectWithNullTeams_ReturnsProjectWithoutBatchInfo`: Null teams handled
- ? `Handle_BatchNotFound_ReturnsProjectWithoutBatchName`: Missing batch handled
- ? `Handle_MapperReturnsNull_HandlesGracefully`: Null mapping handled

**Business Rules Tested:**
- Project-Batch relationship
- Project-Team relationship
- Trainee-Batch association
- Error handling and recovery

---

### 5. Trainee Management Handlers

#### CreateTraineeHandlerTests
**Location**: `Handlers/Trainees/CreateTraineeHandlerTests.cs`

**Success Scenarios:**
- ? `Handle_ValidCommand_CreatesTrainee`: Valid trainee data creates trainee
- ? `Handle_MultipleTrainees_CreatesEachIndependently`: Multiple trainees created independently
- ? `Handle_ValidatesRequiredFields`: Required fields validated

**Business Rules Tested:**
- Trainee-Batch association
- Required field validation
- Email format (if applicable)
- Batch assignment

---

### 6. Document Management Handlers

#### CreateDocumentTypeHandlerTests
**Location**: `Handlers/Documents/CreateDocumentTypeHandlerTests.cs`

**Success Scenarios:**
- ? `Handle_ValidCommand_CreatesDocumentType`: Valid document type created
- ? `Handle_DifferentDocumentTypes_CreatesEachSuccessfully`: Multiple document types (Resume, Cover Letter, Portfolio)
- ? `Handle_EmptyDescription_CreatesDocumentType`: Empty description allowed

**Business Rules Tested:**
- Document type uniqueness
- Description field optional
- Timestamp management

---

## Test Patterns Used

### 1. Arrange-Act-Assert (AAA)
All tests follow the AAA pattern:
```csharp
// Arrange - Set up test data and mocks
var command = new CreateUserCommand { ... };
_repositoryMock.Setup(x => x.Method()).Returns(...);

// Act - Execute the handler
var result = await _handler.Handle(command, CancellationToken.None);

// Assert - Verify results
result.ShouldNotBeNull();
result.Succeeded.ShouldBeTrue();
```

### 2. Mocking with Moq
```csharp
_repositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
    .ReturnsAsync(entity);
```

### 3. Fluent Assertions with Shouldly
```csharp
result.ShouldNotBeNull();
result.Data.Count.ShouldBe(2);
result.Message.ShouldContain("success");
```

### 4. Theory Tests for Multiple Scenarios
```csharp
[Theory]
[InlineData(UserRole.Admin)]
[InlineData(UserRole.Trainer)]
[InlineData(UserRole.Trainee)]
public async Task Handle_DifferentRoles_Test(UserRole role)
```

---

## Running Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~LoginUserQueryHandlerTests"
```

### Run Tests with Coverage
```bash
dotnet test /p:CollectCoverage=true
```

---

## Test Coverage Summary

| Module | Test Classes | Success Tests | Failure Tests | Total Tests |
|--------|-------------|---------------|---------------|-------------|
| Auth | 2 | 3 | 6 | 9 |
| Users | 2 | 6 | 2 | 8 |
| Batches | 3 | 10 | 6 | 16 |
| Projects | 1 | 4 | 4 | 8 |
| Trainees | 1 | 3 | 0 | 3 |
| Documents | 1 | 3 | 0 | 3 |
| **TOTAL** | **10** | **29** | **18** | **47** |

---

## Additional Handlers to Test

The following handlers still need comprehensive test coverage:

### Phase Management
- `CreatePhaseTypeHandler`
- `UpdatePhaseTypeHandler`
- `DeletePhaseTypeHandler`
- `GetPhaseTypeByIdHandler`

### Batch Type Management
- `CreateBatchTypeHandler`
- `UpdateBatchTypeHandler`
- `DeleteBatchTypeHandler`

### Document Submissions
- `SubmitDocumentHandler`
- `GetDocumentSubmissionsByProjectIdHandler`
- `DeleteDocumentSubmissionHandler`

### Document Requirements
- `SetDocumentRequirementHandler`
- `UpdateDocumentRequirementHandler`
- `DeleteDocumentRequirementHandler`

### Curriculum Management
- `CreateCurriculumQueryHandler`
- `UpdateCurriculumCommandHandler`
- `DeleteCurriculumCommandHandler`
- `GetCurriculumByBatchIdQueryHandler`

### Attendance
- `UploadBatchAttendanceCommandHandler`
- `UpdateBatchAttendanceCommandHandler`
- `GetAttendanceByBatchQueryHandler`

### Dashboard
- `GetTraineeDashboardQueryHandler`
- `GetAdminDashboardSummaryQueryHandler`
- `GetTrainingHoursReportQueryHandler`

### Links
- `CreateLinkTypeHandler`
- `AssignLinkTypeToBatchHandler`
- `GetLinkTypesByBatchIdHandler`

---

## Best Practices

1. **Mock All Dependencies**: Never use real database or external services
2. **Test One Thing**: Each test should verify one specific behavior
3. **Descriptive Names**: Test names should clearly describe what they test
4. **Arrange Clearly**: Set up test data in a readable way
5. **Assert Completely**: Verify all important aspects of the result
6. **Cover Edge Cases**: Test null values, empty collections, etc.
7. **Test Failures**: Always test failure scenarios, not just happy paths

---

## Continuous Integration

These tests are designed to run in CI/CD pipelines:
- Fast execution (no database dependencies)
- Deterministic results
- Clear failure messages
- No external dependencies

---

## Contributing

When adding new handlers:
1. Create a corresponding test class
2. Test all success scenarios
3. Test all failure scenarios
4. Test edge cases
5. Update this documentation
6. Ensure all tests pass before committing

---

*Last Updated: January 2025*
*Test Framework: xUnit 2.5.3*
*Mocking: Moq 4.20.72*
*Assertions: Shouldly 4.3.0*
