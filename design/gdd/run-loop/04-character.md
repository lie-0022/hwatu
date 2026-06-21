# 04. 캐릭터 (Character)

## 1. Overview
플레이 가능 캐릭터 정의(4축)와 선택 화면. MVP는 광객 1명.

## 2. Player Fantasy
캐릭터마다 다른 시작 덱·자원으로 "다른 게임"을 하는 느낌(STS의 4인처럼).

## 3. Detailed Rules
- 캐릭터 차이 **4축**: 시작 HP(STS 66~80) / 시작 유물 / 시작 덱(10~12장) / 고유 자원·메커니즘. 시작 골드 99·기본 에너지 3은 공통.
- **MVP 광객(Luminary)**: HP 80, 골드 99, 덱 10(빛타격×5/방패×4/점화×1), 고유 자원 `Radiance(광)`.
- 선택 화면: 캐릭터 버튼 → `GameFlow.StartNewRun(CharacterData)`.

## 6. Dependencies
`CharacterData`(Core.Run) · `GameFlow.StartNewRun` · `RunUI` CharPanel.

## 7. Tuning Knobs
시작 HP / 덱 구성 / 골드 / 고유 자원.

## 8. Acceptance Criteria
- [x] 캐릭터 선택 시 그 캐릭터 시작 상태(HP·덱)로 런 시작. (현재 광객 1, 컴파일 + 흐름 검증)

## 후속
캐릭터 2~5(타짜·피바라기 등 SPEC §6.2) · 시작 유물 · 고유 자원 메커니즘(화투 패 활용) · `CharacterData`를 ScriptableObject로 외부화.
