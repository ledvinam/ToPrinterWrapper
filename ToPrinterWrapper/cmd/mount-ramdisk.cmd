@echo off
REM Mounts the RAM disk (R:) using OSFMount
REM Error handling and logging for maintainability

setlocal
set SCRIPT_DIR=%~dp0
set OSFMOUNT="C:\Program Files\OSFMount\osfmount.com"
set DRIVE=R:
set SIZE=1G
set LABEL=PRINT

REM Check if OSFMount exists
if not exist %OSFMOUNT% (
    echo ERROR: OSFMount not found at %OSFMOUNT%.
    exit /b 1
)

REM Try to mount the RAM disk
%OSFMOUNT% -a -t vm -m %DRIVE% -o format:ntfs:"%LABEL%" -s %SIZE%
if errorlevel 1 (
    echo ERROR: Failed to mount RAM disk on %DRIVE%.
    exit /b 1
)

echo RAM disk mounted on %DRIVE% with label "%LABEL%".
endlocal
