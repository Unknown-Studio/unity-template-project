using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using Suhdo.Data;
using Suhdo.Features.Player.StateMachine.RootStates;
using Suhdo.Features.Player.StateMachine.SubStates;
using Suhdo.Features.Weapons;
using Suhdo.Managers;
using Suhdo.Managers.Input;
using UnityEngine;

namespace Suhdo.Features.Player.StateMachine
{
    /// <summary>
    /// PlayerStateMachine là Context Object + MonoBehaviour điều phối HSM.
    /// 
    /// Trách nhiệm:
    /// 1. Nhận services từ Reflex DI — IInputService, CurrencyManager.
    /// 2. Pre-allocate TẤT CẢ State instances (Root + Sub) để tránh GC runtime.
    /// 3. Delegate Update/FixedUpdate xuống Root State hiện tại.
    /// 4. Cung cấp component references (Rigidbody2D, PlayerStats) cho States.
    /// 
    /// Mọi PlayerBaseState đều truy cập services qua Ctx (this).
    /// → IInputService inject 1 lần ở đây, tất cả states dùng Ctx.InputService.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerStateMachine : MonoBehaviour
    {
        // ═══════════════════════════════════════════════
        //  SERIALIZED FIELDS
        // ═══════════════════════════════════════════════

        [Header("Data")]
        [SerializeField] private PlayerStats playerStats;

        // ═══════════════════════════════════════════════
        //  INJECTED SERVICES (từ Reflex DI)
        // ═══════════════════════════════════════════════

        /// <summary>Input service — inject 1 lần, mọi state truy cập qua Ctx.InputService.</summary>
        public IInputService InputService { get; private set; }

        /// <summary>Currency manager — quản lý tiền tệ trong game.</summary>
        public CurrencyManager CurrencyManager { get; private set; }

        /// <summary>
        /// Weapon service — quản lý vũ khí đang trang bị và chỉ số (có buff).
        /// States truy cập qua Ctx.WeaponService.CurrentWeaponStat.
        /// </summary>
        public IWeaponService WeaponService { get; private set; }

        [Inject]
        public void Construct(IInputService inputService, CurrencyManager currencyManager, IWeaponService weaponService)
        {
            InputService    = inputService;
            CurrencyManager = currencyManager;
            WeaponService   = weaponService;
        }

        // ═══════════════════════════════════════════════
        //  COMPONENT REFERENCES
        // ═══════════════════════════════════════════════

        public Rigidbody2D Rigidbody2D { get; private set; }
        // public Animator Animator { get; private set; }
        public PlayerStats PlayerStats => playerStats;

        /// <summary>Quản lý visual prefab của vũ khí. Optional — null nếu không gắn.</summary>
        public WeaponHandler WeaponHandler { get; private set; }

        // ═══════════════════════════════════════════════
        //  CURRENT STATE
        // ═══════════════════════════════════════════════

        /// <summary>Root state đang active (NormalState hoặc CombatState).</summary>
        public PlayerBaseState CurrentState { get; private set; }

        /// <summary>
        /// Được gọi bởi PlayerBaseState.SwitchState() khi đổi Root State.
        /// KHÔNG gọi trực tiếp từ bên ngoài.
        /// </summary>
        internal void SetCurrentState(PlayerBaseState newState)
        {
            Debug.Log($"<color=#FFD700>[HSM]</color> Root: {CurrentState} → {newState}");
            CurrentState = newState;
        }

        // ═══════════════════════════════════════════════
        //  PRE-ALLOCATED STATES
        // ═══════════════════════════════════════════════

        // Root States
        public PlayerNormalState NormalState { get; private set; }
        public PlayerCombatState CombatState { get; private set; }

        // Sub States (shared giữa các Root)
        public PlayerIdleState IdleState { get; private set; }
        public PlayerMoveState MoveState { get; private set; }
        public PlayerGatherState GatherState { get; private set; }
        public PlayerAttackState AttackState { get; private set; }

        // ═══════════════════════════════════════════════
        //  GAME STATE FLAGS
        // ═══════════════════════════════════════════════

        /// <summary>True khi Player đứng trong vùng tương tác (cưa máy, thu thập...).</summary>
        public bool IsInActionZone { get; set; }

        /// <summary>True khi Player đứng trong vùng chiến đấu.</summary>
        public bool IsInCombatZone { get; set; }

        // ═══════════════════════════════════════════════
        //  UNITY LIFECYCLE
        // ═══════════════════════════════════════════════

        private void Awake()
        {
            Rigidbody2D  = GetComponent<Rigidbody2D>();
            WeaponHandler = GetComponentInChildren<WeaponHandler>();
            // Animator = GetComponent<Animator>();
        }

        private void Start()
        {
            // Pre-allocate tất cả States — ZERO GC allocation runtime
            NormalState = new PlayerNormalState(this);
            CombatState = new PlayerCombatState(this);
            IdleState   = new PlayerIdleState(this);
            MoveState   = new PlayerMoveState(this);
            GatherState = new PlayerGatherState(this);
            AttackState = new PlayerAttackState(this);

            // Khởi tạo WeaponHandler (visual) sau khi DI inject đã xong
            WeaponHandler?.Initialize(WeaponService, InputService);

            // TỰ ĐỘNG TRANG BỊ VŨ KHÍ MẶC ĐỊNH (Addressables + Firebase ready)
            if (!string.IsNullOrEmpty(playerStats.defaultWeaponId))
            {
                WeaponService.EquipWeaponByIdAsync(playerStats.defaultWeaponId).Forget();
            }

            // Khởi tạo HSM — bắt đầu từ NormalState (Root) → IdleState (Sub)
            CurrentState = NormalState;
            CurrentState.EnterState();

            Debug.Log($"<color=#FFD700>[HSM]</color> Initialized → {CurrentState}");
        }

        private void Update()
        {
            // 1 dòng duy nhất — Root.UpdateState() tự gọi Sub.UpdateState()
            CurrentState.UpdateState();
        }

        private void FixedUpdate()
        {
            // 1 dòng duy nhất — Root.PhysicsUpdateState() tự gọi Sub.PhysicsUpdateState()
            CurrentState.PhysicsUpdateState();
        }

        // ═══════════════════════════════════════════════
        //  TRIGGER ZONES
        // ═══════════════════════════════════════════════

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("ActionZone"))
                IsInActionZone = true;

            if (other.CompareTag("CombatZone"))
                IsInCombatZone = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("ActionZone"))
                IsInActionZone = false;

            if (other.CompareTag("CombatZone"))
                IsInCombatZone = false;
        }
    }
}
