using System.Collections.Generic;
using Hwatu.Core.Cards;
using Hwatu.Core.Rng;

namespace Hwatu.Core.Combat
{
    /// <summary>
    /// 더미(draw/discard/exhaust) 드로우·이동 로직. 엔진 상태 전체가 아니라 필요한 리스트만 받아
    /// 순수하게 동작한다(헤드리스 테스트 용이). 컨벤션: 리스트의 끝(last) = 더미의 top.
    /// </summary>
    public static class PileSystem
    {
        /// <summary>
        /// drawPile의 top(끝)에서 n장을 hand로 옮긴다. drawPile이 비면 discardPile을 셔플해 재생성한다.
        /// 둘 다 비면 거기서 멈춘다(있는 만큼만, 카드 분실 없음).
        /// </summary>
        public static void Draw(
            List<CardInstance> hand,
            List<CardInstance> drawPile,
            List<CardInstance> discardPile,
            IRandom shuffleRng,
            int n)
        {
            for (int i = 0; i < n; i++)
            {
                if (drawPile.Count == 0)
                {
                    if (discardPile.Count == 0)
                    {
                        return; // 더 뽑을 카드가 없다
                    }

                    drawPile.AddRange(discardPile);
                    discardPile.Clear();
                    shuffleRng.Shuffle(drawPile);
                }

                int top = drawPile.Count - 1;
                hand.Add(drawPile[top]);
                drawPile.RemoveAt(top);
            }
        }
    }
}
