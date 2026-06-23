@echo off
chcp 65001 >nul
title Unity 포커스 유지 (무인 작업용 - 멈추려면 이 창 닫기)
echo.
echo  무인 작업용 Unity 포커스 유지를 시작합니다.
echo  15초마다 Unity 창을 앞으로 올립니다.
echo  멈추려면 이 창을 닫거나 Ctrl+C 를 누르세요.
echo.
powershell -ExecutionPolicy Bypass -File "%~dp0focus-unity.ps1"
pause
