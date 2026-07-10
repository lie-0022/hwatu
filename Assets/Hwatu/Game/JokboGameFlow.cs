using System;
using UnityEngine;
using Hwatu.Core.Jokbo;
using Hwatu.Core.Run;

namespace Hwatu.Game
{
    /// <summary>
    /// 족보 런의 MonoBehaviour 래퍼 — POCO <see cref="JokboRunFlow"/>를 구동하고 페이즈 전환을 방송한다.
    /// 화면(RunUI·JokboCombatView)이 <see cref="OnPhaseChanged"/>를 구독해 자기 화면을 켜고 끈다.
    /// 기존 GameFlow의 족보판. 실제 UI 배선은 후속(JokboRunUI)에서 이 래퍼를 참조한다.
    /// </summary>
    public sealed class JokboGameFlow : MonoBehaviour
    {
        [SerializeField] private ulong _seed = 20260710UL;
        [SerializeField] private int _startHp = 70;

        /// <summary>런 상태머신(로직).</summary>
        public JokboRunFlow Flow { get; } = new JokboRunFlow();

        /// <summary>페이즈 전환 시 발생(현재 페이즈 전달).</summary>
        public event Action<JokboRunPhase> OnPhaseChanged;

        private void Start()
        {
            NewRun();
        }

        public void NewRun()
        {
            Flow.StartNewRun(_seed, _startHp);
            Notify();
        }

        public void EnterNode(MapNode node)
        {
            Flow.EnterNode(node);
            Notify();
        }

        public void OnCombatEnded(bool won)
        {
            Flow.OnCombatEnded(won);
            Notify();
        }

        public void TakeReward(int index)
        {
            Flow.TakeReward(index);
            Notify();
        }

        public void OnRest(int choice)
        {
            Flow.OnRest(choice);
            Notify();
        }

        public void LeaveShop()
        {
            Flow.LeaveShop();
            Notify();
        }

        private void Notify()
        {
            OnPhaseChanged?.Invoke(Flow.Phase);
        }
    }
}
