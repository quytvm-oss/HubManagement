# Local secrets

This repository intentionally contains no database, JWT, or SMTP credentials.

Configure development secrets from the repository root:

```powershell
dotnet user-secrets set --project HubManagement.WebApi "PostGreSqlSetting:ConnectionString" "Server=localhost;Port=5432;Database=HubManagement;User Id=postgres;Password=<password>;Minimum Pool Size=5"
dotnet user-secrets set --project HubManagement.WebApi "JwtOptions:SigningKey" "<at-least-32-random-characters>"
dotnet user-secrets set --project HubManagement.WebApi "MailOptions:SMTP:UserName" "<username>"
dotnet user-secrets set --project HubManagement.WebApi "MailOptions:SMTP:Password" "<password>"
```

Use environment variables or a managed secret store in deployed environments. Revoke credentials that were previously committed before using this repository again.
