using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ExampleApp.Public;

namespace ExampleApp.Core.Behaviours.ConstructorLib
{
    [RequireComponentInChildren(typeof(Rigidbody))]
    [RequireComponentInChildren(typeof(Collider))]
    [ExampleAppComponent(English: "Physics", Russian: "Физика")]
    public class PhysicsBehaviour : ConstructorExampleAppBehaviour, ISwitchModeSubscriber, IColliderAware
    {
        #region Enums

        public enum Relativeness
        {
            [Item(English: "object", Russian: "объекта")]
            Self,

            [Item(English: "world", Russian: "мира")]
            World
        }

        public enum Gravity
        {
            [Item(English: "affected by gravity", Russian: "подчиняется гравитации")]
            GravityOn,

            [Item(English: "does not affected by gravity", Russian: "не подчиняется гравитации")]
            GravityOff
        }

        public enum Kinematic
        {
            [Item(English: "object is static", Russian: "объект статичен")]
            Kinematic,

            [Item(English: "object is not static", Russian: "объект не статичен")]
            NonKinematic
        }

        public enum Obstacle
        {
            [Item(English: "object is obstacle", Russian: "объект является препятствием")]
            Obstacle,

            [Item(English: "object is not obstacle", Russian: "объект не является препятствием")]
            NonObstacle,
        }

        #endregion

        public delegate void CommonForceHandler();

        public delegate void TriggerHandler([Parameter(English: "target object", Russian: "целевой объект")] Wrapper target);

        private readonly List<int> _currentForceRoutines = new();
        private int _routineId;

        private readonly BehaviourState _currentState = new();

        private readonly List<Collider> _nonTriggerColliders = new();

        private Rigidbody _thisRigidbody;
        private Collider _thisCollider;

        private Vector3 _lastFrameVelocity;
        private bool _isKinematic;
        private bool _isKinematicInitialized;
        private bool _useGravity;
        private bool _isGravityInitialized;

        #region ExampleAppInspector

        [ExampleAppInspector(English: "Mass", Russian: "Масса")]
        public float MassInspector
        {
            set => _thisRigidbody.mass = value;
            get => _thisRigidbody.mass;
        }
        
        [ExampleAppInspector(English: "Bounciness", Russian: "Пружинистость")]
        public float BouncinessInspector
        {
            set => _thisCollider.material.bounciness = value;
            get => _thisCollider.material.bounciness;
        }
        
        [ExampleAppInspector(English: "Use gravity", Russian: "Гравитация")]
        public bool GravityInspector
        {
            get
            {
                if (!_isGravityInitialized)
                {
                    _useGravity = _thisRigidbody.useGravity;
                    _isGravityInitialized = true;
                }
                return _useGravity;
            }
            set
            {
                if (_useGravity == value)
                {
                    return;
                }
                
                _useGravity = value;
                TrySetPhysicsSettings();
            }
        }
        
        [ExampleAppInspector(English: "Static", Russian: "Статичный")]
        public bool KinematicInspector
        {
            get
            {
                if (!_isKinematicInitialized)
                {
                    _isKinematic = _thisRigidbody.isKinematic;
                    _isKinematicInitialized = true;
                }

                return _isKinematic;
            }
            set
            {
                if (_isKinematic == value)
                {
                    return;
                }
                
                _isKinematic = value;
                TrySetPhysicsSettings();
            }
        }

        [ExampleAppInspector(English: "Is obstacle", Russian: "Препятствие")]
        public bool ObstacleInspector
        {
            set => ObstacleSetter = value ? Obstacle.Obstacle : Obstacle.NonObstacle;
            get => _nonTriggerColliders.Any(col => !col.isTrigger);
        }
        
        #endregion

        #region Actions

        [LogicGroup(English: "Physics", Russian: "Физика")]
        [LogicTooltip(English: "Instantly applies force to the object in the direction of the specified vector in the selected coordinate system.", Russian: "Мгновенно прикладывает силу к объекту в направлении заданного вектора в выбранной системе координат.")]
        [Action(English: "apply force of", Russian: "приложить силу величиной")]
        [ArgsFormat(English: "{%} in direction of {%} relative to the {%}", Russian: "{%} в направлении {%} относительно {%}")]
        public void ApplyForceInDirectionRelativeTo(float forceValue, Vector3 forceDirection, Relativeness relativeness)
        {
            if (!_thisRigidbody)
            {
                return;
            }

            var force = forceDirection.normalized * forceValue;

            switch (relativeness)
            {
                case Relativeness.Self:
                    _thisRigidbody.AddRelativeForce(force, ForceMode.Impulse);
                    break;
                case Relativeness.World:
                    _thisRigidbody.AddForce(force, ForceMode.Impulse);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(relativeness), relativeness, null);
            }
        }

