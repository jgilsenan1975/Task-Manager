# Deploying to Azure

This walks through hosting the whole app — Angular frontend, ASP.NET Core API, and SQL Server database — as a single Azure App Service, deployed automatically from GitHub via GitHub Actions.

**Architecture:** one App Service runs the ASP.NET Core API. At publish time, the compiled Angular app is copied into the API's `wwwroot` folder, so the API serves both the SPA's static files and its own `/api/*` endpoints from the same URL. This avoids CORS configuration and gives you one link to share. The database is a separate Azure SQL Database instance the API connects to over a connection string.

You'll need the [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) installed and logged in (`az login`), and this repo already pushed to GitHub (see the main README).

Everywhere below, replace `joe` / `task-manager-joe` with something unique to you — App Service and SQL Server names are globally unique across all of Azure, so `task-manager` alone will almost certainly be taken.

## 1. Create the resource group

Everything you create lands in one resource group, so it's easy to find — or delete — as a unit later.

```bash
az group create --name rg-task-manager --location eastus
```

## 2. Create the App Service plan and web app

```bash
# Basic B1 (~small monthly cost, doesn't sleep). Swap --sku B1 for --sku F1 for the free tier instead.
az appservice plan create \
  --name plan-task-manager \
  --resource-group rg-task-manager \
  --sku B1 \
  --is-linux

az webapp create \
  --name task-manager-joe \
  --resource-group rg-task-manager \
  --plan plan-task-manager \
  --runtime "DOTNETCORE:8.0"
```

Your app will be reachable at `https://task-manager-joe.azurewebsites.net` once deployed.

## 3. Create the SQL Server and free-tier database

```bash
az sql server create \
  --name sql-task-manager-joe \
  --resource-group rg-task-manager \
  --location eastus \
  --admin-user taskadmin \
  --admin-password "<pick-a-strong-password>"

az sql db create \
  --resource-group rg-task-manager \
  --server sql-task-manager-joe \
  --name TaskManagerDb \
  --edition GeneralPurpose \
  --family Gen5 \
  --capacity 1 \
  --compute-model Serverless \
  --use-free-limit \
  --free-limit-exhaustion-behavior AutoPause
```

`--use-free-limit` is what puts this database on Azure SQL's free monthly allowance (100,000 vCore-seconds and 32 GB, indefinitely) instead of billing from the first second. `AutoPause` means if you ever blow through the free allowance in a given month, the database just pauses rather than racking up charges.

By default, no traffic can reach the SQL server — not even Azure's own services. Open the firewall for Azure services (App Service needs this) and, if you want to connect from your own machine with SSMS/Azure Data Studio, your own IP too:

```bash
az sql server firewall-rule create \
  --resource-group rg-task-manager \
  --server sql-task-manager-joe \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Optional: allow your current machine to connect directly
MY_IP=$(curl -s https://api.ipify.org)
az sql server firewall-rule create \
  --resource-group rg-task-manager \
  --server sql-task-manager-joe \
  --name AllowMyIP \
  --start-ip-address $MY_IP \
  --end-ip-address $MY_IP
```

## 4. Wire up the connection string and app settings

The API reads its connection string from configuration key `ConnectionStrings:DefaultConnection` (see `appsettings.json`). Rather than putting the real value in that file, set it as an App Service **connection string** — App Service injects it as an environment variable at runtime, which ASP.NET Core's configuration system automatically maps back onto `ConnectionStrings:DefaultConnection`, overriding whatever's in `appsettings.json`.

```bash
az webapp config connection-string set \
  --resource-group rg-task-manager \
  --name task-manager-joe \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:sql-task-manager-joe.database.windows.net,1433;Database=TaskManagerDb;User ID=taskadmin;Password=<same-password-as-above>;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;"
```

Do the same for the JWT signing key — this is what `appsettings.json` calls `Jwt:Key`, and the double-underscore below (`Jwt__Key`) is how App Service app settings express nested configuration sections:

```bash
# Generate a real random secret rather than reusing the placeholder in appsettings.json
JWT_SECRET=$(openssl rand -base64 48)

az webapp config appsettings set \
  --resource-group rg-task-manager \
  --name task-manager-joe \
  --settings \
    Jwt__Key="$JWT_SECRET" \
    Jwt__Issuer="TaskManager.Api" \
    Jwt__Audience="TaskManager.Client" \
    Jwt__ExpiryMinutes=120 \
    ASPNETCORE_ENVIRONMENT=Production
```

Nothing above ever needs to be committed to git — it lives only in Azure's configuration store.

**About the database schema:** the API calls `db.Database.Migrate()` on startup (see `Program.cs`), so the first time it starts up against the new database, it creates all the tables for you from the EF Core migrations in the repo. That means you need to have generated a migration at least once locally before your first deploy:

```bash
cd backend/TaskManager.Api
dotnet tool install --global dotnet-ef   # first time only
dotnet ef migrations add InitialCreate
```

Commit the generated `Migrations/` folder — it needs to ship with the published app for auto-migration to work.

## 5. Set up GitHub Actions

The workflow file is already in this repo at `.github/workflows/deploy.yml`. Two things to do:

**a. Update the app name.** Open `.github/workflows/deploy.yml` and change this line to match what you created in step 2:

```yaml
env:
  AZURE_WEBAPP_NAME: task-manager-joe
```

**b. Give GitHub permission to deploy.** Download the App Service's publish profile:

```bash
az webapp deployment list-publishing-profiles \
  --name task-manager-joe \
  --resource-group rg-task-manager \
  --xml > publish-profile.xml
```

Open `publish-profile.xml`, copy its entire contents, then in your GitHub repo go to **Settings → Secrets and variables → Actions → New repository secret**, name it `AZURE_WEBAPP_PUBLISH_PROFILE`, and paste the XML as the value. Delete `publish-profile.xml` locally afterward — it's a credential, don't commit it.

Commit and push your changes (the workflow file, the migration you generated, anything else):

```bash
git add .
git commit -m "Add Azure deployment workflow"
git push
```

That push triggers the workflow. Watch it run under your repo's **Actions** tab — it builds the Angular app, publishes the API, copies the Angular build into `wwwroot`, and deploys the result to App Service. Once it's green, visit `https://task-manager-joe.azurewebsites.net` and you should see the app live, backed by the real database.

Every subsequent push to `main` redeploys automatically.

## Troubleshooting

**"Application Error" on first load.** Check **Log stream** in the App Service portal blade (or `az webapp log tail --name task-manager-joe --resource-group rg-task-manager`). The most common cause is the connection string or JWT key not being set correctly — double check step 4.

**Migrations didn't run / tables don't exist.** Confirm the `Migrations/` folder was committed and is present in the published output — `dotnet publish` only includes it if it was part of the project when you built.

**GitHub Actions fails at the deploy step.** Usually a stale or malformed publish profile secret — regenerate it with the command in step 5b and update the GitHub secret.

**Angular routes 404 on refresh (e.g. `/projects/3/board`).** This is what `app.MapFallbackToFile("index.html")` in `Program.cs` handles — if you see this, confirm that line is still present and that the Angular build actually landed in `wwwroot` (check the "Copy Angular build into published wwwroot" step in the Actions log).

## Tearing it down

Since everything lives in one resource group, cleanup is one command:

```bash
az group delete --name rg-task-manager --yes --no-wait
```
