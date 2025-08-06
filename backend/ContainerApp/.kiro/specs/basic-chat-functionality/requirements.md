# Requirements Document

## Introduction

This feature implements basic chat functionality for a frontend application to communicate with an AI assistant. The system enables users to send messages and receive AI responses through a synchronous flow across three microservices: Manager (API Gateway), Engine (AI Processing), and Accessor (Data Storage). The implementation includes thread management for conversation history and basic CRUD operations for chat messages.

## Requirements

### Requirement 1

**User Story:** As a frontend user, I want to send a chat message and receive an AI response, so that I can have a conversation with the AI assistant.

#### Acceptance Criteria

1. WHEN a user sends a POST request to `/chat` with a valid message THEN the system SHALL return an AI-generated response within a reasonable time
2. WHEN a user starts a new conversation THEN the frontend SHALL generate a new GUID for threadId
3. WHEN a user sends a message with an existing threadId THEN the system SHALL maintain conversation context from previous messages
4. WHEN the Manager receives a chat request THEN it SHALL validate the input and extract userId from the access token (empty for first version)
5. WHEN the Manager validates the request THEN it SHALL forward the request to the Engine via Dapr
6. WHEN the Engine receives a chat request THEN it SHALL retrieve message history from Accessor
7. IF the threadId exists THEN the Accessor SHALL return the conversation history
8. IF the threadId does not exist THEN the Accessor SHALL create a new thread and return empty/initial history
9. WHEN the Engine has the message history THEN it SHALL call Azure OpenAI to generate a response
10. WHEN the Engine receives the OpenAI response THEN it SHALL store both the user message and assistant response via Accessor
11. WHEN all processing is complete THEN the system SHALL return the assistant response to the frontend

### Requirement 2

**User Story:** As a frontend user, I want to retrieve my previous chat threads, so that I can continue past conversations or review chat history.

#### Acceptance Criteria

1. WHEN a user sends a GET request to `/chat/threads` THEN the system SHALL return a list of the user's chat threads
2. WHEN the Manager receives a threads request THEN it SHALL extract the userId from the access token (empty for first version)
3. WHEN the Manager has the userId THEN it SHALL retrieve the user's threads from Accessor
4. WHEN the Accessor receives a threads request THEN it SHALL return all threads associated with the userId
5. WHEN threads are returned THEN each thread SHALL include threadId, lastMessage, and timestamp
6. IF a user has no previous threads THEN the system SHALL return an empty array

### Requirement 3

**User Story:** As the system, I want to store chat messages and thread information persistently, so that conversation history is maintained across sessions.

#### Acceptance Criteria

1. WHEN a new thread is created THEN the system SHALL store a ChatThread record with threadId, userId, chatType, createdAt, and updatedAt
2. WHEN a message is sent or received THEN the system SHALL store a ChatMessage record with id, threadId, role, content, and timestamp
3. WHEN storing a user message THEN the role SHALL be set to "user"
4. WHEN storing an AI response THEN the role SHALL be set to "assistant"
5. WHEN retrieving message history THEN the system SHALL return messages in chronological order
6. WHEN a thread is accessed THEN the system SHALL update the updatedAt timestamp

### Requirement 4

**User Story:** As a developer, I want well-defined API endpoints across all services, so that the system components can communicate effectively.

#### Acceptance Criteria

1. WHEN the Manager service is deployed THEN it SHALL expose POST `/chat` endpoint for sending messages
2. WHEN the Manager service is deployed THEN it SHALL expose GET `/chat/threads` endpoint for retrieving threads
3. WHEN the Accessor service is deployed THEN it SHALL expose POST `/chat-history/message` endpoint for storing messages
4. WHEN the Accessor service is deployed THEN it SHALL expose GET `/chat-history/{threadId}` endpoint for retrieving thread history
5. WHEN the Accessor service is deployed THEN it SHALL expose GET `/chat-history/threads/{userId}` endpoint for retrieving user threads
6. WHEN the Engine service is deployed THEN it SHALL expose POST `/engine/chat` endpoint for processing chat requests
7. WHEN any endpoint receives a request THEN it SHALL validate the request format and return appropriate error responses for invalid input
8. WHEN any endpoint processes a request successfully THEN it SHALL return the response in the specified JSON format

### Requirement 5

**User Story:** As a system administrator, I want the chat system to handle errors gracefully, so that users receive meaningful feedback when issues occur.

#### Acceptance Criteria

1. WHEN an invalid request is received THEN the system SHALL return a 400 Bad Request with error details
2. WHEN a service is unavailable THEN the system SHALL return a 503 Service Unavailable error
3. WHEN the OpenAI API fails THEN the system SHALL return a 502 Bad Gateway error with appropriate message
4. WHEN a database operation fails THEN the system SHALL return a 500 Internal Server Error
5. WHEN a threadId is not found during history retrieval THEN the system SHALL create a new thread automatically
6. WHEN any error occurs THEN the system SHALL log the error details for debugging purposes