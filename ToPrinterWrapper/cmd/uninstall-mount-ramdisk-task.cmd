@echo off
REM Uninstalls the Task Scheduler task for mounting the RAM disk at startup
REM Error handling and logging for maintainability

setlocal
set TASK_NAME=MountRAMDisk

REM Try to delete the scheduled task
schtasks /Delete /F /TN "%TASK_NAME%"
if errorlevel 1 (
    echo ERROR: Failed to delete scheduled task '%TASK_NAME%'. It may not exist.
    exit /b 1
)

echo Task '%TASK_NAME%' deleted (if it existed).
pause
endlocal
