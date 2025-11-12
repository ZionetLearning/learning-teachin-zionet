# Meetings Endpoints Documentation

## Overview
This document details all the meetings endpoints available in the Manager service under the `/meetings-manager` route.

**Base Path**: `/meetings-manager`

**Tag**: `Meetings`

---

### the way the meeting works:
- Meetings can be created by Admins and Teachers
- after creating a meeting the attendees can generate tokens to join the meeting
- with this token and the group call id the user can join the meeting using ACS SDK
- Front need to install Azure Communication Services SDK to join the meeting and handle the call

## Endpoints

### 1. Get Meeting
Retrieves details of a specific meeting.

**Endpoint**: `GET /meetings-manager/{meetingId}`

**Authorization**: Admin, Teacher, or Student

**Route Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `meetingId` | `Guid` | ✅ | The unique identifier of the meeting |

**Request Body**: None

**Response**:
- **200 OK**: Returns `MeetingDto`
- **400 Bad Request**: Invalid meeting ID
- **401 Unauthorized**: Invalid or missing caller ID
- **403 Forbidden**: User is not authorized to view the meeting (must be an attendee or admin)
- **404 Not Found**: Meeting not found
- **500 Internal Server Error**: An error occurred while retrieving the meeting

**Response Schema** (`MeetingDto`):
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "attendees": [
    {
      "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "role": "Teacher" | "Student"
    }
  ],
  "startTimeUtc": "2024-01-15T10:00:00Z",
  "durationMinutes": 60,
  "description": "Optional meeting description",
  "status": "Scheduled" | "Completed" | "Cancelled",
  "groupCallId": "unique-group-call-id",
  "createdOn": "2024-01-10T08:00:00Z",
  "createdByUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Authorization Rules**:
- Admin users can view any meeting
- Non-admin users can only view meetings where they are attendees

---

### 2. Get Meetings for User
Retrieves all meetings for a specific user.

**Endpoint**: `GET /meetings-manager/user/{userId}`

**Authorization**: Admin, Teacher, or Student

**Route Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `userId` | `Guid` | ✅ | The unique identifier of the user |

**Request Body**: None

**Response**:
- **200 OK**: Returns `List<MeetingDto>`
- **400 Bad Request**: Invalid user ID
- **401 Unauthorized**: Invalid or missing caller ID
- **403 Forbidden**: User is not authorized to view meetings for the specified user
- **500 Internal Server Error**: An error occurred while retrieving meetings

**Response Schema**: `List<MeetingDto>` (see MeetingDto schema above)

**Authorization Rules**:
- Admin users can view meetings for any user
- Non-admin users can only view their own meetings (callerId must match userId)

---

### 3. Create Meeting
Creates a new meeting with specified attendees and details.

**Endpoint**: `POST /meetings-manager`

**Authorization**: Admin or Teacher

**Route Parameters**: None

**Request Body**: `CreateMeetingRequest`
```json
{
  "attendees": [
    {
      "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "role": "Teacher" | "Student"
    }
  ],
  "startTimeUtc": "2024-01-15T10:00:00Z",
  "durationMinutes": 60,
  "description": "Optional meeting description (max 500 characters)"
}
```

**Request Schema** (`CreateMeetingRequest`):
| Field | Type | Required | Validation | Description |
|-------|------|----------|------------|-------------|
| `attendees` | `List<MeetingAttendee>` | ✅ | Must have at least one attendee | List of meeting attendees |
| `startTimeUtc` | `DateTimeOffset` | ✅ | Valid date/time | Meeting start time in UTC |
| `durationMinutes` | `int` | ✅ | Between 1 and 1440 | Meeting duration (1 minute to 24 hours) |
| `description` | `string` | ❌ | Max 500 characters | Optional meeting description |

**MeetingAttendee Schema**:
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `userId` | `Guid` | ✅ | The user's unique identifier |
| `role` | `AttendeeRole` | ✅ | Either "Teacher" or "Student" |

**Response**:
- **201 Created**: Returns `MeetingDto` with location header `/meetings-manager/{meetingId}`
- **400 Bad Request**: Invalid request data or validation errors
- **401 Unauthorized**: Invalid or missing caller ID
- **403 Forbidden**: User is not authorized to create meetings
- **500 Internal Server Error**: An error occurred while creating the meeting

**Response Schema**: `MeetingDto` (see schema above)

**Authorization Rules**:
- Admin users can create any meeting
- Teacher users can create meetings but must be included as an attendee with the "Teacher" role
- Creates or retrieves ACS (Azure Communication Services) identities for all attendees

**Additional Validations**:
- All attendees must pass validation
- Meeting configuration must comply with `MeetingOptions` settings
- At least one attendee is required

---

### 4. Update Meeting
Updates an existing meeting's details.

**Endpoint**: `PUT /meetings-manager/{meetingId}`

**Authorization**: Admin or Teacher

