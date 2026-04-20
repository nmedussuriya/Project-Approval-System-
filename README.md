# Project Approval System (PAS)

##  Overview
The Project Approval System (PAS) is a web-based application developed using ASP.NET Core MVC to automate the process of matching student projects with academic supervisors.

The system implements a **Blind Matching mechanism**, ensuring fair selection based on technical merit rather than personal bias.

---

##  Features

### Student
- Secure login and registration
- Submit project proposals
- Edit or delete proposals
- Track project status (Pending / Under Review / Matched)
- View supervisor details after match confirmation

### Supervisor
- Select research areas of expertise
- View anonymous project proposals
- Express interest in projects
- Confirm matches
- View student details after matching

###  Admin
- Manage users (Students & Supervisors)
- Manage research areas
- Monitor project-supervisor matches

---

##  Blind Matching Logic
1. Student submits project
2. Supervisor reviews anonymously
3. Supervisor expresses interest
4. Match is confirmed
5. System reveals identities

---

##  Technology Stack
- **Frontend:** HTML, CSS, Bootstrap
- **Backend:** ASP.NET Core MVC
- **Database:** SQL Server
- **ORM:** Entity Framework Core
- **Authentication:** ASP.NET Identity

---

##  Setup Instructions

1. Clone the repository
2. Open the project in Visual Studio
3. Run the following command in Package Manager Console:

```powershell
Update-Database
