# 05. 밸런스 · 시뮬

> `design/gdd/run-loop/12-balance §9`. SimAi/BalanceSim 헤드리스(Play 무관).

## 도구

- **SimAi** (`Core/Content/SimAi.cs`): 휴리스틱 자동 플레이어 — 에너지로 낼 수 있는 카드를 공격 우선 소진 후 EndTurn. `RunCombat(engine, turnCap)` → CombatResult.
- **BalanceSim** (`Core/Content/BalanceSim.cs`): `Simulate(deck, enemy, maxHp, trials)` → 시드 1..N 자동 대전 집계 `Matchup{WinRatePct, AvgTurns, AvgHpLeftOnWin}`. 결정론 시드라 재현 가능.

## 측정 결과 (시작덱 × 적, 각 10회 · 2026-06-22)

| 적 | 광객(HP80) | 묵귀(HP70) |
|---|---|---|
| 잡도깨비(일반) | 100% · 1t · 잔78 | 100% · 1t · 잔69 |
| 멧돼지(일반) | 100% · 2t · 잔80 | 100% · 2t · 잔70 |
| 두꺼비(일반) | 100% · 3t · 잔75 | 100% · 3t · 잔65 |
| 광귀(엘리트) | 100% · 2t · 잔71 | 100% · 2t · 잔61 |
| 구미호(보스) | 100% · 5t · 잔39 | 100% · 5t · 잔30 |

- **해석**: 시작덱이 단일 적엔 전부 승(초반 안전 = 적정). 보스는 HP 약 절반 소모로 긴장. 두꺼비는 독 지구전 3턴.
- **한계**: SimAi가 단순 휴리스틱(공격 우선) = 승률 **하한 추정**. 다적·act2~3 스케일·덱 성장·포션/유물 미반영.

## 계획

- SimAi 고도화(타깃 선택 · 방어 판단 · 포션 사용).
- act2~3 스케일 + 덱 성장(보상 반영) 시뮬 → 진짜 난이도 곡선.
- 승률 극단(>95% 너무 쉬움 / <30% 너무 어려움) 자동 식별 → 튜닝 루프.
