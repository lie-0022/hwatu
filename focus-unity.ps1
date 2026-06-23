# focus-unity.ps1
# 무인 자율 작업 중 Unity 에디터가 비포커스되어 도메인 리로드(컴파일)가 멈추는 것을 막는다.
# Unity 창을 15초마다 전면 포커스로 끌어올린다 (AppActivate — 1회 테스트로 검증됨).
#
# 실행(무인 작업 시작 전):
#   powershell -ExecutionPolicy Bypass -File "C:\Users\jayju\project\화투\focus-unity.ps1"
# 멈추기: 이 PowerShell 창을 닫거나 Ctrl+C.
# 주의: 실행 중에는 Unity가 15초마다 맨 앞으로 올라온다 → 다른 작업을 할 때는 끄세요(무인 전용).

$wsh = New-Object -ComObject WScript.Shell
Write-Host "Unity 포커스 유지 시작 — 15초 간격. 멈추려면 이 창을 닫거나 Ctrl+C." -ForegroundColor Cyan
while ($true) {
    $p = Get-Process Unity -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowHandle -ne 0 } | Select-Object -First 1
    if ($p) {
        $r = $wsh.AppActivate($p.Id)
        Write-Host ("{0:HH:mm:ss}  Unity 포커스 (PID {1}, {2})" -f (Get-Date), $p.Id, $r)
    } else {
        Write-Host ("{0:HH:mm:ss}  Unity 프로세스 없음 — 에디터가 켜져 있는지 확인" -f (Get-Date)) -ForegroundColor Yellow
    }
    Start-Sleep -Seconds 15
}
