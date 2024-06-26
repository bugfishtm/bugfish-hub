@echo off
setlocal

:: Define the URL to fetch the serialized array from
set "URL=https://software.bugfish.eu/_store/_hub/"

:: Define the temp folder for operations
set "TEMP_DIR=%~dp0_temp"
set "OUTPUT_FILE=%TEMP_DIR%\update.zip"

:: Temp Directory Create
:: Ensure the temp directory exists
if not exist "%TEMP_DIR%" mkdir "%TEMP_DIR%"

:: Fetch the content from the URL and store it in a variable
:: Show Error if Update URL is not Available
echo bugfishHUB Updater
echo(
echo ---------------------------------------------
curl -s "%URL%" > "%TEMP_DIR%\response.txt"
if %errorlevel% neq 0 (
    echo The update server is not available!
	echo ---------------------------------------------
	IF EXIST "%TEMP_DIR%\response.txt" (
		DEL "%TEMP_DIR%\response.txt"
	) 
	IF EXIST  "%TEMP_DIR%" RMDIR /S /Q "%TEMP_DIR%"
	echo(
    pause
    exit /b
)

:: Read the content of the response
setlocal enabledelayedexpansion
set "content="
for /f "delims=" %%i in (%TEMP_DIR%\response.txt) do set "content=!content!%%i"
if not defined content (
    echo The update server is not available!
	echo ---------------------------------------------
	IF EXIST "%TEMP_DIR%\response.txt" (
		DEL "%TEMP_DIR%\response.txt"
	) 
	IF EXIST  "%TEMP_DIR%" RMDIR /S /Q "%TEMP_DIR%"
	echo(
    pause
    exit /b
)
pause

:: Deserialize the array (this assumes a simple comma-separated value format for demonstration)
echo Deserializing the array
set "first_entry="
set "delim=,"
for %%i in (!content:%delim%= !) do (
    if not defined first_entry (
        set "first_entry=%%i"
    )
)
if not defined first_entry (
    echo No Update Available on URL
    pause
    exit /b
)
echo First entry: !first_entry!
pause

:: Check if the first entry is available
if not defined first_entry (
    echo No Update Available on URL
    pause
    exit /b
)

:: Download the update
echo Downloading update from: %first_entry%
curl -o "%OUTPUT_FILE%" "%first_entry%"
if %errorlevel% neq 0 (
    echo Failed to download the update
    pause
    exit /b
)
if not exist "%OUTPUT_FILE%" (
    echo Failed to download the update (file not found)
    pause
    exit /b
)
pause

:: Unpack the downloaded zip file
echo Unpacking the downloaded zip file
"%ProgramFiles%\7-Zip\7z.exe" x "%OUTPUT_FILE%" -o"%TEMP_DIR%"
if %errorlevel% neq 0 (
    echo Failed to unpack the update
    pause
    exit /b
)
pause

:: Delete specific files in the current directory (modify as needed)
echo Deleting specific files in the current directory
del /f /q "%~dp0specificfile1.ext"
del /f /q "%~dp0specificfile2.ext"
pause

:: Move unpacked files to the current directory
echo Moving unpacked files to the current directory
xcopy /s /e /y "%TEMP_DIR%\*" "%~dp0"
if %errorlevel% neq 0 (
    echo Failed to move unpacked files
    pause
    exit /b
)
pause

:: Cleanup
echo Cleaning up
rd /s /q "%TEMP_DIR%"

echo Update completed successfully

endlocal
pause
pause
