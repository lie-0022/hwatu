# 01. 전투 시스템

> 헤드리스 `Hwatu.Core.Combat` 순수 POCO(엔진 비의존). 설계 상세: `design/gdd/run-loop/15-status·18-keywords·13-boss-phase`.

## 현황

- **상태머신** `CombatPhase`: PlayerTurnStart → PlayerAction(입력 대기) → EnemyTurn → CheckDeath → … `CombatEngine.Advance()`가 자동 phase를 소화하고, PlayerAction에서만 멈춰 `PlayCard(handIdx, target)` / `EndTurn()` 입력을 받는다.
- **효과 op 6**: DealDamage · GainBlock · Draw · ApplyStatus · GainResource · ClearStatus (`EffectDispatcher` switch).
- **키워드 4**: Exhaust(사용 시 소멸) · Retain(턴 끝 유지) · Innate(첫 손패 보장) · Ethereal(미사용 시 소멸).
- **status 5**: Radiance(광=공격력) · Weak(가하는 피해 ×3/4) · Vulnerable(받는 피해 ×3/2) · Poison(턴틱) · Dexterity(방어 +). 전부 정수 결정론.
- **데미지**: `(기본 + Radiance) ×Vulnerable − Block`, Weak는 공격자 측 ×3/4.
- **적 AI**: SequenceAi(고정 순서) · PhaseAi(HP 임계 광폭 전환) · Doom(예고 N턴 → 발동). intent 완전정보(`PeekNext`).
- **포션**: `CombatEngine.UsePotion(PotionData)` 즉시 효과.

## STS 비교

| STS | 화투 | 비고 |
|-----|------|------|
| 에너지/드로우/Block | 3 / 5 / 턴 리셋 | ✅ |
| 힘/민첩 | Radiance / Dexterity | ✅ (광=힘 통합) |
| 약화/취약/중독 | Weak / Vulnerable / Poison | ✅ |
| 소멸/휘발/타고남/유지 | Exhaust / Ethereal / Innate / Retain | ✅ |
| intent 완전정보 | PeekNext | ✅ |
| 보스 페이즈 | PhaseAi + Doom | ✅ (Doom 텔레그래프는 화투 고유) |

## 갭 / 계획

- status·키워드가 **UI에서 의미 불명**(수치/라벨만) → [04-ui-ux](04-ui-ux.md) P1 hover 사전.
- 단일 적 전제(target=0 고정) — 멀티 적 전투/카드 타깃 선택은 미구현(보류).
- 검증: EditMode 전투 테스트 다수 + 헤드리스 SimAi 자동 완주.
