@echo off
REM Installs a Task Scheduler task to mount the RAM disk at startup (requires admin)

set SCRIPT_PATH=%~dp0mount-ramdisk.cmd
set TASK_NAME=MountRAMDisk

REM Create the scheduled task to run at startup with highest privileges
schtasks /Create /F /TN "%TASK_NAME%" /TR "\"%SCRIPT_PATH%\"" /SC ONSTART /RL HIGHEST /RU "SYSTEM"

echo Task '%TASK_NAME%' created to run mount-ramdisk.cmd at startup with admin rights.
pause
