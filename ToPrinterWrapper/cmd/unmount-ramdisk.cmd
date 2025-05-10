@echo off
REM Unmounts the RAM disk (R:) using OSFMount
REM Error handling and logging for maintainability

setlocal
set OSFMOUNT="C:\Program Files\OSFMount\osfmount.com"
set DRIVE=R:

REM Check if OSFMount exists
if not exist %OSFMOUNT% (
    echo ERROR: OSFMount not found at %OSFMOUNT%.
    exit /b 1
)

REM Try to unmount the RAM disk
%OSFMOUNT% -d -m %DRIVE%
if errorlevel 1 (
    echo ERROR: Failed to unmount RAM disk on %DRIVE%.
    exit /b 1
)

echo RAM disk unmounted from %DRIVE%.
endlocal
