using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExampleApp.Public;

namespace ExampleApp.Core.Behaviours.ConstructorLib
{
    [ExampleAppComponent(English: "Motion", Russian: "Движение")]
    public class MotionBehaviour : ConstructorExampleAppBehaviour
    {
        public enum LockRotationRules
        {
            [Item("do not rotate")] 
            Lock,
            [Item("rotate only in the horizontal axis")] 
            OnlyHorizontal,
            [Item("rotate on all axes")] 
            AllowAll
        }

        private readonly List<int> _currentMotionRoutines = new();
        private int _routineId;

        private readonly BehaviourState _currentState = new();
        private float _minimumTargetStopDistance;

        public delegate void CommonMovementHandler();

        public delegate void ToWrapperMovementHandler([Parameter(English: "target object", Russian: "целевой объект")] Wrapper target);

        public delegate void ToVectorMovementHandler([Parameter(English: "target coordinates", Russian: "целевые координаты")] Vector3 target);

        public delegate void WayPointMovementHandler(
            [Parameter(English: "waypoint number", Russian: "номер точки в маршруте")] int wayPointIndex,
            [Parameter(English: "waypoint reached", Russian: "достигнутая точка")] dynamic waypoint);

        #region Actions

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "Instantly moves the specified object to a position set by world space coordinates.", Russian: "Мгновенно перемещает указанный объект в позицию, заданную с помощью координат в мировом пространстве.")]
        [Action(English: "set position", Russian: "задать позицию")]
        public void SetPosition(Vector3 targetPosition)
        {
            transform.position = targetPosition;
        }

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "Instantly moves the specified object to the coordinates of the second object.", Russian: "Мгновенно перемещает указанный объект по координатам второго объекта.")]
        [Action(English: "instantly move to the center of object", Russian: "мгновенно переместиться в центр объекта")]
        public void TeleportTo(Wrapper targetObject)
        {
            var destinationTransform = targetObject.GetGameObject()?.transform;

            if (destinationTransform == null)
            {
                return;
            }

            SetPosition(destinationTransform.position);
            OnAnyMovementFinished?.Invoke();
            OnToWrapperMovementFinished?.Invoke(targetObject);
        }

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "Starts the moving process of the specified object in the direction of the selected axis at the specified speed. The movement continues until it is stopped by the movement stop block. Use negative speed values to change the moving direction.", Russian: "Запускает процесс перемещения указанного объекта в направлении выбранной оси с заданной скоростью. Перемещение продолжается, пока оно не будет остановлено блоком завершения перемещения. Чтобы изменить направление перемещения, используйте отрицательные значение скорости.")]
        [Action(English: "move in direction of axis", Russian: "перемещаться в направлении оси")]
        [ArgsFormat(English: "{%} at a speed of {%} m/s", Russian: "{%} со скоростью {%} м/с")]
        public IEnumerator MoveByAxisWithSpeed(Axis axis, float speed)
        {
            var routineId = _routineId++;
            _currentMotionRoutines.Add(routineId);

            _currentState.IsMoving = true;

            var velocity = EnumToVector(axis) * speed;

            while (_currentState.IsMoving)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                SetPosition(transform.position + velocity * Time.deltaTime);
                yield return WaitForEndOfFrame;
            }

            _currentMotionRoutines.Remove(routineId);
        }

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "Starts the moving process of the specified object in the direction of the selected axis by the specified distance at the specified speed. The movement continues until the object covers the distance. Use negative speed values to change the moving direction.", Russian: "Запускает процесс перемещения указанного объекта в направлении выбранной оси на заданное расстояние с заданной скоростью. Перемещение продолжается, пока объект не преодолеет расстояние. Чтобы изменить направление перемещения, используйте отрицательные значение скорости.")]
        [Action(English: "move in direction of axis", Russian: "перемещаться в направлении оси")]
        [ArgsFormat(English: "{%} to a distance of {%} m at a speed of {%} m/s", Russian: "{%} на расстояние {%} м со скоростью {%} м/с")]
        public IEnumerator MoveByAxisAtDistance(Axis axis, float distance, float speed)
        {
            var routineId = _routineId++;
            _currentMotionRoutines.Add(routineId);

            _currentState.IsMoving = true;

            var velocity = EnumToVector(axis) * speed;
            var startPosition = transform.position;

            while (Vector3.Distance(startPosition, transform.position) < distance && _currentState.IsMoving)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                SetPosition(transform.position + velocity * Time.deltaTime);
                yield return WaitForEndOfFrame;
            }

            _currentMotionRoutines.Remove(routineId);

            OnAnyMovementFinished?.Invoke();
        }

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "Starts the moving process of the specified object in the direction of the selected axis for the specified time at the specified speed. Moving continues until the time has run out. Use negative speed values to change the moving direction.", Russian: "Запускает процесс перемещения указанного объекта в направлении выбранной оси в течение указанного времени с заданной скоростью. Перемещение продолжается, пока не истечет время. Чтобы изменить направление перемещения, используйте отрицательные значение скорости.")]
        [Action(English: "move in direction of axis", Russian: "перемещаться в направлении оси")]
        [ArgsFormat(English: "{%} for {%} s. at a speed of {%} m/s", Russian: "{%} в течение {%} с. со скоростью {%} м/с")]
        public IEnumerator MoveByAxisByTime(Axis axis, float duration, float speed)
        {
            var routineId = _routineId++;
            _currentMotionRoutines.Add(routineId);

            _currentState.IsMoving = true;

            var velocity = EnumToVector(axis) * speed;
            var travelTime = 0f;

            while (travelTime <= duration && _currentState.IsMoving)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                SetPosition(transform.position + velocity * Time.deltaTime);
                yield return WaitForEndOfFrame;

                travelTime += Time.deltaTime;
            }

            _currentMotionRoutines.Remove(routineId);

            OnAnyMovementFinished?.Invoke();
        }

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "Starts the moving process of the specified object in the direction of the second object at the specified speed. The movement continues until the specified object reaches the second object.", Russian: "Запускает процесс перемещения указанного объекта в направлении второго объекта с заданной скоростью. Перемещение продолжается, пока указанный объект не достигнет второго объекта.")]
        [Action(English: "move to object", Russian: "перемещаться к объекту")]
        [ArgsFormat(English: "{%} at a speed of {%} m/s", Russian: "{%} со скоростью {%} м/с")]
        public IEnumerator MoveToObjectAtSpeed(Wrapper targetObject, float speed)
        {
            var destinationTransform = targetObject.GetGameObject()?.transform;

            if (destinationTransform == null)
            {
                yield break;
            }

            yield return MoveToPointAtSpeed(destinationTransform.position, speed);
        }

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "Starts the moving process of the specified object in the direction of the specified coordinates at the specified speed. The movement continues until the object reaches the coordinates.", Russian: "Запускает процесс перемещения указанного объекта в направлении указанных координат с заданной скоростью. Перемещение продолжается, пока объект не достигнет координат.")]
        [Action(English: "move by coordinates", Russian: "перемещаться к координатам")]
        [ArgsFormat(English: "{%} at a speed of {%} m/s", Russian: "{%} со скоростью {%} м/с")]
        public IEnumerator MoveToCoordinatesAtSpeed(Vector3 destination, float speed)
        {
            yield return MoveToPointAtSpeed(destination, speed);
        }

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "Starts the moving process of the specified object along the path at the specified speed. A route is a list of objects or coordinates in world space defined by vectors.", Russian: "Запускает процесс перемещения указанного объекта по маршруту с указанной скоростью. Маршрут представляет собой список объектов или координат в мировом пространстве, заданных векторами.")]
        [Action(English: "move along the path", Russian: "перемещаться по маршруту")]
        [ArgsFormat(English: "{%} at a speed of {%} m/s", Russian: "{%} со скоростью {%} м/с")]
        public IEnumerator MoveAlongPath(List<dynamic> path, float speed)
        {
            var routineId = _routineId++;
            _currentMotionRoutines.Add(routineId);

            var currentWayPointIndex = 1;
            foreach (var pathItem in path)
            {
                var currentTarget = Vector3.zero;
                switch (pathItem)
                {
                    case Vector3 vector:
                        currentTarget = vector;
                        break;
                    case Wrapper wrapper:
                        var targetObject = wrapper.GetGameObject();

                        if (targetObject == null)
                        {
                            break;
                        }

                        currentTarget = targetObject.transform.position;

                        break;
                    default:
                        yield break;
                }

                yield return MoveToCoordinatesAtSpeed(currentTarget, speed);
                OnPathTargetedMovementFinished?.Invoke(currentWayPointIndex, pathItem);
                currentWayPointIndex++;
            }

            _currentMotionRoutines.Remove(routineId);

            OnAnyMovementFinished?.Invoke();
        }

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "Controls any movement. The paused movement can be resumed with the “Continue” block.", Russian: "Управляет любым перемещением. Приостановленное движение можно возобновить блоком “Продолжить”.")]
        [ActionGroup("MotionControl")]
        [Action(English: "stop any movement", Russian: "завершить любое перемещение")]
        public void StopAnyMovement()
        {
            _currentState.IsMoving = false;
            StopAllCoroutines();
        }

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "Controls any movement. The paused movement can be resumed with the “Continue” block.", Russian: "Управляет любым перемещением. Приостановленное движение можно возобновить блоком “Продолжить”.")]
        [ActionGroup("MotionControl")]
        [Action(English: "pause any movement", Russian: "приостановить любое перемещение")]
        public void PauseAnyMovement()
        {
            _currentState.IsPaused = true;
        }

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "Controls any movement. The paused movement can be resumed with the “Continue” block.", Russian: "Управляет любым перемещением. Приостановленное движение можно возобновить блоком “Продолжить”.")]
        [ActionGroup("MotionControl")]
        [Action(English: "continue any movement", Russian: "продолжить любое перемещение")]
        public void ContinueAnyMovement()
        {
            _currentState.IsPaused = false;
        }

        #endregion

        #region Checkers

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "Returns true if the specified object is moving. Otherwise returns false.", Russian: "Возвращает “истину”, если указанный объект перемещается в данный момент. В противном случае возвращает “ложь”.")]
        [Checker(English: "is moving at the moment", Russian: "перемещается в данный момент")]
        public bool IsMovingNow()
        {
            return _currentMotionRoutines.Count > 0;
        }

        #endregion

        #region Variables

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "Returns the position of the specified object along the selected axis in world coordinates.", Russian: "Возвращает позицию указанного объекта по выбранной оси в мировых координатах.")]
        [VariableGroup("GetPositionByAxis")]
        [Variable(English: "position on X axis", Russian: "позиция по оси X")]
        public float PositionX => transform.position.x;

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "Returns the position of the specified object along the selected axis in world coordinates.", Russian: "Возвращает позицию указанного объекта по выбранной оси в мировых координатах.")]
        [VariableGroup("GetPositionByAxis")]
        [Variable(English: "position on Y axis", Russian: "позиция по оси Y")]
        public float PositionY => transform.position.y;

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "Returns the position of the specified object along the selected axis in world coordinates.", Russian: "Возвращает позицию указанного объекта по выбранной оси в мировых координатах.")]
        [VariableGroup("GetPositionByAxis")]
        [Variable(English: "position on Z axis", Russian: "позиция по оси Z")]
        public float PositionZ => transform.position.z;

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "Возвращает позицию указанного объекта в мировых координатах в виде вектора  [x;  y;  z]", Russian: "Returns the position of the specified object in world coordinates as a vector [x; y; z]")]
        [Variable(English: "position", Russian: "позиция")]
        public Vector3 Position => transform.position;

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "Sets the side of the object that it will be facing in the direction of the move.", Russian: "Задает сторону объекта, которой он будет направлен в сторону перемещения. Значение для настройки перемещения объекта “вперёд лицом”: (x: 0; y: 0; z: 1)")]
        [Variable(English: "front side while moving", Russian: "лицевая сторона при перемещении")]
        public Vector3 MovementFaceDirection
        {
            set => transform.forward = value.normalized;
        }

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "Sets the minimum distance between the specified and target objects for movement to be considered complete. The block calculates the distance between the centers of the objects, so using a 0 value is not recommended.", Russian: "Задает минимальное расстояние между заданным и целевым объектами, чтобы движение к нему считалось завершенным. Вычисляется расстояние между центрами объектов, поэтому использование значения 0 не рекомендуется.")]
        [Variable(English: "minimum stop distance in front of target object", Russian: "минимальное расстояние остановки перед целевым объектом")]
        [ArgsFormat(English: "{%} m.", Russian: "{%} м.")]
        public float MinimumTargetStopDistance
        {
            set => _minimumTargetStopDistance = value;
        }
        
        #endregion

        #region Functions

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "Returns the straight-line distance from the specified object to the second object or to world coordinates specified with a vector. The distance is returned in meters as a real number.", Russian: "Возвращает расстояние по прямой от указанного объекта до второго объекта или мировых координат, указанных с помощью вектора. Расстояние возвращается в метрах в виде вещественного числа.")]
        [Function(English: "distance to", Russian: "расстояние до")]
        public float GetDistanceTo(dynamic target)
        {
            var distanceTarget = target;
            switch (target)
            {
                case Vector3 vector:
                    distanceTarget = vector;
                    break;
                case GameObject obj:
                    distanceTarget = obj.transform.position;
                    break;
                case Wrapper wrapper:
                    var targetObject = wrapper.GetGameObject();
                    
                    if (targetObject == null)
                    {
                        break;
                    }
                    
                    distanceTarget = targetObject.transform.position;
                    
                    break;
                default:
                    distanceTarget = null;
                    break;
            }

            return distanceTarget == null ? 0 : (float) Vector3.Distance(transform.position, distanceTarget);
        }

        #endregion

        #region Events

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "The event is triggered when the specified object completes any movement. The movement is considered completed if the object has reached the target position or if the movement has been stopped by the corresponding block. The object for which the event was triggered is passed to the parameter.", Russian: "Событие срабатывает, когда указанный объект завершает любое перемещение. Перемещение считается завершенным, если объект достиг целевой позиции, или если перемещение было остановлено соответствующим блоком. В параметр передается объект, для которого сработало событие.")]
        [Event(English: "completed any movement", Russian: "завершил любое перемещение")]
        public event CommonMovementHandler OnAnyMovementFinished;

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "The event is triggered when the specified object completes moving to the target object. The object for which the event was triggered (moving object) and the object to which the movement was completed (target object) are passed in parameters.", Russian: "Событие срабатывает, когда указанный объект завершает перемещение к целевому объекту. В параметры передается объект, у которого сработало событие (перемещающийся объект), а также объект, к которому было завершено перемещение (целевой объект).")]
        [Event(English: "completed moving to target object", Russian: "завершил перемещение к целевому объекту")]
        public event ToWrapperMovementHandler OnToWrapperMovementFinished;

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "The event is triggered when the specified object completes moving to the target coordinates. The object for which the event was triggered (moving object) and the coordinates as vector to which the movement was completed (target coordinates) are passed in parameters.", Russian: "Событие срабатывает, когда указанный объект завершает перемещение к целевым координатам. В параметры передается объект, у которого сработало событие (перемещающийся объект), а также координаты, в виде вектора, к которым было завершено перемещение (целевые координаты).")]
        [Event(English: "completed moving to", Russian: "завершил движение к целевым координатам")]
        public event ToVectorMovementHandler OnToVectorMovementFinished;

        [LogicGroup(English: "Motion", Russian: "Перемещение")]
        [LogicTooltip(English: "The event is triggered when the specified object moving along the path reaches a waypoint on the path. The parameters pass the object for which the event was triggered, the number of the waypoint, and the point reached.", Russian: "Событие срабатывает, когда указанный объект, двигающийся по маршруту, достигает очередную точки маршрута. В параметры передается объект, у которого сработало событие, номер точки в маршруте, а также достигнутая точка.")]
        [Event(English: "reached a waypoint", Russian: "достиг точки маршрута")]
        public event WayPointMovementHandler OnPathTargetedMovementFinished;

        #endregion

        #region PrivateHelpers
        
        private IEnumerator MoveToPointAtSpeed(Vector3 destination, float speed)
        {
            var routineId = _routineId++;
            _currentMotionRoutines.Add(routineId);

            _currentState.IsMoving = true;

            var velocity = new Vector3();
            var distanceToObject = Vector3.Distance(destination, transform.position);

            while (distanceToObject > velocity.magnitude && _currentState.IsMoving)
            {
                var thisPosition = transform.position;

                distanceToObject = Vector3.Distance(destination, thisPosition);
                velocity = (destination - thisPosition).normalized * speed * Time.deltaTime;

                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                SetPosition(transform.position + velocity);
                yield return WaitForEndOfFrame;
            }

            SetPosition(destination);

            _currentMotionRoutines.Remove(routineId);

            OnAnyMovementFinished?.Invoke();
            OnToVectorMovementFinished?.Invoke(destination);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        #endregion
        
    }
}
