@echo off
"%~dp0\nssm.exe" install HealthMonitoring "%~dp0\HealthMonitoring.SelfHost.exe"
if %ERRORLEVEL% GEQ 1 EXIT /B %ERRORLEVEL%
"%~dp0\nssm.exe" start HealthMonitoring
if %ERRORLEVEL% GEQ 1 EXIT /B %ERRORLEVEL%