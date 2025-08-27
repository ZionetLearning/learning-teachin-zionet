# 🔐 Authentication System – Manager API

This document outlines the authentication & authorization mechanism used in the `Manager` service. It covers the endpoints, token handling, and session security architecture.

---

## 📌 Overview

This service uses **JWT-based authentication** combined with secure, HttpOnly **refresh token cookies** for session management. It also implements CSRF protection, session fingerprinting, and rate-limiting for enhanced security.

---

## ⚙️ Technologies Used

* **ASP.NET Core Minimal APIs**
* **JWT Bearer Tokens**
* **Refresh Token Sessions (stored in DB via Accessor client)**
* **CSRF protection**
* **HMAC-SHA256 hashing**
* **Dapr** for inter-service communication
* **Rate Limiting** with IP-based partitioning

---

## 📂 API Endpoints

All endpoints are grouped under `/auth` and tagged with `Auth`.

### 🔐 `POST /auth/login`

Authenticates a user via email/password.

**Body:**

```json
{
  "email": "user@example.com",
  "password": "superSecret123"
}
```

**Response:**

```json
{
  "accessToken": "<JWT_ACCESS_TOKEN>"
}
```

* Sets a **HttpOnly refresh token cookie** with CSRF protection headers.
* Access token is returned in body for use in `Authorization: Bearer` headers.

---

### 🔄 `POST /auth/refresh-tokens`

Rotates the refresh token and returns a new access token.

* Requires valid refresh token in cookie
* Requires CSRF header: `X-CSRF-Token`
* Rate-limited via IP to prevent abuse

**Response:**

```json
{
  "accessToken": "<NEW_JWT_ACCESS_TOKEN>"
}
```

---

### 🚪 `POST /auth/logout`

Revokes the refresh token session.

* Deletes the refresh session from DB
* Clears cookies

**Response:**

```json
{
  "message": "Logged out successfully"
}
```

---

### ✅ `GET /auth/protected`

Test endpoint to verify authentication.

Returns the `UserId` parsed from JWT:

```json
{
  "message": "You are authenticated! , UserId: <guid>"
}
```

---

## 🔑 Token Structure

### JWT Access Token

* Short-lived (e.g. 15 minutes)
* Contains:

  * `sub`: UserId (as GUID)
  * `iss`, `aud`: from config

### Refresh Token

* Random 32-char string (GUID-based)
* Stored in HttpOnly cookie
* Server stores **hashed version** + metadata (IP, UA, fingerprint, timestamps)

---

## 🛡 Security Features

| Feature                   | Description                                       |
| ------------------------- | ------------------------------------------------- |
| **HttpOnly Cookies**      | Refresh token stored securely in cookie           |
| **CSRF Protection**       | Uses double-submit (cookie + header token)        |
| **IP Validation**         | Refresh request IP must match saved session       |
| **User-Agent Check**      | Must match the initial login's browser info       |
| **Device Fingerprinting** | Optional header `x-fingerprint` hashed & verified |
| **HMAC-SHA256**           | Refresh tokens are stored as secure hashes        |
| **Rate Limiting**         | IP-based limits for refresh token requests        |

---

## 📦 Configuration (`appsettings.Local.json`)

```json
"Jwt": {
  "Secret": "<secure_key>",
  "Issuer": "manager",
  "Audience": "client-app",
  "RefreshTokenHashKey": "<refresh_hash_key>",
  "AccessTokenTTL": 15,
  "RefreshTokenTTL": 60
},
"RateLimiting": {
  "RefreshToken": {
    "PermitLimit": 5,
    "WindowMinutes": 1,
    "QueueLimit": 1,
    "RejectionStatusCode": 429
  }
}
```

---

## 🧪 Testing Tips

* Use Postman or a frontend to trigger `/login` and extract `accessToken`.
* Use browser DevTools > Application > Cookies to inspect refresh/CSRF cookies.
* Test `/refresh-tokens` with:

  * Cookie: `refresh-token`
  * Header: `X-CSRF-Token`
* Add `Authorization: Bearer <accessToken>` header to test protected routes.

---

## 🧼 Example Cookie Headers

```http
Set-Cookie: refresh-token=abc123...; HttpOnly; Secure
Set-Cookie: csrf-token=xyz987...; Secure
```

---

## 📚 Related Files

| File                     | Purpose                                      |
| ------------------------ | -------------------------------------------- |
| `AuthEndpoints.cs`       | API route handlers                           |
| `AuthService.cs`         | Core business logic for login/refresh/logout |
| `Program.cs`             | JWT + Auth config and DI setup               |
| `appsettings.Local.json` | Secrets and TTL settings                     |

---
\br
\br
\br
\br
\br
\br
\br
\br
\br
\br
\br
\br
\br
\br
## ✅ Best Practices Summary

* 🔒 Always hash and store refresh tokens securely
* ⏳ Expire access tokens quickly
* 🚫 Invalidate sessions on logout
* 🧠 Use consistent fingerprinting for device validation
* 🧵 Use structured logging (with `BeginScope`) for tracing auth flows

---
