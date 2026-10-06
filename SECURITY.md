# Local secrets

This repository intentionally contains no database or SMTP credentials. Browser authentication uses a protected HttpOnly BFF session cookie rather than a frontend-managed JWT.

Configure development secrets from the repository root:

```powershell
dotnet user-secrets set --project HubManagement.WebApi "PostGreSqlSetting:ConnectionString" "Server=localhost;Port=5432;Database=HubManagement;User Id=postgres;Password=<password>;Minimum Pool Size=5"
dotnet user-secrets set --project HubManagement.WebApi "MailOptions:SMTP:UserName" "<username>"
dotnet user-secrets set --project HubManagement.WebApi "MailOptions:SMTP:Password" "<password>"
```

Use environment variables or a managed secret store in deployed environments. Revoke credentials that were previously committed before using this repository again.
