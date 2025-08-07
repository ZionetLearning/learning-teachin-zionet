# Requirements Document

## Introduction

This feature adds a synchronous chat endpoint to the Engine service that allows the Manager to send chat requests directly and receive immediate responses. The endpoint will handle chat history retrieval, OpenAI integration, and storage of both questions and responses through the Accessor service.

## Requirements

### Requirement 1

**User Story:** As a Manager service, I want to send a chat request to the Engine and receive an immediate response, so that I can provide real-time chat functionality to end users.

#### Acceptance Criteria

1. WHEN the Manager sends a POST request to `/chat` with threadId, userMessage, and userId THEN the Engine SHALL respond synchronously with an assistantMessage
2. WHEN a chat request is received THEN the Engine SHALL validate that all required fields (threadId, userMessage, userId) are present
3. IF any required field is missing THEN the Engine SHALL return a 400 Bad Request response with validation error details

### Requirement 2

**User Story:** As the Engine service, I want to retrieve existing chat history for a thread, so that I can provide contextual responses from OpenAI.

#### Acceptance Criteria

1. WHEN processing a chat request THEN the Engine SHALL retrieve chat history from the Accessor service using the threadId
2. WHEN chat history is retrieved THEN the Engine SHALL include it in the context sent to OpenAI

### Requirement 3

**User Story:** As the Engine service, I want to send the user message and chat history to OpenAI, so that I can generate contextually appropriate responses.

#### Acceptance Criteria

1. WHEN chat history is loaded THEN the Engine SHALL format the conversation history for OpenAI API
2. WHEN sending to OpenAI THEN the Engine SHALL include both the current user message and historical context
WHEN OpenAI responds THEN the Engine SHALL extract the assistant message from the aiResp model returned by ai.ProcessAsync(aiReq, ct)

### Requirement 4

**User Story:** As the Engine service, I want to store both the user question and AI response in the database, so that chat history is persisted for future conversations.

#### Acceptance Criteria

1. WHEN an AI response is received THEN the Engine SHALL send both the user message and assistant response to the Accessor PUT endpoint
2. WHEN sending to Accessor THEN the Engine SHALL include the threadId and userId in the request
3. IF the Accessor PUT request fails THEN the Engine SHALL log the error but still return the response to the Manager

### Requirement 5

**User Story:** As the Engine service, I want to handle errors gracefully during the chat process, so that the system remains stable and provides meaningful feedback.

#### Acceptance Criteria

1. IF OpenAI API call fails THEN the Engine SHALL return a 500 Internal Server Error with appropriate error message
2. IF Accessor service is unavailable THEN the Engine SHALL attempt to continue without chat history and log the issue
3. WHEN any error occurs THEN the Engine SHALL log detailed error information for debugging purposes
