# Claude Code Game Studios -- Game Studio Agent Architecture

Indie game development managed through 47 coordinated Claude Code subagents.
Each agent owns a specific domain, enforcing separation of concerns and quality.

**게임 컨셉**: 화투(花鬪/Hwatu) 소재 기반 싱글플레이 로그라이크 — 플레이어가 몬스터를 처치하며 진행. 플랫폼 PC/Windows. 세부 기획(화투 패 활용·룰·점수·메타)은 추후 확정.

## Technology Stack

- **Engine**: Unity 6.3 LTS (6000.3.x)
- **Language**: C#
- **Version Control**: Git with trunk-based development
- **Build System**: Unity Build Pipeline (Build Profiles)
- **Asset Pipeline**: Unity Asset Import Pipeline + Addressables

> **Note**: This project uses **Unity 6.3 LTS** exclusively.
> Unity-specific agents (`unity-csharp`, `unity-architect`, `unity-debugger`) are active.
> Godot agents are disabled in `.claude/agents/_disabled/`. Unreal agents remain
> available but should not be invoked unless the project pivots.

## Project Path

- **Unity Project Root**: `./` (화투 / Hwatu — Unity project at repo root)
- **Unity Version**: 6000.3.10f1 (Unity 6.3)
- **Render Pipeline**: Universal Render Pipeline (URP) — 2D Renderer

## Project Structure

@.claude/docs/directory-structure.md

## Engine Version Reference

<!-- 엔진 API 레퍼런스 문서는 아직 미생성. 필요 시 `/setup-engine`으로 생성한다. -->

## Technical Preferences

@.claude/docs/technical-preferences.md

## Coordination Rules

@.claude/docs/coordination-rules.md

## Collaboration Protocol

**User-driven collaboration, not autonomous execution.**
Every task follows: **Question -> Options -> Decision -> Draft -> Approval**

- Agents MUST ask "May I write this to [filepath]?" before using Write/Edit tools
- Agents MUST show drafts or summaries before requesting approval
- Multi-file changes require explicit approval for the full changeset
- No commits without user instruction

협업 프로토콜 상세는 아래 Workflow 섹션의 `workflow-b-type.md`를 참조한다.

> **First session?** If the project has no engine configured and no game concept,
> run `/start` to begin the guided onboarding flow.

## Workflow

**B타입 워크플로우 (2026-04-26~)**: 1인 개발자(PM/결정자) + AI(풀스택 개발자) 협업 모델.
Sprint 시스템 폐지, continuous flow.
**2026-05-08**: Unity MCP 도입으로 AI가 Scene/GameObject/컴포넌트/프리팹/머티리얼/Build Settings 등을 직접 처리. Editor handoff는 외부 자산·체감 테스트·미세 시각 튜닝에 한정.

@.claude/docs/workflow-b-type.md

@.claude/docs/mcp-capabilities.md

@.claude/docs/editor-handoff.md

@.claude/docs/review-workflow.md


## Naming Conventions

@.claude/docs/naming-conventions.md

## Coding Standards

@.claude/docs/coding-standards.md

## Development Conventions

게임 개발 기초 규약(C# 컨벤션, Unity 스크립트 모범사례, 에셋/모델링 파이프라인,
씬/프리팹, Git, 협업). 협업·유지보수의 기준이며 `naming-conventions.md`·
`technical-preferences.md`와 충돌 시 기존 규약 우선.

@.claude/docs/dev-conventions.md

## Context Management

@.claude/docs/context-management.md
