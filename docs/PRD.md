# Laisvai — Product Requirements Document

## Overview
Laisvai is a Lithuanian-language web platform connecting tech and creative freelancers
with businesses and individuals looking to hire them. Freelancers can post their 
services and apply to job listings. Clients can post job listings and browse 
freelancer profiles. Messaging only opens once a client accepts a freelancer's 
application.

## Target Users

**Freelancers**
Lithuanian-based tech and creative professionals looking for project-based work:
- Web developers
- Mobile developers
- UI/UX designers
- Graphic designers

**Clients**
Lithuanian businesses, startups, or individuals who need tech or creative work done
on a project or ongoing basis.

## Problem It Solves
There is no dedicated Lithuanian platform for tech freelancers. Professionals either
use global platforms like Upwork (language and cultural barriers) or rely purely on
personal networks. Laisvai fills this gap with a localized, Lithuanian-first 
experience.

## Core Interaction Model
1. Freelancer registers and submits an application to join the platform
2. Admin approves or rejects the freelancer application
3. Approved freelancer sets up their profile, skills, and service listings
4. Client registers freely and posts job listings
5. Freelancer browses job listings and applies with a cover message
6. Client reviews applications and accepts or rejects them
7. Once a client accepts a freelancer's application, both sides can message each other
8. Client can leave a review on the freelancer after working together

## Features In Scope (MVP)

### Authentication
- Register and login via email and password
- Role selection at registration: Freelancer or Client
- Freelancer accounts require admin approval before becoming active
- Client accounts are active immediately after registration
- JWT-based session management

### Freelancer Side
- Submit application to join the platform (pending admin approval)
- Create and manage a profile: full name, bio, category, skills, portfolio link, avatar
- Post, edit, and delete service listings with title, description, category, 
  and negotiable pricing
- Browse open job listings with search and filtering
- Apply to job listings with a cover message (one application per listing)
- View own application statuses
- Message a client once the client has accepted the freelancer's application
- View reviews left by clients on their profile

### Client Side
- Register and immediately access the platform
- Create and manage a profile: display name, about, website, avatar
- Post, edit, and delete job listings with title, description, category, 
  required skills, negotiable budget, and optional deadline
- Close a job listing when no longer accepting applications
- Browse approved freelancer profiles and service listings with search and filtering
- View applications received on own job listings
- Accept or reject freelancer applications
- Message a freelancer once their application has been accepted
- Leave a star rating and text review on a freelancer (once per freelancer)

### Skills
- A predefined list of skills exists on the platform (e.g. React, Figma, Flutter)
- Freelancers select their skills from this list when setting up their profile
- Clients select required skills from this list when posting a job listing
- Skills are used for search and filtering across the platform

### Search and Filtering
- Search freelancers by name or skill
- Filter freelancers by category and minimum average rating
- Search job listings by title or required skill
- Filter job listings by category and whether they are still open
- Sort results by newest or highest rated (freelancers)

### Messaging
- Messaging between a freelancer and client is only unlocked when the client 
  accepts the freelancer's application
- Each accepted application creates one conversation between the two parties
- Simple threaded inbox showing all active conversations
- Unread message indicator per conversation

### Reviews
- Only clients can leave reviews on freelancers
- A client can only leave one review per freelancer
- Review consists of a star rating (1–5) and a text comment
- Reviews are visible on the freelancer's public profile
- Average rating is calculated and displayed on the profile

### Admin
- View all pending freelancer applications
- Approve or reject freelancer applications
- Deactivate user accounts

## Out of Scope for MVP
- Payments and escrow
- Contracts
- Email or push notifications
- Mobile app
- Video calls
- Freelancers reviewing clients
- Social features (following, sharing, liking)
- Admin analytics dashboard

## Success Metrics
- Number of approved freelancer profiles
- Number of job listings posted
- Number of applications submitted and accepted
- Number of conversations initiated
- Number of reviews left