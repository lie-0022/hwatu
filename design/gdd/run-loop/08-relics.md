# 08. 유물 (Relics)

## 1. Overview
유물 = 패시브 효과(MVP는 전투 시작 훅). 보물/엘리트/보스 보상으로 획득.

## 3. Detailed Rules
- `RelicData`(전투 시작 `Action<PlayerState>`). `RunState.Relics`. `CombatFactory.CreateCombat`가 전투 시작 시 `ApplyCombatStart` 적용.
- 8종: 방석(광+1)·담요(방+5)·등잔(시작 상징)·엽전꾸러미(골드, 후속)·숫돌(광+1) · 부적(방+8)·룬돌(광+2)·강철비늘(민첩+1).
- 시작 유물: 등잔.

## 6. Dependencies
`RelicData`·`RelicContent`·`RunState.Relics`·`CombatFactory.CreateCombat(relics)`.

## 8. Acceptance Criteria
- [x] 유물 전투 시작 효과(방석 광+1 / 담요 방+5) — `RelicTests` 4종 통과. 시작 등잔 보유.

## 후속
훅 확장(턴 시작/처치/피격) · 보상 유물 UI(C3, 보물·엘리트·보스) · 엽전 골드 처리 · 숫돌 '첫 공격 +3'.
