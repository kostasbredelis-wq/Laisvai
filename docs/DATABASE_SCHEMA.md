# Laisvai — Database Schema

## Design Principles
- Auth is separated from profile data
- Skills are relational, not stored as strings
- Conversations are a first-class entity for clean messaging logic
- All major tables have CreatedAt and UpdatedAt timestamps
- Unique constraints are enforced at the database level where needed

---

## Users
| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | Auto-increment |
| Email | string | Unique, required |
| PasswordHash | string | Bcrypt hashed |
| Role | enum | Freelancer, Client, Admin |
| IsActive | bool | False until approved (freelancers), true by default (clients) |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

---

## FreelancerProfiles
| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| UserId | int (FK → Users) | One-to-one, unique |
| FullName | string | |
| Bio | string | Nullable |
| Category | enum | WebDeveloper, MobileDeveloper, UIUXDesigner, GraphicDesigner |
| PortfolioUrl | string | Nullable |
| AvatarUrl | string | Nullable |
| ApplicationStatus | enum | Pending, Approved, Rejected |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

---

## ClientProfiles
| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| UserId | int (FK → Users) | One-to-one, unique |
| DisplayName | string | Company or individual name |
| About | string | Nullable |
| Website | string | Nullable |
| AvatarUrl | string | Nullable |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

---

## Skills
| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| Name | string | Unique (e.g. "React", "Figma", "Flutter") |

---

## FreelancerSkills
| Column | Type | Notes |
|--------|------|-------|
| FreelancerId | int (FK → FreelancerProfiles) | Composite PK with SkillId |
| SkillId | int (FK → Skills) | Composite PK with FreelancerId |

---

## Services
| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| FreelancerId | int (FK → FreelancerProfiles) | |
| Title | string | |
| Description | string | |
| Category | enum | Same as FreelancerProfiles category |
| PricingType | enum | Negotiable |
| IsActive | bool | Default true |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

---

## JobListings
| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| ClientId | int (FK → ClientProfiles) | |
| Title | string | |
| Description | string | |
| Category | enum | Same categories |
| BudgetType | enum | Negotiable |
| Deadline | datetime | Nullable |
| IsOpen | bool | Default true |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

---

## JobListingSkills
| Column | Type | Notes |
|--------|------|-------|
| JobListingId | int (FK → JobListings) | Composite PK with SkillId |
| SkillId | int (FK → Skills) | Composite PK with JobListingId |

---

## Applications
| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| JobListingId | int (FK → JobListings) | |
| FreelancerId | int (FK → FreelancerProfiles) | |
| CoverMessage | string | |
| Status | enum | Pending, Accepted, Rejected |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

Unique constraint on (JobListingId, FreelancerId) — a freelancer can only apply once per listing.

---

## Conversations
| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| FreelancerId | int (FK → FreelancerProfiles) | |
| ClientId | int (FK → ClientProfiles) | |
| JobListingId | int (FK → JobListings) | Nullable — set when freelancer initiates about a listing |
| ApplicationId | int (FK → Applications) | Nullable — set when application is accepted |
| CreatedAt | datetime | |
| UpdatedAt | datetime | Updated when a new message is sent, useful for sorting inbox by recent activity |

Unique constraint on (FreelancerId, ClientId, JobListingId) — prevents duplicate conversations about the same listing.

---

## Messages
| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| ConversationId | int (FK → Conversations) | |
| SenderId | int (FK → Users) | |
| Content | string | |
| IsRead | bool | Default false |
| SentAt | datetime | |

---

## Reviews
| Column | Type | Notes |
|--------|------|-------|
| Id | int (PK) | |
| ClientId | int (FK → ClientProfiles) | Who left the review |
| FreelancerId | int (FK → FreelancerProfiles) | Who received the review |
| Rating | int | 1–5 |
| Comment | string | |
| CreatedAt | datetime | |

Unique constraint on (ClientId, FreelancerId) — a client can only review a freelancer once.

---

## Relationships Summary

- One User → one FreelancerProfile or one ClientProfile (never both)
- One FreelancerProfile → many FreelancerSkills → many Skills
- One FreelancerProfile → many Services
- One FreelancerProfile → many Applications
- One FreelancerProfile → many Reviews (as subject)
- One ClientProfile → many JobListings
- One JobListing → many JobListingSkills → many Skills
- One JobListing → many Applications
- One Conversation → many Messages
- One FreelancerProfile + one ClientProfile → many Conversations
- One ClientProfile → many Reviews (as author)