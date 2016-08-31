@echo off
"%~dp0\nssm.exe" stop HealthMonitoring
"%~dp0\nssm.exe" remove HealthMonitoring confirm