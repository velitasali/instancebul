@echo off
echo Building Instancebul...
echo.

echo [1/2] Building small version (169KB, requires .NET runtime)...
dotnet publish -c Release --no-self-contained -o bin/Release/net8.0-windows/publish-small
if %ERRORLEVEL% NEQ 0 (
    echo Failed to build small version
    exit /b 1
)
copy config.json bin\Release\net8.0-windows\publish-small\ >nul 2>&1
copy app.ico bin\Release\net8.0-windows\publish-small\ >nul 2>&1

echo [2/2] Building self-contained version (147MB, no runtime needed)...
dotnet publish -c Release
if %ERRORLEVEL% NEQ 0 (
    echo Failed to build self-contained version
    exit /b 1
)
copy config.json bin\Release\net8.0-windows\win-x64\publish\ >nul 2>&1
copy app.ico bin\Release\net8.0-windows\win-x64\publish\ >nul 2>&1

echo.
echo ====================================
echo Build complete!
echo ====================================
echo.
echo Small build (169KB):
echo   bin\Release\net8.0-windows\publish-small\Instancebul.exe
echo.
echo Self-contained build (147MB):
echo   bin\Release\net8.0-windows\win-x64\publish\Instancebul.exe
echo.
