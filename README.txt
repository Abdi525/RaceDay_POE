RACEDAY POE - PART 2 API + PART 3 MVC

PROJECTS
- Part2_API: ASP.NET Core Web API (project: poe11.csproj)
- Part3_MVC: ASP.NET Core MVC website (project: POE_P3_WEB.csproj)

IMPORTANT
The MVC website consumes the REST API in Part2_API. Do not run the MVC project by itself.
No Docker is included or required.
Event banner images and participant profile images are stored locally in Part3_MVC/wwwroot/images.
Part 3 includes organiser event management, category management, participant enrolment lists,
result recording, participant event entries, results and profile image upload.

DATABASE
The API uses SQL Server LocalDB and the database name RaceDayAU.
The API creates the database on first startup using EnsureCreated().
Make sure SQL Server LocalDB is installed on your Windows computer.

HOW TO RUN IN VISUAL STUDIO
1. Open Part2_API/poe11.csproj and run the 'poe11' HTTP profile.
   API base address: http://localhost:5122
   Swagger: http://localhost:5122/swagger
2. Keep the API running.
3. Open Part3_MVC/POE_P3_WEB.csproj and run the 'http' profile.
   MVC website: http://localhost:5285
4. Register an Organiser account and create an event.
5. Register a Participant account, browse events and enrol in a category.

API CONNECTION
Part3_MVC/appsettings.json contains:
ApiSettings:BaseUrl = http://localhost:5122/api/

If you change the API port, update this setting to match.

AUTHENTICATION
The API now issues JWT tokens at /api/auth/login. The MVC application stores the token in its server-side session and sends it as a Bearer token with protected API requests. API role checks use the token's role claim.

DATABASE NOTE
EnsureCreated creates a new RaceDayAU database if one does not already exist. If you already have an older database with a different schema, back it up before testing and apply the required schema changes separately.

BUILD NOTE
The .NET SDK was not available in the file-processing environment, so a local dotnet build could not be run here. Open the projects in Visual Studio, restore NuGet packages, then build and run using the steps above.
