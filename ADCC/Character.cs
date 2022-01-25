using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ASFramework.Characters
{
    [RequireComponent(typeof(ProceduralAnimator))]
    public class Character : MonoBehaviour
    {
        #region state
        [Header("Default movement")]
        protected Rigidbody rb;
        public Vector3 headOffset = Vector3.up * 2;

        [Header("Animator")]
        [HideInInspector] public ProceduralAnimator animator;

        public CharacterState debugState; // doesn't actually do anything; just lets you see what state it's in
        public CharacterState state { get; private set; }
        protected MoveType currentMoveType;
        protected Dictionary<CharacterState, MoveType> moveTypes = new Dictionary<CharacterState, MoveType>();

        public float groundCheckRadius = 0.4f;
        public bool Grounded;

        public Vector3 groundSlope;
        [SerializeField] private LayerMask GroundMask;

        [Header("Control attributes")]
        protected InputModule inputModule;
        public Vector3 WASD;
        public Vector3 WASDQE;
        public Vector3 target;

        public Vector3 TargetForward { get; protected set; }
        public Vector3 TargetRight { get; protected set; }
        public Vector3 TargetDir { get; set; }
        #endregion

        #region action variables
        [SerializeField] public List<ProceduralAction> actions = new List<ProceduralAction>();
        [SerializeField] public List<ProceduralAction> chainActions = new List<ProceduralAction>();

        // action that runs when nothing else runs
        public string defaultActionName = "null";
        private bool defaultActing;

        // current action
        public ProceduralAction activeAction;

        public ActionIndexAndLayer queuedAction = new ActionIndexAndLayer(-1, 0, false);
        protected float queueTimeout = 0;

        protected float actionTimer;
        protected int chainOrigin;
        #endregion

        #region Unity Callbacks
        public virtual void Start()
        {
            rb = transform.GetComponent<Rigidbody>();
            animator = transform.GetComponent<ProceduralAnimator>();

            activeAction = new ProceduralAction("empty", -100, 0, 0, 0);
            actionTimer = 1;

            MoveType[] moves = transform.GetComponents<MoveType>();

            for (int i = 0; i < moves.Length; i++)
            {
                moveTypes.Add(moves[i].ThisState, moves[i]);
            }

            SwitchToState(CharacterState.Walking);
        }

        public virtual void Update()
        {
            TargetForward = Vector3.Scale(target - transform.position, Vector3.one - Vector3.up).normalized;
            TargetRight = Vector3.Cross(Vector3.up, TargetForward).normalized;
            TargetDir = (target - (transform.position + headOffset)).normalized;

            debugState = state;

            ActionChecks();
        }

        public virtual void FixedUpdate()
        {
            RaycastHit hit;
            if (Physics.SphereCast(transform.position + Vector3.up * 1f, groundCheckRadius, Vector3.down, out hit, 1f, GroundMask))
            {
                if (transform.position.y < hit.point.y && Vector3.Dot(hit.normal, Vector3.up) > 0.5f)
                {
                    float emulatedYVel = (hit.point.y - transform.position.y);
                    transform.position = transform.position + Vector3.up * emulatedYVel;

                    //Vector3 move = Vector3.Scale(((transform.position - hit.point).normalized * groundCheckRadius), new Vector3(1, -1, 1));

                    //transform.position += move;

                    if (rb.velocity.y < 0)
                    {
                        rb.velocity = Vector3.Scale(rb.velocity, Vector3.forward + Vector3.right);
                        //rb.useGravity = false;
                    }
                }
            }

            //if (rb.velocity.y > 0 || !Grounded)
                //rb.useGravity = true;

            Grounded = Physics.CheckSphere(transform.position, groundCheckRadius, GroundMask);
        }
        #endregion

        // sometimes, you want to do something specific when changing states
        #region special actions
        public void ToggleSprint()
        {
            if (state == CharacterState.Sprinting)
                SwitchToNeutralState();
            else if (state == CharacterState.Walking)
                SwitchToState(CharacterState.Sprinting);
        }

        public void Clamber(Vector3 _target, Vector3 _direction)
        {
            if (!moveTypes.ContainsKey(CharacterState.Clamber))
                return;

            Clamber clamb = (Clamber)moveTypes[CharacterState.Clamber];
            clamb.target = _target;
            clamb.direction = _direction;

            QueueAnimation("Clamber");
        }
        #endregion

        #region actions and states
        public void ActionChecks()
        {
            actionTimer += Time.deltaTime;

            // if there's no actions running or queued run idle
            if (defaultActionName == "NULL" && defaultActing)
            {
                StopAnimation();
                defaultActing = false;
            }
            else
                if (queuedAction.index == -1 && activeAction.index == -100)
                {
                    StartAnimation(defaultActionName);
                    defaultActing = true;
                }

            // if theres an active action AND a queued action, check if we can interrupt current action and start a new one
            if (queuedAction.index != -1)
                if (activeAction.index != -100)
                {
                    if (actionTimer > activeAction.interruptTime)
                    {
                        StartAnimation(queuedAction.index);
                    }
                }
                else
                {
                    StartAnimation(queuedAction.index);
                }

            if (activeAction.index != -100) // for some reason this won't null,
                if (!activeAction.Infinite && actionTimer >= activeAction.duration * 0.98f)
                {
                    SwitchToState(Grounded ? CharacterState.Walking : CharacterState.Airborne);
                    //activeAction = null; // even though we set it to null right here and it just repeats
                    activeAction = new ProceduralAction("", -100, 0, 0, 0);
                    animator.StopAction();
                    chainOrigin = -1;
                }
        }

        public void SetAnimFloat(string name, float value)
        {
            animator.animator.SetFloat(name, value);
        }

        public void SwitchToNeutralState()
        {
            if (Grounded)
                SwitchToState(CharacterState.Walking);
            else
                SwitchToState(CharacterState.Airborne);
        }

        public void SwitchToState(CharacterState _state)
        {
            if (!moveTypes.ContainsKey(_state))
                return;

            moveTypes[state].Run = false;
            moveTypes[_state].PrevState = state;

            currentMoveType = moveTypes[_state];

            state = _state;

            rb.drag = 0;

            rb.useGravity = true;

            moveTypes[state].Begin();

            moveTypes[state].Run = true;
        }

        public void SwitchToState(CharacterState _state, float locktime)
        {
            if (currentMoveType.TimeInState < locktime)
                return;

            SwitchToState(_state);
        }

        public void QueueAnimation(string name)
        {
            if (activeAction.AlwaysOverride)
                StartAnimation(name);
            else
                QueueAnimation(GetAnimationIndex(name));
        }

        public virtual void QueueAnimation(int index)
        {
            //if (!isServer && !isLocalPlayer)
            //   return;

            if (index < 0)
                return;

            if (activeAction.index != -100)
            {
                if (actionTimer < 0.2f)
                    return;

                if (chainOrigin == index && activeAction.childAction != -1)
                {
                    queuedAction.index = activeAction.childAction;
                    queuedAction.layer = actions[index].layer;
                    queuedAction.chain = true;

                    return;
                }
            }

            queuedAction.index = index;
            queuedAction.layer = actions[index].layer;
            queuedAction.chain = false;
        }

        public void StartAnimation(string name)
        {
            StartAnimation(GetAnimationIndex(name));
        }

        public virtual void StartAnimation(int index)
        {
            if (actions.Count <= index || index < 0)
                return;

            if (!actions[index].Ready())
                return;

            actionTimer = 0;

            if (queuedAction.chain == false)
            {
                activeAction = actions[index];
                animator.StartAction(activeAction);
                chainOrigin = index;
            }
            else
            {
                activeAction = chainActions[index];
                animator.ContinueChain();
            }

            defaultActing = false;

            activeAction.lastActivated = Time.time;

            if (activeAction.type == ActionType.StateChange)
                SwitchToState(activeAction.state);

            queuedAction.index = -1;
            queuedAction.layer = 0;
            queuedAction.chain = false;
        }

        public virtual void StopAnimation()
        {
            defaultActing = false;

            if (activeAction.index != -100) // for some reason this won't null,
                if (actionTimer >= activeAction.duration * 0.98f)
                {
                    SwitchToState(Grounded ? CharacterState.Walking : CharacterState.Airborne);
                    //activeAction = null; // even though we set it to null right here and it just repeats
                    activeAction = new ProceduralAction("", -100, 0, 0, 0);
                    animator.StopAction();
                    chainOrigin = -1;
                }
        }

        public int GetAnimationIndex(string name)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].name.ToLower() == name.ToLower())
                {
                    return i;
                }
            }

            return -1;
        }

        public int GetChainIndex(string name)
        {
            for (int i = 0; i < chainActions.Count; i++)
            {
                if (chainActions[i].name.ToLower() == name.ToLower())
                {
                    return i;
                }
            }

            return -1;
        }
        #endregion

        public virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + TargetForward);
            Gizmos.DrawLine(transform.position, transform.position + TargetRight);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(headOffset + transform.position, 0.2f);
        }
    }
    
    [Serializable]
    public class ProceduralAction
    {
        [Header("Name and Index")]
        public string name = "null";
        public int index = 1;
        public int layer = 0;
        public float interruptTime = 0; // time until interrupt?
        public float duration = 0;
        [Tooltip("Animation always gets overridden by starting or queuing a new one.")]
        public bool AlwaysOverride = false;
        [Tooltip("Action does not stop until interrupted.")]
        public bool Infinite = false;

        [Header("Gameplay Affectors")]
        public ActionType type;
        public CharacterState state;
        public float speed = 1;

        public float Cooldown;
        [HideInInspector] public float lastActivated;

        public int childAction;

        public ProceduralAction(string _name, int _index, int _layer, float _duration, float _interrupt)
        {
            name = _name;
            index = _index;
            layer = _layer;
            duration = _duration;
            interruptTime = _interrupt;
            type = ActionType.StateChange;
            state = CharacterState.RootMotion;
            speed = 1;
        }

        public ProceduralAction(string _name, int _index, int _layer, float _duration, float _interrupt, ActionType _type)
        {
            name = _name;
            index = _index;
            layer = _layer;
            duration = _duration;
            interruptTime = _interrupt;
            type = _type;
            state = CharacterState.RootMotion;
            speed = 1;
        }

        public bool Ready()
        { 
            return Time.time - lastActivated > Cooldown;
        }
    }

    [Serializable]
    public struct ActionIndexAndLayer
    {
        public int index;
        public int layer;
        public bool chain;

        public ActionIndexAndLayer(int _index, int _layer, bool _chain)
        {
            index = _index;
            layer = _layer;
            chain = _chain;
        }
    }

    public enum ActionType
    {
        StateChange,
        KeepState
    }

    public enum CharacterState
    {
        Walking,
        Sprinting,
        Airborne,
        Jumping,
        Dodge,
        Slide,
        Blink,
        Sit,
        Clamber,
        StaticAttack,
        RootMotion,
        MobileAttack
    }
}
