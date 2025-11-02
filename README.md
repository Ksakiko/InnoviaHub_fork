# InnoviaHub (Skolprojekt både i grupp och individuellt)

Ett enkelt bokningssystem för resurser och faciliteter.

## Vad är InnoviaHub?

InnoviaHub är en webbapplikation där användare kan:

- Logga in säkert med Microsoft-konto
- Boka rum och resurser (Just nu går det bara att boka en hel dag per resurs)
- Se tillgängliga resurser i realtid
- Hantera sina bokningar

## Produktionslänk

https://jellyfish-app-ouuck.ondigitalocean.app/

## Tekniker och ramverk

**Frontend (Webbsida):**

- Angular 19
- TypeScript
- Azure Entra ID för inloggning
- Tailwind CSS (v.^3.3.5)
- ngx-toastr (v.^19.1.0)

**Backend (Server):**

- .NET 9.0
- ASP.NET Core API
- Entity Framework

**Databas**

- EF Core In-Memory Database (för tillfället)

**Realtidskommunikation:**

- SignalR (frontend och backend)

## Kom igång

### Entra ID

För att logga in, oavsett om det är lokalt eller i produktionsmiljön, behöver du bli tillagd i Innovia Hubs Entra ID-katalog. Kontakta någon i teamet för att bli tillagd.

### Vad du behöver installerat

- Node.js (version 18 eller senare)
- .NET 9.0 SDK
- Git

### Starta projektet

1. **Klona projektet:**

   ```bash
   git clone https://github.com/Ksakiko/InnoviaHub_fork.git
   cd InnoviaHub_fork
   ```

2. **Skapa en `.env`-fil i mappen `backend` för AI-funktionen**

   Lägg till följande med en giltig API-nyckel från OpenAI i filen

   ```bash
   OPENAI_API_KEY="API KEY FROM OPENAI"
   ```

3. **Starta backend (API):**

   ```bash
   cd backend
   dotnet run
   ```

   Servern startar på: <http://localhost:5184>

4. **Starta frontend (webbsida):**

   Öppna en separat terminal från backend. Kontrollera att du befinner dig i projektmappen InnoviaHub_fork och kör sedan:

   ```bash
   cd frontend
   npm install
   ng serve
   ```

   Webbsidan öppnas på: <http://localhost:4200>

## Hur man använder systemet

1. Öppna webbläsaren och gå till <http://localhost:4200>
2. Klicka **Login with Microsoft** för att logga in
3. Gå till bokningssidan genom att klicka på **Boka-ikonen** i menyn
4. Välj en resurstyp från flikarna längst upp på sidan
5. Välj ett datum med datumväljaren
6. Välj en specifik resurs från listan till höger på sidan
7. Boka resurser genom att klicka på **Boka-knappen** längst ner i resurslistan
8. Kolla bokade tider och resurser på **Profilsidan**
9. **Logga ut** när du är klar

## Utveckling

### Mappar

- `backend/` - Server-kod (.NET)
- `frontend/` - Webbsida-kod (Angular)
- `README.md` - Den här filen

### Brancher

- `main` - Huvudbranch (stabil kod)
- `develop` - Utvecklingsbranch

### Testning

Det finns en debug-sida på <http://localhost:4200/azure-debug> för att testa Azure-inloggning.

## Problem?

Om något inte fungerar:

1. Kontrollera att Node.js och .NET är installerat
2. Kör `npm install` i frontend-mappen
3. Kör `dotnet restore` i backend-mappen
4. Starta om både frontend och backend

## Bidra till projektet

1. Skapa en ny branch: `git checkout -b min-nya-feature`
2. Gör dina ändringar
3. Committa: `git commit -m "Lägg till min nya feature"`
4. Pusha: `git push origin min-nya-feature`
5. Skapa en Pull Request

## IoT-integration med [forkat repo](https://github.com/Ksakiko/innovia-iot) (Individuellt arbete)

På adminsidan har ett sensortelemetri-system implementerats (under fliken "Alla sensorer") med [ett API](https://github.com/Ksakiko/innovia-iot) som genererar påhittade mätvärden för demonstrationssyfte. Nuvarande tillgängliga mätvärden är:

- Temperatur
- CO₂

### SignalR

För att åstadkomma överföring av realtidsdata används **SignalR**. När användare besöker sidan "Alla sensorer" så startas SignalR-koppling och stängs av när användare lämnar sektionen.

### ngx-toastr

När SignalR är igång skickas alert-data från API:et för överskridna mätvärden baserat på registrerade regler för varje sensorenhet, och visas som aviseringar. Alert-aviseringar i användargränssnittet är implementerade med hjälp av npm-paketet **ngx-toastr**.

## AI-integration (Individuellt arbete)

På profilsidan finns ett chattfält för att boka resurser med hjälp av AI (OpenAIs API, GPT-4.1).

Användarens inmatning skickas till backenden för att hanteras av API:et. Sedan returneras data från GPT-4.1 till frontenden som AI:s svar.

Följande funktioner är tillgängliga i chatten:

- Boka en resurs på ett specifikt datum
- Få information om tillgängliga resurser baserat på ett specifikt datum
- Ändra resurs eller datum under bokningsprocessen

---

**Skapad av InnoviaHub-teamet** 🚀
