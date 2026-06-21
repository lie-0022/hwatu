using System;
using UnityEngine;
using Hwatu.Core.Rng;
using Hwatu.Core.Run;

namespace Hwatu.Game
{
    /// <summary>런 진행 페이즈(화면).</summary>
    public enum RunPhase
    {
        MainMenu,
        CharacterSelect,
        Map,
        Combat,
        Reward,
        BossReward,
        GameOver,
        Victory
    }

    /// <summary>
    /// 한 판 루프의 중추 상태머신. RunState를 보유하고 페이즈 전환을 관리한다(02-run-state.md).
    /// 화면 View들은 <see cref="OnPhaseChanged"/>를 구독해 자기 화면을 켜고 끈다.
    /// </summary>
    public sealed class GameFlow : MonoBehaviour
    {
        [SerializeField] private ulong _seed = 20260622UL;

        private readonly MapGenerator _mapGen = new MapGenerator();

        public RunState Run { get; private set; }
        public RunPhase Phase { get; private set; }

        /// <summary>페이즈 전환 시 발생(현재 페이즈 전달).</summary>
        public event Action<RunPhase> OnPhaseChanged;

        private void Start()
        {
            SetPhase(RunPhase.MainMenu);
        }

        /// <summary>메인 메뉴에서 캐릭터 선택으로.</summary>
        public void GoToCharacterSelect()
        {
            SetPhase(RunPhase.CharacterSelect);
        }

        /// <summary>캐릭터를 골라 새 런 시작 → 1막 맵 생성 → 맵 화면.</summary>
        public void StartNewRun(CharacterData character)
        {
            Run = new RunState(character, _seed);
            GenerateActMap();
            SetPhase(RunPhase.Map);
        }

        /// <summary>현재 액트의 맵을 생성한다(맵 전용 RNG 스트림).</summary>
        public void GenerateActMap()
        {
            IRandom rng = new RngStreams(Run.Seed).ForStream("map_act" + Run.Act);
            Run.Map = _mapGen.Generate(rng, Run.Act);
            Run.CurrentNodeId = -1;
        }

        /// <summary>맵에서 노드를 선택해 진입.</summary>
        public void EnterNode(MapNode node)
        {
            Run.CurrentNodeId = node.Id;
            switch (node.Type)
            {
                case NodeType.Combat:
                case NodeType.Elite:
                case NodeType.Boss:
                    SetPhase(RunPhase.Combat);
                    break;
                default:
                    // 휴식/상점/이벤트/보물 — MVP는 stub(즉시 맵 복귀). 07에서 구현.
                    SetPhase(RunPhase.Map);
                    break;
            }
        }

        /// <summary>전투 종료 콜백(승/패).</summary>
        public void OnCombatEnded(bool won)
        {
            if (!won)
            {
                SetPhase(RunPhase.GameOver);
                return;
            }
            MapNode node = Run.Map.GetNode(Run.CurrentNodeId);
            if (node != null && node.Type == NodeType.Boss)
            {
                SetPhase(RunPhase.Victory);
                return;
            }
            SetPhase(RunPhase.Reward);
        }

        /// <summary>보상 화면을 마치고 맵으로 복귀.</summary>
        public void OnRewardDone()
        {
            SetPhase(RunPhase.Map);
        }

        /// <summary>현재 노드가 보스인가(전투 화면이 보스 적을 쓰도록).</summary>
        public bool CurrentNodeIsBoss()
        {
            MapNode node = Run.Map?.GetNode(Run.CurrentNodeId);
            return node != null && node.Type == NodeType.Boss;
        }

        private void SetPhase(RunPhase phase)
        {
            Phase = phase;
            OnPhaseChanged?.Invoke(phase);
        }
    }
}
