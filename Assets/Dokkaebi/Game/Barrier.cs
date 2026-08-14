using UnityEngine;

namespace Dokkaebi.Game
{
    /// <summary>
    /// 통로를 막는 영불 장벽. 문짝이 아니라 "넓은 통로 + 결계"라서
    /// 플레이어는 나갈 길이 어디인지 처음부터 보고, 적을 다 잡아야 통과할 수 있다.
    ///
    /// 닫혀 있는 동안 은은하게 밝기가 흔들려 살아 있는 결계처럼 보이게 한다.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Barrier : MonoBehaviour
    {
        /// <summary>닫혔을 때 길을 막는 콜라이더. 비워두면 같은 오브젝트에서 찾는다.</summary>
        [SerializeField] private Collider2D _blocker;

        [Header("맥동")]
        [SerializeField] private float _pulseSpeed = 2.4f;
        [SerializeField] private float _pulseMin = 0.55f;
        [SerializeField] private float _pulseMax = 0.85f;

        private SpriteRenderer _sr;
        private Color _baseColor;

        /// <summary>장벽이 열렸는지(통과 가능한지) 여부</summary>
        public bool IsOpen { get; private set; }

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _baseColor = _sr.color;

            if (_blocker == null)
            {
                _blocker = GetComponent<Collider2D>();
            }

            Apply();
        }

        /// <summary>장벽을 열거나 닫는다.</summary>
        /// <param name="open">열림 여부</param>
        public void SetOpen(bool open)
        {
            if (IsOpen == open)
            {
                return;
            }

            IsOpen = open;
            Apply();
        }

        private void Apply()
        {
            _sr.enabled = !IsOpen;

            if (_blocker != null)
            {
                _blocker.enabled = !IsOpen;
            }
        }

        private void Update()
        {
            if (IsOpen)
            {
                return;
            }

            float t = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) * 0.5f;
            float alpha = Mathf.Lerp(_pulseMin, _pulseMax, t);
            _sr.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, alpha);
        }
    }
}
