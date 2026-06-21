# 화투 (Hwatu) — 로그라이크 덱빌더

화투(花鬪) 소재의 싱글플레이 로그라이크 덱빌더. *Slay the Spire 2* 스타일의 턴제 카드 전투를 화투 테마로 재해석합니다. PC(Windows) 대상.

## 기술 스택
- **엔진**: Unity 6.3 LTS (`6000.3.10f1`)
- **언어**: C# (.NET Standard 2.1)
- **렌더**: Universal RP — 2D Renderer
- **입력**: Unity Input System (New)

## 프로젝트 구조
```
Assets/Hwatu/
  Core/    # 엔진 비의존 게임 로직 (순수 POCO, 헤드리스 테스트 가능)
    Combat/ Effects/ Cards/ Enemies/ Rng/ Content/
  Game/    # uGUI 전투 UI (CombatView · CardView · EnemyView · TargetingArrow …)
  Tests/   # EditMode NUnit 테스트
docs/      # 기획·룰 문서 (SPEC, 화투 룰 정리)
```
- `Hwatu.Core`는 `noEngineReferences:true` — 엔진/UI를 모르는 순수 C#. 덕분에 **헤드리스 테스트·시드 재현**이 가능합니다.

## 실행
1. Unity **6000.3.10f1**(Unity 6.3)로 이 프로젝트를 엽니다.
2. `Assets/Scenes/SampleScene.unity`를 열고 **Play**.
3. 손패 카드를 **드래그**해 사용 — 공격은 **적에 조준**, 방어는 **위로**. 드래그 중 **우클릭으로 취소**.

## 테스트
- Unity: `Window > General > Test Runner > EditMode > Run All`
- Core는 순수 C#이라 도메인 리로드만으로 실행됩니다.

## 현재 진행
**단일 전투 한 판이 완성** — 드래그 타깃팅 + HP 바/intent 아이콘/데미지 팝업 + 모션 polish (M1 코어 EditMode 테스트 통과).
- ✅ 전투 코어 · 전투 UI · 비주얼 · 모션
- ⏭ 다음: 카드 풀 확장(현재 3장) · 노드 맵·카드 보상 · 적 3종+엘리트+보스
- 상세 로드맵: [`docs/화투_로그라이크_SPEC.md`](docs/화투_로그라이크_SPEC.md) §9·§11

## 문서
- [게임 기획·구현 스펙](docs/화투_로그라이크_SPEC.md)
- [화투 룰 정리](docs/화투_룰_정리.md)

## 라이선스
TBD (협업 합류 시 합의)
