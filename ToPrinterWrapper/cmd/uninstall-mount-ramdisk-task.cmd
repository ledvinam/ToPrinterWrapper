@echo off
REM Uninstalls the Task Scheduler task for mounting the RAM disk at startup

set TASK_NAME=MountRAMDisk

schtasks /Delete /F /TN "%TASK_NAME%"

echo Task '%TASK_NAME%' deleted (if it existed).
pause
