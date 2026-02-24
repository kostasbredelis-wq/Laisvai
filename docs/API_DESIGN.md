# Laisvai — API Design

## Base URL
`/api`

## Authentication
All protected routes require a JWT token in the Authorization header:
`Authorization: Bearer <token>`

Roles:
- Public — no token required
- Freelancer — approved freelancer account required
- Client — client account required
- Admin — admin account required

---

## Auth

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | /api/auth/register | Public | Register as Freelancer or Client |
| POST | /api/auth/login | Public | Login, returns JWT token |

### POST /api/auth/register
```json
{
  "email": "string",
  "password": "string",
  "role": "Freelancer | Client"
}
```

### POST /api/auth/login
```json
{
  "email": "string",
  "password": "string"
}
```
Response:
```json
{
  "token": "string",
  "role": "string",
  "userId": "int"
}
```

---

## Freelancers

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | /api/freelancers | Public | Browse approved freelancers with filters |
| GET | /api/freelancers/{id} | Public | Get freelancer public profile |
| GET | /api/freelancers/me | Freelancer | Get own profile |
| POST | /api/freelancers/profile | Freelancer | Create own profile |
| PUT | /api/freelancers/me | Freelancer | Update own profile |

### Query Parameters — GET /api/freelancers
- `category` — WebDeveloper, MobileDeveloper, UIUXDesigner, GraphicDesigner
- `skillId` — filter by skill ID
- `minRating` — minimum average rating (1–5)
- `sort` — newest, highest_rated

---

## Skills

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | /api/skills | Public | Get all available skills |
| POST | /api/skills | Admin | Add a new skill to the platform |
| DELETE | /api/skills/{id} | Admin | Remove a skill |

---

## Services

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | /api/services | Public | Browse all active service listings |
| GET | /api/services/{id} | Public | Get a single service listing |
| GET | /api/services/me | Freelancer | Get own service listings |
| POST | /api/services | Freelancer | Create a service listing |
| PUT | /api/services/{id} | Freelancer | Update own service listing |
| DELETE | /api/services/{id} | Freelancer | Delete own service listing |

---

## Job Listings

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | /api/jobs | Public | Browse open job listings with filters |
| GET | /api/jobs/{id} | Public | Get a single job listing |
| GET | /api/jobs/me | Client | Get own job listings |
| POST | /api/jobs | Client | Post a new job listing |
| PUT | /api/jobs/{id} | Client | Update own job listing |
| DELETE | /api/jobs/{id} | Client | Delete own job listing |
| PATCH | /api/jobs/{id}/close | Client | Close a job listing |

### Query Parameters — GET /api/jobs
- `category` — filter by category
- `skillId` — filter by required skill ID
- `isOpen` — true / false
- `sort` — newest

---

## Applications

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | /api/jobs/{id}/apply | Freelancer | Apply to a job listing |
| GET | /api/jobs/{id}/applications | Client | View applications for own listing |
| GET | /api/applications/me | Freelancer | View own applications and statuses |
| PATCH | /api/applications/{id}/accept | Client | Accept an application, creates a conversation |
| PATCH | /api/applications/{id}/reject | Client | Reject an application |

---

## Conversations

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | /api/conversations | Freelancer, Client | Get all own conversations |
| GET | /api/conversations/{id} | Freelancer, Client | Get a single conversation with messages |

Note: Conversations are created automatically when a client accepts an application.
They are never created manually via the API.

---

## Messages

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | /api/conversations/{id}/messages | Freelancer, Client | Send a message in a conversation |
| PATCH | /api/conversations/{id}/read | Freelancer, Client | Mark conversation as read |

### POST /api/conversations/{id}/messages
```json
{
  "content": "string"
}
```

---

## Reviews

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | /api/reviews | Client | Leave a review on a freelancer |
| GET | /api/reviews/freelancer/{id} | Public | Get all reviews for a freelancer |

### POST /api/reviews
```json
{
  "freelancerId": "int",
  "rating": "int (1-5)",
  "comment": "string"
}
```

---

## Admin

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | /api/admin/applications | Admin | View all pending freelancer applications |
| PATCH | /api/admin/freelancers/{id}/approve | Admin | Approve a freelancer |
| PATCH | /api/admin/freelancers/{id}/reject | Admin | Reject a freelancer |
| PATCH | /api/admin/users/{id}/deactivate | Admin | Deactivate a user account |