@ECHO OFF

SET VERSION=1.14.0.0

dotnet clean --configuration Release || GOTO :error
dotnet build --configuration Release /p:Version=%VERSION%^
    --no-incremental || GOTO :error
dotnet test --configuration Release || GOTO :error
rmdir /s /q Wautoma\bin\Release\net48 || GOTO :error
dotnet publish Wautoma\Wautoma.fsproj --configuration Release || GOTO :error

ECHO.
ECHO.
ECHO BUILD SUCCESSFUL
GOTO :EOF

:error
ECHO.
ECHO.
ECHO BUILD FAILED
ECHO Failed with error #%errorlevel%.
EXIT /b %errorlevel%