**Route Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `meetingId` | `Guid` | ✅ | The unique identifier of the meeting to update |

**Request Body**: `UpdateMeetingRequest`
```json
{
  "attendees": [
    {
      "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "role": "Teacher" | "Student"
    }
  ],
  "startTimeUtc": "2024-01-15T10:00:00Z",
  "durationMinutes": 60,
  "description": "Updated meeting description",
  "status": "Scheduled" | "Completed" | "Cancelled"
}
```

**Request Schema** (`UpdateMeetingRequest`):
| Field | Type | Required | Validation | Description |
|-------|------|----------|------------|-------------|
| `attendees` | `List<MeetingAttendee>` | ❌ | Each attendee must be valid | Updated list of meeting attendees |
| `startTimeUtc` | `DateTimeOffset` | ❌ | Valid date/time | Updated meeting start time in UTC |
| `durationMinutes` | `int` | ❌ | Between 1 and 1440 | Updated meeting duration |
| `description` | `string` | ❌ | Max 500 characters | Updated meeting description |
| `status` | `MeetingStatus` | ❌ | Valid enum value | Updated meeting status |

**Response**:
- **200 OK**: Returns message "Meeting updated successfully"
- **400 Bad Request**: Invalid meeting ID or request data
- **401 Unauthorized**: Invalid or missing caller ID
- **403 Forbidden**: User is not authorized to update the meeting
- **404 Not Found**: Meeting not found
- **500 Internal Server Error**: An error occurred while updating the meeting

**Authorization Rules**:
- Admin users can update any meeting
- Non-admin users can only update meetings they created (createdByUserId must match callerId)

**Additional Validations**:
- All provided attendees must pass validation
- Meeting configuration must comply with `MeetingOptions` settings
- All fields are optional; only provided fields will be updated

---

### 5. Delete Meeting
Deletes an existing meeting.

**Endpoint**: `DELETE /meetings-manager/{meetingId}`

**Authorization**: Admin or Teacher

**Route Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `meetingId` | `Guid` | ✅ | The unique identifier of the meeting to delete |

**Request Body**: None

**Response**:
- **200 OK**: Returns message "Meeting deleted successfully"
- **400 Bad Request**: Invalid meeting ID
- **401 Unauthorized**: Invalid or missing caller ID
- **403 Forbidden**: User is not authorized to delete the meeting
- **404 Not Found**: Meeting not found
- **500 Internal Server Error**: An error occurred while deleting the meeting

**Authorization Rules**:
- Admin users can delete any meeting
- Non-admin users can only delete meetings they created (createdByUserId must match callerId)

---

### 6. Generate Token for Meeting
Generates an Azure Communication Services (ACS) access token for a user to join a specific meeting.

**Endpoint**: `POST /meetings-manager/{meetingId}/token`

**Authorization**: Admin, Teacher, or Student

**Route Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `meetingId` | `Guid` | ✅ | The unique identifier of the meeting |

**Request Body**: None

**Response**:
- **200 OK**: Returns `AcsTokenResponse`
- **400 Bad Request**: Invalid meeting ID
- **401 Unauthorized**: Invalid or missing caller ID
- **403 Forbidden**: User is not an attendee of the meeting
- **404 Not Found**: Meeting not found or user identity not found
- **500 Internal Server Error**: An error occurred while generating the token

**Response Schema** (`AcsTokenResponse`):
```json
{
  "userId": "acs-user-id-string",
  "token": "access-token-string",
  "expiresOn": "2024-01-15T12:00:00Z",
  "groupId": "group-call-id-string"
}
```

| Field | Type | Description |
|-------|------|-------------|
| `userId` | `string` | The ACS user ID |
| `token` | `string` | The access token for joining the meeting |
| `expiresOn` | `DateTimeOffset` | Token expiration time |
| `groupId` | `string` | The group call ID for the meeting |

**Authorization Rules**:
- User must be an attendee of the meeting
- Token is generated for the authenticated user (callerId)

---

## Enums

### AttendeeRole
Defines the role of a meeting attendee.

**Values**:
- `Teacher`
- `Student`

### MeetingStatus
Defines the status of a meeting.

**Values**:
- `Scheduled` - Meeting is scheduled to occur
- `Completed` - Meeting has been completed
- `Cancelled` - Meeting has been cancelled

---

## Authentication
All endpoints require authentication. The following claims are used:
- **User ID Claim**: Identifies the authenticated user
- **Role Claim**: Identifies the user's role (Admin, Teacher, or Student)

## Authorization Policies
- **AdminOrTeacher**: Accessible by Admin and Teacher roles
- **AdminOrTeacherOrStudent**: Accessible by Admin, Teacher, and Student roles

---

## Notes
- All timestamps are in UTC
- All GUIDs must be valid and non-empty
- Meeting durations are limited to 1-1440 minutes (1 minute to 24 hours)
- Descriptions are limited to 500 characters
- ACS identities are automatically created for attendees when creating a meeting
