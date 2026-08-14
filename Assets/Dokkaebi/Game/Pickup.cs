using UnityEngine;
using Dokkaebi.Core;

namespace Dokkaebi.Game
{
    /// <summary>드롭 종류</summary>
    public enum PickupKind
    {
        /// <summary>마나(영불 구슬)</summary>
        Mana,

        /// <summary>골드(엽전)</summary>
        Gold
    }

    /// <summary>
    /// 바닥에 떨어진 드롭. 처음엔 튕겨 나가 흩어졌다가, 플레이어가 자석 범위에 들어오면
    /// 빨려 들어와 흡수된다. 운동 계산은 Core의 MagnetPull(순수 로직)이 담당한다.
    ///
    /// 물리 엔진을 쓰지 않고 직접 위치를 옮긴다 — 드롭은 벽에 부딪히거나 밀릴 필요가 없고,
    /// Rigidbody를 붙이면 적·플레이어와 충돌 처리까지 얽혀 오히려 성가시다.
    /// </summary>
    public class Pickup : MonoBehaviour
    {
        [SerializeField] private PickupKind _kind = PickupKind.Mana;

        /// <summary>마나면 회복량, 골드면 획득량(정수로 반올림)</summary>
        [SerializeField] private float _amount = 1f;

        [Header("자석 흡수")]
        [SerializeField] private float _magnetRadius = 3.2f;
        [SerializeField] private float _magnetMaxSpeed = 14f;
        [SerializeField] private float _magnetAcceleration = 34f;
        [SerializeField] private float _magnetDrag = 9f;

        /// <summary>이 거리 안에 닿으면 흡수된다</summary>
        [SerializeField] private float _collectRadius = 0.28f;

        [Header("등장")]
        [SerializeField] private float _scatterSpeed = 3.2f;

        /// <summary>등장 직후 이 시간 동안은 흡수되지 않는다(떨어지자마자 빨려드는 걸 막는다)</summary>
        [SerializeField] private float _armDelay = 0.25f;

        private MagnetPull _magnet;
        private PlayerController _player;
        private Vector2 _velocity;
        private float _age;

        private void Awake()
        {
            _magnet = new MagnetPull(_magnetRadius, _magnetMaxSpeed, _magnetAcceleration, _magnetDrag);

            // 죽은 자리에서 사방으로 흩어지게 — 여러 개가 겹쳐 보이지 않는다
            float angle = Random.Range(0f, Mathf.PI * 2f);
            _velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _scatterSpeed;
        }

        private void Start()
        {
            _player = FindFirstObjectByType<PlayerController>();
        }

        /// <summary>드롭 종류와 양을 지정한다(스폰 직후 호출).</summary>
        /// <param name="kind">종류</param>
        /// <param name="amount">마나 회복량 또는 골드 획득량</param>
        public void Init(PickupKind kind, float amount)
        {
            _kind = kind;
            _amount = amount;
        }

        private void Update()
        {
            _age += Time.deltaTime;

            if (_player == null || _player.IsDead)
            {
                Drift();
                return;
            }

            Vector2 self = transform.position;
            Vector2 target = _player.transform.position;

            bool armed = _age >= _armDelay;
            _velocity = armed
                ? ToUnity(_magnet.Step(ToNumerics(self), ToNumerics(target), ToNumerics(_velocity), Time.deltaTime))
                : ToUnity(_magnet.Step(ToNumerics(self), ToNumerics(self), ToNumerics(_velocity), Time.deltaTime));

            transform.position = self + _velocity * Time.deltaTime;

            if (armed && Vector2.Distance(transform.position, target) <= _collectRadius)
            {
                Collect();
            }
        }

        /// <summary>플레이어가 없을 때는 그냥 감속만 시킨다.</summary>
        private void Drift()
        {
            Vector2 self = transform.position;
            _velocity = ToUnity(_magnet.Step(ToNumerics(self), ToNumerics(self), ToNumerics(_velocity), Time.deltaTime));
            transform.position = self + _velocity * Time.deltaTime;
        }

        private void Collect()
        {
            if (_kind == PickupKind.Gold)
            {
                _player.CollectGold(Mathf.RoundToInt(_amount));
            }
            else
            {
                _player.CollectMana(_amount);
            }

            Destroy(gameObject);
        }

        private static System.Numerics.Vector2 ToNumerics(Vector2 v)
        {
            return new System.Numerics.Vector2(v.x, v.y);
        }

        private static Vector2 ToUnity(System.Numerics.Vector2 v)
        {
            return new Vector2(v.X, v.Y);
        }
    }
}
