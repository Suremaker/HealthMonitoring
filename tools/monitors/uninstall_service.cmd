@echo off
"%~dp0\nssm.exe" stop HealthMonitoring.Monitors
"%~dp0\nssm.exe" remove HealthMonitoring.Monitors confirm