        [LogicGroup(English: "Physics", Russian: "Физика")]
        [LogicTooltip(English: "Applies a force to an object in the direction of a specified vector in the selected coordinate system for the specified time.", Russian: "Прикладывает силу к объекту в направлении заданного вектора в выбранной системе  координат в течение указанного времени.")]
        [Action(English: "apply force of", Russian: "приложить силу величиной")]
        [ArgsFormat(English: "{%} in direction of {%} for {%} relative to the {%}", Russian: "{%} в направлении {%} в течение {%} относительно {%}")]
        public IEnumerator StartApplyingForceInDirectionRelativeTo(float forceValue, Vector3 forceDirection, float duration, Relativeness relativeness)
        {
            var routineId = _routineId++;
            _currentForceRoutines.Add(routineId);

            _currentState.IsPerforming = true;

            var force = forceDirection.normalized * forceValue;

            var travelTime = 0f;

            while (travelTime <= duration && _currentState.IsPerforming)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                switch (relativeness)
                {
                    case Relativeness.Self:
                        _thisRigidbody.AddRelativeForce(force);
                        break;
                    case Relativeness.World:
                        _thisRigidbody.AddForce(force);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(relativeness), relativeness, null);
                }

                yield return WaitForEndOfFrame;

                travelTime += Time.fixedDeltaTime;
            }

            _currentForceRoutines.Remove(routineId);
            OnForceApplyFinished?.Invoke();
        }

        [LogicGroup(English: "Physics", Russian: "Физика")]
        [LogicTooltip(English: "Controls any force application to object. A paused force application can be continued with the “Continue” block.", Russian: "Управляет действием любой силы на объект. Приостановленное действие силы можно возобновить блоком “Продолжить”.")]
        [ActionGroup("PhysicsControl")]
        [Action(English: "stop any force application", Russian: "остановить действие любой силы")]
        public void StopAnyForce()
        {
            _currentState.IsPerforming = false;
            StopAllCoroutines();
            _thisRigidbody.velocity = Vector3.zero;
            _thisRigidbody.angularVelocity = Vector3.zero;
        }

        [LogicGroup(English: "Physics", Russian: "Физика")]
        [LogicTooltip(English: "Controls any force application to object. A paused force application can be continued with the “Continue” block.", Russian: "Управляет действием любой силы на объект. Приостановленное действие силы можно возобновить блоком “Продолжить”.")]
        [ActionGroup("PhysicsControl")]
        [Action(English: "pause any force application", Russian: "приостановить действие любой силы")]
        public void PauseAnyForce()
        {
            _currentState.IsPaused = true;
        }

        [LogicGroup(English: "Physics", Russian: "Физика")]
        [LogicTooltip(English: "Controls any force application to object. A paused force application can be continued with the “Continue” block.", Russian: "Управляет действием любой силы на объект. Приостановленное действие силы можно возобновить блоком “Продолжить”.")]
        [ActionGroup("PhysicsControl")]
        [Action(English: "continue any force application", Russian: "продолжить действие любой силы")]
        public void ContinueAnyForce()
        {
            _currentState.IsPaused = false;
        }

        #endregion

        #region Variables

        [LogicGroup(English: "Physics", Russian: "Физика")]
        [LogicTooltip(English: "Returns the value of the selected physical property of the object.", Russian: "Возвращает величину выбранного физического свойства объекта.")]
        [VariableGroup("PhysicsValueGet")]
        [Variable(English: "value of the physical property mass", Russian: "величина физического свойства масса")]
        public float MassGetter => _thisRigidbody.mass;

        [LogicGroup(English: "Physics", Russian: "Физика")]
        [LogicTooltip(English: "Returns the value of the selected physical property of the object.", Russian: "Возвращает величину выбранного физического свойства объекта.")]
        [VariableGroup("PhysicsValueGet")]
        [Variable(English: "value of the physical property bounciness", Russian: "величина физического свойства пружинистость")]
        public float BouncinessGetter => _thisCollider.material.bounciness;

        [LogicGroup(English: "Physics", Russian: "Физика")]
        [LogicTooltip(English: "Returns the value of the selected physical property of the object.", Russian: "Возвращает величину выбранного физического свойства объекта.")]
        [VariableGroup("PhysicsValueGet")]
        [Variable(English: "value of the physical property acceleration", Russian: "величина физического свойства ускорение")]
        public float AccelerationGetter => Acceleration;

        public float Acceleration { private set; get; }

        [LogicGroup(English: "Physics", Russian: "Физика")]
        [LogicTooltip(English: "Returns the value of the selected physical property of the object.", Russian: "Возвращает величину выбранного физического свойства объекта.")]
        [VariableGroup("PhysicsValueGet")]
        [Variable(English: "value of the physical property speed", Russian: "величина физического свойства скорость")]
        public float SpeedGetter => _thisRigidbody.velocity.magnitude;

        [LogicGroup(English: "Physics", Russian: "Физика")]
        [LogicTooltip(English: "Returns the value of the selected physical property of the object.", Russian: "Возвращает величину выбранного физического свойства объекта.")]
        [VariableGroup("PhysicsValueGet")]
        [Variable(English: "value of the physical property angular speed", Russian: "величина физического свойства угловая скорость")]
        public float AngularSpeedGetter => _thisRigidbody.angularVelocity.magnitude;

        [LogicGroup(English: "Physics", Russian: "Физика")]
        [LogicTooltip(English: "Sets the value of one of the physical properties of the object.", Russian: "Задает величину одного из физических свойств объекта.")]
        [VariableGroup("PhysicsValueSet")]
        [Variable(English: "value of the physical property mass", Russian: "величина физического свойства масса")]
        public float MassSetter
        {
            set => _thisRigidbody.mass = value;
        }

        [LogicGroup(English: "Physics", Russian: "Физика")]
        [LogicTooltip(English: "Sets the value of one of the physical properties of the object.", Russian: "Задает величину одного из физических свойств объекта.")]
        [VariableGroup("PhysicsValueSet")]
        [Variable(English: "value of the physical property bounciness", Russian: "величина физического свойства пружинистость")]
        public float BouncinessSetter
        {
            set => _thisCollider.material.bounciness = Mathf.Clamp01(value);
        }

        [LogicGroup(English: "Physics", Russian: "Физика")]
        [LogicTooltip(English: "Sets whether gravity affects the object.", Russian: "Задает, воздействует ли гравитация на объект.")]
        [Variable(English: "physical property", Russian: "физическое свойство")]
        public Gravity GravitySetter
        {
            set => _thisRigidbody.useGravity = value == Gravity.GravityOn;
        }

        [LogicGroup(English: "Physics", Russian: "Физика")]
        [LogicTooltip(English: "Sets whether the object is static. If the object is static, there are no physical forces acting on it.", Russian: "Задает статичность указанного объекта. Если объект статичный, никакие физические силы не воздействуют на него.")]
        [Variable(English: "physical property", Russian: "физическое свойство")]
        public Kinematic KinematicSetter
        {
            set => _thisRigidbody.isKinematic = value == Kinematic.Kinematic;
        }

        [LogicGroup(English: "Physics", Russian: "Физика")]
        [LogicTooltip(English: "Sets whether the specified object is an obstacle for the player and other objects.", Russian: "Задает, является ли указанный объект препятствием для игрока и других объектов.")]
        [Variable(English: "physical property", Russian: "физическое свойство")]
        public Obstacle ObstacleSetter
        {
            set
            {
                foreach (var col in _nonTriggerColliders)
                {
                    col.isTrigger = value == Obstacle.NonObstacle;
                }
            }
        }

        #endregion

        #region Checkers

        [LogicGroup(English: "Physics", Russian: "Физика")]
        [LogicTooltip(English: "Returns true if a force is currently being applied to the specified object. Otherwise, returns “false“", Russian: "Возвращает “истину”, если сила действует на указанный объект в данный момент. В противном случае возвращает “ложь”")]
        [Checker(English: "is affected by force at the moment", Russian: "подвержен приложению силы в данный момент")]
        public bool IsAffectedByForceNow()
        {
            return _currentForceRoutines.Count > 0;
        }

        #endregion

        #region Events

        [LogicGroup(English: "Physics", Russian: "Физика")]
        [LogicTooltip(English: "The event is triggered when the force has finished being applied to the object. The object for which the event was triggered is passed to the parameter.", Russian: "Событие срабатывает, когда сила перестает действовать на указанный объект. В параметр передается объект, для которого сработало событие.")]
        [Event(English: "the force has finished being applied to the object", Russian: "сила перестала действовать на объект")]
        public event CommonForceHandler OnForceApplyFinished;

        [LogicGroup(English: "Physics", Russian: "Физика")]
        [LogicTooltip(English: "The event is triggered when the specified object gets inside or outside another object. The parameters include the specified object and the object inside which the specified object got in or out.", Russian: "Событие срабатывает, когда указанный объект попал внутрь или вышел из другого объекта. В параметры передаются указанный объект и объект, внутрь которого попал или вышел указанный.")]
        [EventGroup("PhysicsTriggerEvents")]
        [Event(English: "object got inside of target object", Russian: "объект попал внутрь целевого объекта")]
        public event TriggerHandler OnPhysicsTriggerEnter;

        [LogicGroup(English: "Physics", Russian: "Физика")]
        [LogicTooltip(English: "The event is triggered when the specified object gets inside or outside another object. The parameters include the specified object and the object inside which the specified object got in or out.", Russian: "Событие срабатывает, когда указанный объект попал внутрь или вышел из другого объекта. В параметры передаются указанный объект и объект, внутрь которого попал или вышел указанный.")]
        [EventGroup("PhysicsTriggerEvents")]
        [Event(English: "object got outside of target object", Russian: "объект вышел наружу целевого объекта")]
        public event TriggerHandler OnPhysicsTriggerExit;

        #endregion

        #region PrivateHelpers

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void Awake()
        {
            _thisRigidbody = GetComponentInChildren<Rigidbody>();

            var allColliders = GetComponentsInChildren<Collider>();
            _thisCollider = allColliders[0]; // TODO how to be sure this is the "main" collider?

            foreach (var col in allColliders)
            {
                if (col.isTrigger)
                {
                    continue;
                }

                _nonTriggerColliders.Add(col);
            }
        }

        private void FixedUpdate()
        {
            var currentVelocity = _thisRigidbody.velocity;

            Acceleration = ((currentVelocity - _lastFrameVelocity) / Time.fixedDeltaTime).magnitude;

            _lastFrameVelocity = currentVelocity;
        }

        private Wrapper[] ContainedObjects { get; set; }
        
        public void OnObjectEnter(Wrapper[] wrappers)
        {
            foreach (var w in wrappers)
            {
                if (!IsContainObject(w))
                {
                    OnPhysicsTriggerEnter?.Invoke(w);
                }
            }
            
            ContainedObjects = wrappers;
        }

        public void OnObjectExit(Wrapper[] wrappers)
        {
            foreach (var w in ContainedObjects)
            {
                if (!IsContainObject(w, wrappers))
                {
                    OnPhysicsTriggerExit?.Invoke(w);
                }
            }

            ContainedObjects = wrappers;
        }
        
        private bool IsContainObject(Wrapper obj)
        {
            return ContainedObjects != null && ContainedObjects.Any(containedObj => containedObj == obj);
        }

        
        private static bool IsContainObject(Wrapper obj, Wrapper[] wrappers)
        {
            return wrappers != null && wrappers.Any(w => w == obj);
        }

        private void TrySetPhysicsSettings()
        {
            if (!ProjectData.IsPlayMode)
            {
                return;
            }
            
            ObjectController controller = gameObject.GetWrapper().GetObjectController();
            if (controller == null || controller.Parent is {LockChildren: true})
            {
                return;
            }
            
            _thisRigidbody.useGravity = GravityInspector;
            _thisRigidbody.isKinematic = KinematicInspector;
        }
        
        #endregion

        

        public void OnSwitchMode(GameMode newMode, GameMode oldMode)
        {
            TrySetPhysicsSettings();
        }
    }
}
