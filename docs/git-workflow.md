# Git 협업 워크플로

> 1인 + AI 협업(B타입)이지만, 외부 협업자 합류를 전제로 한 브랜치·커밋 전략.

## 브랜치 전략
- **`main`**: 항상 빌드·플레이 가능한 안정 브랜치. 직접 푸시 지양 → PR로 머지.
- **`feature/<기능>`**: 기능 개발. 예) `feature/run-loop`, `feature/card-reward`, `feature/map`.
- **`fix/<버그>`**: 버그 수정.
- **`docs/<주제>`**: 문서 단독 변경(선택).

브랜치는 항상 `main`에서 분기. 작은 단위로 짧게 유지(장기 브랜치 머지 충돌 회피).

## 커밋 규칙
- **의미 단위** 커밋. 형식: `<type>: <요약>`
  - `feat` 기능 / `fix` 버그 / `docs` 문서 / `refactor` 리팩토링 / `test` 테스트 / `chore` 잡무
- 본문에 관련 설계 문서/이슈를 참조.
- **auto checkpoint 처리**: 작업 중 `auto: checkpoint`(Stop 훅 안전망)가 쌓이면, 푸시/머지 전 반드시
  `git reset --soft <마지막 의미 커밋>` 후 의미 메시지 하나로 재커밋해 **묶는다**. `auto:` 메시지는 히스토리에 남기지 않는다.
- 커밋 푸시는 사용자 지시 시.

## 플로우
1. `main`에서 `feature/<기능>` 분기.
2. 기능 개발 + 커밋(작업 중 auto는 squash로 정리).
3. 푸시 → PR 생성 → 리뷰 → `main` 머지(**squash merge 권장** — 깔끔한 히스토리).
4. `main`은 항상 green: EditMode 테스트 통과 + SampleScene 플레이 가능.

## Unity 협업 주의
- **씬/프리팹 동시 편집 금지**(YAML 머지 충돌). 기능은 프리팹으로 분리.
- **`.meta` 항상 커밋**. Asset Serialization = Force Text(Project Settings > Editor).
- 대용량 바이너리(폰트·텍스처·오디오)는 향후 **Git LFS** 도입 검토(현재 `malgun.ttf` 13MB 일반 커밋 상태).
- (권장) `.gitattributes`로 씬/프리팹 **UnityYAMLMerge(Smart Merge)** 설정.

## 현재 작업 브랜치
- `feature/run-loop`: 한 판 루프(맵·전투·보상·덱빌딩·보스) 구현. 설계 문서는 `docs/design/run-loop/`.
