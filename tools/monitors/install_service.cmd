@echo off
"%~dp0\nssm.exe" install HealthMonitoring.Monitors "%~dp0\HealthMonitoring.Monitors.SelfHost.exe"
if %ERRORLEVEL% GEQ 1 EXIT /B %ERRORLEVEL%
"%~dp0\nssm.exe" start HealthMonitoring.Monitors
if %ERRORLEVEL% GEQ 1 EXIT /B %ERRORLEVEL%