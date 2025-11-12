# Games Endpoints Documentation

## Overview
This document details all the games endpoints available in the Manager service, including game attempt submission, history retrieval, mistake tracking, and sentence generation endpoints.

---

## Table of Contents
1. [Games Endpoints](#games-endpoints)
   - [Submit Attempt](#1-submit-attempt)
   - [Get History](#2-get-history)
   - [Get Mistakes](#3-get-mistakes)
   - [Get All Histories](#4-get-all-histories)
   - [Delete All Games History](#5-delete-all-games-history)
2. [Sentence Generation Endpoints](#sentence-generation-endpoints)
   - [Generate Sentence](#6-generate-sentence)
   - [Generate Split Sentence](#7-generate-split-sentence)

---

# Games Endpoints

**Base Path**: `/games-manager`

**Tag**: `Games`

---

## 1. Submit Attempt
Submits a student's attempt at solving a game exercise.

**Endpoint**: `POST /games-manager/attempt`

**Authorization**: Admin, Teacher, or Student

**Route Parameters**: None

**Request Body**: `SubmitAttemptRequest`
```json
{
  "exerciseId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "givenAnswer": ["word1", "word2", "word3"]
}
```

**Request Schema** (`SubmitAttemptRequest`):
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `exerciseId` | `Guid` | ? | The unique identifier of the exercise |
| `givenAnswer` | `List<string>` | ? | The student's answer as a list of words |

**Response**:
- **200 OK**: Returns `SubmitAttemptResult`
- **400 Bad Request**: Invalid attempt submission
- **401 Unauthorized**: Invalid or missing UserId in token
- **404 Not Found**: Exercise not found
- **500 Internal Server Error**: Failed to submit attempt

**Response Schema** (`SubmitAttemptResult`):
```json
{
  "attemptId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "exerciseId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "studentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "gameType": "WordOrderGame",
  "difficulty": "Easy",
  "status": "Success",
  "correctAnswer": ["word1", "word2", "word3"],
  "attemptNumber": 1,
  "accuracy": 100.0
}
```

| Field | Type | Description |
|-------|------|-------------|
| `attemptId` | `Guid` | The unique identifier of the attempt |
| `exerciseId` | `Guid` | The unique identifier of the exercise |
| `studentId` | `Guid` | The unique identifier of the student |
| `gameType` | `string` | Type of game (see GameType enum) |
| `difficulty` | `Difficulty` | Difficulty level of the exercise |
| `status` | `AttemptStatus` | Status of the attempt |
| `correctAnswer` | `List<string>` | The correct answer |
| `attemptNumber` | `int` | The attempt number for this exercise |
| `accuracy` | `decimal` | Accuracy percentage (0-100) |

**Authorization Rules**:
- Student ID is extracted from the authenticated user's token
- Students can only submit attempts for themselves

---

## 2. Get History
Retrieves game history for a specific student.

**Endpoint**: `GET /games-manager/history/{studentId}`

**Authorization**: Admin, Teacher, or Student

**Route Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `studentId` | `Guid` | ? | The unique identifier of the student |

**Query Parameters**:
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `summary` | `bool` | ? | `false` | Return summary view instead of detailed |
| `page` | `int` | ? | `0` | Page number for pagination |
| `pageSize` | `int` | ? | `10` | Number of items per page |
| `getPending` | `bool` | ? | `false` | Include pending (unanswered) exercises |

**Request Body**: None

**Response**:
- **200 OK**: Returns either `PagedResult<SummaryHistoryDto>` or `PagedResult<AttemptHistoryDto>` depending on `summary` parameter
- **403 Forbidden**: Student trying to view another student's history
- **500 Internal Server Error**: Failed to fetch history

**Response Schema - Summary View** (`PagedResult<SummaryHistoryDto>`):
```json
{
  "items": [
    {
      "gameType": "WordOrderGame",
      "difficulty": "Easy",
      "attemptsCount": 15,
      "totalSuccesses": 12,
      "totalFailures": 3
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1,
  "hasNextPage": false
}
```

**SummaryHistoryDto Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `gameType` | `string` | Type of game |
| `difficulty` | `Difficulty` | Difficulty level |
| `attemptsCount` | `int` | Total number of attempts |
| `totalSuccesses` | `int` | Number of successful attempts |
| `totalFailures` | `int` | Number of failed attempts |

**Response Schema - Detailed View** (`PagedResult<AttemptHistoryDto>`):
```json
{
  "items": [
    {
      "exerciseId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "attemptId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "gameType": "WordOrderGame",
      "difficulty": "Medium",
      "givenAnswer": ["word1", "word2"],
      "correctAnswer": ["word1", "word2", "word3"],
      "status": "Failure",
      "accuracy": 66.67,
      "createdAt": "2024-01-15T10:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 25,
  "hasNextPage": true
}
```

**AttemptHistoryDto Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `exerciseId` | `Guid` | The exercise identifier |
| `attemptId` | `Guid` | The attempt identifier |
| `gameType` | `string` | Type of game |
| `difficulty` | `Difficulty` | Difficulty level |
| `givenAnswer` | `List<string>` | Student's answer |
| `correctAnswer` | `List<string>` | Correct answer |
| `status` | `AttemptStatus` | Attempt status |
| `accuracy` | `decimal` | Accuracy percentage |
| `createdAt` | `DateTimeOffset` | When the attempt was made |

**Authorization Rules**:
- Admin and Teacher users can view any student's history
- Student users can only view their own history (studentId must match authenticated user)

---

## 3. Get Mistakes
Retrieves exercises where the student made mistakes.

**Endpoint**: `GET /games-manager/mistakes/{studentId}`

**Authorization**: Admin, Teacher, or Student

**Route Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `studentId` | `Guid` | ? | The unique identifier of the student |

**Query Parameters**:
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `page` | `int` | ? | `0` | Page number for pagination |
| `pageSize` | `int` | ? | `10` | Number of items per page |

**Request Body**: None

**Response**:
- **200 OK**: Returns `PagedResult<MistakeDto>`
- **403 Forbidden**: Student trying to view another student's mistakes
- **500 Internal Server Error**: Failed to fetch mistakes

**Response Schema** (`PagedResult<MistakeDto>`):
```json
{
  "items": [
    {
      "exerciseId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "gameType": "WordOrderGame",
      "difficulty": "Hard",
      "correctAnswer": ["word1", "word2", "word3"],
      "mistakes": [
        {
          "attemptId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "wrongAnswer": ["word2", "word1", "word3"],
          "accuracy": 33.33,
          "createdAt": "2024-01-15T10:00:00Z"
        }
      ]
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 5,
  "hasNextPage": false
}
```

**MistakeDto Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `exerciseId` | `Guid` | The exercise identifier |
| `gameType` | `string` | Type of game |
| `difficulty` | `Difficulty` | Difficulty level |
| `correctAnswer` | `List<string>` | The correct answer |
| `mistakes` | `List<MistakeAttemptDto>` | List of failed attempts for this exercise |

**MistakeAttemptDto Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `attemptId` | `Guid` | The attempt identifier |
| `wrongAnswer` | `List<string>` | The student's incorrect answer |
| `accuracy` | `decimal` | Accuracy percentage |
| `createdAt` | `DateTimeOffset` | When the mistake was made |

**Authorization Rules**:
- Admin and Teacher users can view any student's mistakes
- Student users can only view their own mistakes

---

## 4. Get All Histories
Retrieves game history for all students (admin/teacher only).

**Endpoint**: `GET /games-manager/all-history`

**Authorization**: Admin or Teacher

**Route Parameters**: None

**Query Parameters**:
| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `page` | `int` | ? | `0` | Page number for pagination |
| `pageSize` | `int` | ? | `10` | Number of items per page |

**Request Body**: None

**Response**:
- **200 OK**: Returns paginated game history for all students
- **401 Unauthorized**: Not authenticated
- **403 Forbidden**: User is not Admin or Teacher
- **500 Internal Server Error**: Failed to fetch histories

**Authorization Rules**:
- Only Admin and Teacher roles can access this endpoint

---

## 5. Delete All Games History
Deletes all games history from the system (admin only).

**Endpoint**: `DELETE /games-manager/all-history`

**Authorization**: Admin Only

**Route Parameters**: None

**Request Body**: None

**Response**:
- **200 OK**: Returns success message
```json
{
  "message": "All games history deleted."
}
```
- **401 Unauthorized**: Not authenticated
- **403 Forbidden**: User is not Admin
- **500 Internal Server Error**: Failed to delete games history

**Authorization Rules**:
- Only Admin role can access this endpoint
- This is a destructive operation that removes all game history data

---

# Sentence Generation Endpoints

**Base Path**: `/ai-manager`

**Tag**: `AI`

**Authorization**: Admin, Teacher, or Student (all sentence endpoints)

---

## 6. Generate Sentence
Generates sentences for game exercises based on specified difficulty and other parameters.

**Endpoint**: `POST /ai-manager/sentence`

**Authorization**: Admin, Teacher, or Student

**Route Parameters**: None

**Request Body**: `SentenceRequestDto`
```json
{
  "difficulty": "Medium",
  "nikud": false,
  "count": 5,
  "gameType": "WordOrderGame"
}
```

**Request Schema** (`SentenceRequestDto`):
| Field | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| `difficulty` | `Difficulty` | ? | `Medium` | Difficulty level for sentences |
| `nikud` | `bool` | ? | `false` | Whether to include Hebrew vowel marks (nikud) |
| `count` | `int` | ? | `1` | Number of sentences to generate (1-10) |
| `gameType` | `GameType` | ? | `WordOrderGame` | Type of game the sentences are for |

**Response**:
- **200 OK**: Request accepted (sentences will be delivered via SignalR)
- **400 Bad Request**: Request is null or invalid
- **403 Forbidden**: Invalid or missing UserId
- **408 Request Timeout**: Sentence generation timed out
- **499 Client Closed Request**: Operation was canceled by user
- **500 Internal Server Error**: An error occurred during sentence generation

**Response Body**:
```json
{}
```

**Actual Response Delivery**:
The generated sentences are delivered asynchronously via **SignalR** with event type `SentenceGeneration`. The payload contains a list of `AttemptedSentenceResult`:

```json
{
  "eventType": "SentenceGeneration",
  "payload": [
    {
      "exerciseId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "original": "äùîù æåøçú áùîéí",
      "words": ["äùîù", "æåøçú", "áùîéí"],
      "difficulty": "Medium",
      "nikud": false
    }
  ]
}
```

**AttemptedSentenceResult Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `exerciseId` | `Guid` | Unique identifier for the exercise |
| `original` | `string` | The original complete sentence |
| `words` | `List<string>` | The sentence split into words |
| `difficulty` | `string` | Difficulty level |
| `nikud` | `bool` | Whether nikud is included |

**Notes**:
- User ID is automatically extracted from the authentication token
- Sentences are generated asynchronously by the Engine service
- Results are pushed to the client via SignalR when ready
- Typical generation time: 5-20 seconds depending on count and complexity

---

## 7. Generate Split Sentence
Generates sentences specifically formatted for split/typing practice games.

**Endpoint**: `POST /ai-manager/sentence/split`

**Authorization**: Admin, Teacher, or Student

**Route Parameters**: None

**Request Body**: `SentenceRequestDto`
```json
{
  "difficulty": "Easy",
  "nikud": true,
  "count": 3,
  "gameType": "TypingPractice"
}
```

**Request Schema**: Same as [Generate Sentence](#6-generate-sentence)

**Response**:
- **200 OK**: Request accepted (sentences will be delivered via SignalR)
- **400 Bad Request**: Request is null or invalid
- **403 Forbidden**: Invalid or missing UserId
- **408 Request Timeout**: Split sentence generation timed out
- **499 Client Closed Request**: Operation was canceled by user
- **500 Internal Server Error**: An error occurred during split sentence generation

**Response Body**:
```json
{}
```

**Actual Response Delivery**:
The generated split sentences are delivered asynchronously via **SignalR** with event type `SplitSentenceGeneration`. The payload format is the same as the regular sentence generation:

```json
{
  "eventType": "SplitSentenceGeneration",
  "payload": [
    {
      "exerciseId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "original": "äÇéÆÌìÆã ÷åÉøÅà ñÅôÆø",
      "words": ["äÇéÆÌìÆã", "÷åÉøÅà", "ñÅôÆø"],
      "difficulty": "Easy",
      "nikud": true
    }
  ]
}
```

**Notes**:
- Optimized for typing practice and split sentence games
- User ID is automatically extracted from the authentication token
- Sentences are generated asynchronously by the Engine service
- Results are pushed to the client via SignalR when ready
- The split format may have different word boundaries or formatting compared to regular sentence generation

---

## Enums

### GameType
Defines the type of game.

**Values**:
- `WordOrderGame` - Game where students arrange words in correct order
- `TypingPractice` - Typing practice game
- `SpeakingPractice` - Speaking practice game

### Difficulty
Defines the difficulty level.

**Values**:
- `Easy` - Easy difficulty level
- `Medium` - Medium difficulty level
- `Hard` - Hard difficulty level

### AttemptStatus
Defines the status of a game attempt.

**Values**:
- `Pending` - Exercise generated but not yet attempted
- `Success` - Student answered correctly
- `Failure` - Student answered incorrectly

---

## Common Models

### PagedResult&lt;T&gt;
A generic wrapper for paginated results.

**Fields**:
| Field | Type | Description |
|-------|------|-------------|
| `items` | `IEnumerable<T>` | The items in the current page |
| `page` | `int` | Current page number |
| `pageSize` | `int` | Number of items per page |
| `totalCount` | `int` | Total number of items across all pages |
| `hasNextPage` | `bool` | Whether there are more pages available |

---

## Authentication
All endpoints require authentication. The user ID is extracted from the authentication token (typically JWT).

**Claims Used**:
- **User ID Claim**: Identifies the authenticated user
- **Role Claim**: Identifies the user's role (Admin, Teacher, or Student)

## Authorization Policies
- **AdminOnly**: Only Admin role
- **AdminOrTeacher**: Admin and Teacher roles
- **AdminOrTeacherOrStudent**: All authenticated users

---

## SignalR Integration

The sentence generation endpoints (`/sentence` and `/sentence/split`) use SignalR for asynchronous result delivery:

1. **Client connects** to SignalR hub before making requests
2. **Client sends** HTTP POST request to sentence generation endpoint
3. **Server accepts** request and returns 200 OK immediately
4. **Server processes** sentence generation asynchronously
5. **Server pushes** results to client via SignalR when ready

**SignalR Event Types**:
- `SentenceGeneration` - For regular sentence generation
- `SplitSentenceGeneration` - For split sentence generation

**Event Payload**: Array of `AttemptedSentenceResult` objects

---

## Notes

- All timestamps are in UTC
- All GUIDs must be valid and non-empty
- Pagination is 0-indexed
- Default page size is typically 10 items
- Accuracy is calculated as a percentage (0-100)
- Sentence generation is asynchronous and results are delivered via SignalR
- Game history includes both completed and pending exercises (based on `getPending` parameter)
- Students can only access their own data unless they have Admin or Teacher roles
