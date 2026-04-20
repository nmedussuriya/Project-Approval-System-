 Project Approval System (PAS)
 Overview

The Project Approval System (PAS) is a secure web-based application developed using ASP.NET Core MVC to streamline the process of assigning supervisors to student research projects.
The system implements a Blind Matching mechanism, ensuring that supervisors select projects based solely on technical merit and research area alignment, eliminating bias during the initial selection phase.

Project Objectives

Automate the supervisor allocation process
Ensure fair and unbiased project selection
Match students with supervisors based on expertise
Improve transparency and efficiency in academic project management

Technology Stack
Frontend: HTML, CSS, Bootstrap
Backend: ASP.NET Core MVC (.NET 8)
Database: SQL Server
ORM: Entity Framework Core
Authentication: ASP.NET Core Identity
Version Control: Git & GitHub

User Roles & Functionalities
  1.Student
Secure registration and login
Submit project proposals (Title, Abstract, Tech Stack, Research Area)
Edit or delete proposals before matching
Track project status (Pending / Under Review / Matched)
View supervisor details after match confirmation
  2.Supervisor
Select research areas of expertise
View anonymous project proposals (Blind Matching)
Express interest in projects
Confirm matches
View student details after identity reveal
  3.Admin / Module Leader
Manage users (Students and Supervisors)
Manage research areas
Monitor all project-supervisor matches
Reassign or override matches if necessary
 -- Key Feature: Blind Matching

The system ensures anonymity during the initial selection process:
Student identity is hidden
Supervisor evaluates only:
Project Title
Abstract
Tech Stack
Research Area

Once a match is confirmed:

Student and Supervisor identities are revealed
Collaboration can begin
-- System Workflow
Student submits a project proposal
Supervisor reviews projects anonymously
Supervisor expresses interest
Match is confirmed
Identity reveal occurs
Student and Supervisor begin collaboration

--System Architecture
The application follows the MVC (Model-View-Controller) architecture:
Models → Represent database entities
Views → User interface (Razor pages)
Controllers → Handle application logic and requests

This ensures separation of concerns, maintainability, and scalability.

📂 Project Structure
PAS_Full_System/
│
├── Controllers/        # Application logic
├── Models/             # Data models
├── Views/              # UI (Razor Views)
│   ├── Home/
│   ├── Project/
│   ├── Supervisor/
│   ├── Admin/
│   ├── Profile/
│   └── Shared/
│
├── Data/               # DbContext and EF Core Migrations
├── Areas/Identity/     # Authentication & Authorization
├── wwwroot/            # Static assets (CSS, JS, Images)
│
├── Program.cs          # Application configuration
├── appsettings.json    # App settings & DB connection
└── PAS_Full_System.csproj


Entity Framework Core is used for database management and migrations.

 Setup Instructions
1️⃣ Clone the Repository
git clone https://github.com/your-repo-link.git
cd PAS_Full_System
2️⃣ Open in Visual Studio
Open the solution file (.sln) using Visual Studio 2022
3️⃣ Configure Database

Open Package Manager Console and run:

Update-Database
4️⃣ Run the Application
Press F5 or click Run

--Security Features
ASP.NET Core Identity authentication
Role-Based Access Control (RBAC)
Secure password handling
Authorization for protected routes

--Version Control Strategy
Git-based version control
Feature-based commits
Clear commit messages
Collaborative development using branches
