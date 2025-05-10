@echo off
REM Installs a Task Scheduler task to mount the RAM disk at startup (requires admin)
REM Error handling and logging for maintainability

setlocal
set SCRIPT_PATH=%~dp0mount-ramdisk.cmd
set TASK_NAME=MountRAMDisk

REM Check if the script exists
if not exist "%SCRIPT_PATH%" (
    echo ERROR: mount-ramdisk.cmd not found at %SCRIPT_PATH%.
    exit /b 1
)

REM Create the scheduled task to run at startup with highest privileges
schtasks /Create /F /TN "%TASK_NAME%" /TR "\"%SCRIPT_PATH%\"" /SC ONSTART /RL HIGHEST /RU "SYSTEM"
if errorlevel 1 (
    echo ERROR: Failed to create scheduled task '%TASK_NAME%'.
    exit /b 1
)

echo Task '%TASK_NAME%' created to run mount-ramdisk.cmd at startup with admin rights.
pause
endlocal
