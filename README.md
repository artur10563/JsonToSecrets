# JsonToDockerVars CLI

`JsonToDockerVars` is a console application that extracts variables from JSON files and outputs them in various formats (`docker_string`, `json`, `docker_file`, `koyeb`). This README provides copy-paste instructions to build the project and make it globally accessible from the console on Windows.

---

## Installation and Setup (Windows)

1. Open PowerShell and navigate to the project folder:
```powershell
cd "F:\ASP.NET Projects\Json_To_Docker_Vars\JsonToDockerVars"
```

2. Publish the project as a single-file, self-contained executable:
```powershell
   dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

3. Create a folder for global tools and copy the executable: 
```powershell
mkdir C:\Tools\JsonToDockerVars
copy bin\Release\net9.0\win-x64\publish\JsonToDockerVars.exe C:\Tools\JsonToDockerVars\
```

4. Add the folder to your PATH:
```powershell
setx PATH "$env:PATH;C:\Tools\JsonToDockerVars"
```

5. Run:
```
JsonToSecrets --help"
JsonToSecrets variables --help"
JsonToSecrets sections --help"
```