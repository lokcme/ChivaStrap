@echo off
title Crear ChivasStrap EXE
cd /d "%~dp0"
echo ============================================
echo  Creando ChivasStrap EXE...
echo ============================================
powershell -ExecutionPolicy Bypass -File "publish-all.ps1" -Only windows -NoPause
echo.
echo ============================================
echo  Listo: PublishedBuilds\Windows\Chivas.exe
echo ============================================
pause
