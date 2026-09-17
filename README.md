# StudyPlanner API

Backend-API för StudyPlanner – en webbapplikation för att hantera studieuppgifter.

## Kom igång

### Förutsättningar

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Starta backend

```bash
git clone https://github.com/filipogit/studyplanner.git
cd studyplanner
dotnet ef database update
dotnet run
```

API:et startar på `https://localhost:7257` (eller `http://localhost:5237`).

Swagger-dokumentation finns på `https://localhost:7257/swagger` i development-läge.

## API-endpoints

| Metod | URL | Beskrivning |
|-------|-----|-------------|
| GET | /api/tasks | Hämta alla uppgifter |
| GET | /api/tasks/{id} | Hämta en uppgift |
| POST | /api/tasks | Skapa en uppgift |
| PUT | /api/tasks/{id} | Uppdatera en uppgift |
| POST | /api/tasks/{id}/upload | Ladda upp fil till en uppgift |
| GET | /api/tasks/{taskId}/files/{fileId} | Hämta en uppladdad fil |

## Tekniska val

### ASP.NET WebAPI
Valde ASP.NET WebAPI som backend-ramverk då det ger en tydlig struktur med controllers och har inbyggt stöd för modellvalidering, content negotiation och Swagger-dokumentation.

### Entity Framework Core med SQLite
SQLite valdes som databas för att det inte kräver en separat databasserver – databasen skapas automatiskt som en lokal fil. Entity Framework Core ger typsäker databasåtkomst och migrationer för att versionshantera databasschemat.

### Global felhantering med middleware
En custom middleware fångar alla ohanterade exceptions och returnerar ett strukturerat JSON-svar med statuskod 500. Detta förhindrar att API:et kraschar och ger frontenden ett konsekvent felformat att hantera.

### Filuppladdning med fysisk lagring
Uppladdade filer sparas på disk i en Uploads-mapp med unika filnamn (GUID) för att undvika namnkonflikter. Filens metadata (originalnamn, storlek, content type) lagras i databasen med en relation till uppgiften.

### CORS
CORS är konfigurerat för att tillåta anrop från React-frontenden (localhost:5173) under utveckling.
