using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExampleApp.Public;

namespace ExampleApp.Core.Behaviours.ConstructorLib
{
    [ExampleAppComponent(English: "Rotation", Russian: "Вращение")]
    public class RotateBehaviour : ConstructorExampleAppBehaviour
    {
        private readonly List<int> _currentRotateRoutines = new();
        private int _routineId;

        private readonly BehaviourState _currentState = new();

        public delegate void CommonRotationHandler();

        public delegate void ToWrapperRotationHandler([Parameter(English: "target object", Russian: "целевой объект")] Wrapper target);

        public delegate void ToVectorRotationHandler([Parameter(English: "target rotation", Russian: "целевой поворот")] Vector3 target);

        #region Actions

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "Instantly sets the rotation of the object in degrees along the three axes. The rotation is counted relative to the world coordinates.", Russian: "Мгновенно задает поворот указанного объекта в градусах по трем осям. Поворот считается относительно мировых координат.")]
        [Action(English: "set rotation", Russian: "задать поворот")]
        public void SetRotation(Vector3 eulerAngles)
        {
            RotateHierarchy(gameObject, Quaternion.Euler(eulerAngles));
        }

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "Instantly rotates the object by the specified angle along the selected axis.", Russian: "Мгновенно поворачивает объект на указанный угол по выбранной оси.")]
        [Action(English: "instantly rotate", Russian: "мгновенно повернуться на")]
        [ArgsFormat(English: "{%} degrees on axis {%} m/s", Russian: "{%} градусов по оси {%}")]
        public void RotateByDegreesOnAxis(float angle, Axis axis)
        {
            SetRotation(EnumToVector(axis) * angle);
        }

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "Instantly rotates the object to the selected object.", Russian: "Мгновенно поворачивает объект к другому выбранному объекту.")]
        [Action(English: "instantly rotate to object", Russian: "мгновенно повернуться к объекту")]
        public void RotateToObject(Wrapper targetObject)
        {
            var destinationTransform = targetObject.GetGameObject()?.transform;

            if (destinationTransform == null)
            {
                return;
            }

            var direction = (destinationTransform.position - transform.position).normalized;
            var rotation = Quaternion.LookRotation(direction, Vector3.up);

            SetRotation(rotation.eulerAngles);
        }

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "Instantly rotates the object in the same way as the selected object.", Russian: "Мгновенно задает объекту параметры вращения другого выбранного объекта.")]
        [Action(English: "Instantly rotate the same way as object", Russian: "Мгновенно повернуться также, как объект")]
        public void RotateAsObject(Wrapper targetObject)
        {
            var destinationTransform = targetObject.GetGameObject()?.transform;

            if (destinationTransform == null)
            {
                return;
            }

            SetRotation(destinationTransform.eulerAngles);
        }

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "Starts rotation of the specified object around the selected local axis with the specified speed. The rotation continues until it is stopped by the rotation stop block. Use negative speed values to change the rotation direction.", Russian: "Запускает вращение указанного объекта вокруг выбранной локальной оси с заданной скоростью. Вращение происходит, пока оно не будет остановлено блоком остановки вращения. Чтобы изменить направление вращения, используйте отрицательные значения скорости.")]
        [Action(English: "rotate around the axis", Russian: "вращаться вокруг оси")]
        [ArgsFormat(English: "{%} at a speed of {%} degrees/s.", Russian: "{%} со скоростью {%} градусов/сек.")]
        public IEnumerator RotateAroundAxisWithSpeed(Axis axis, float speed)
        {
            var routineId = _routineId++;
            _currentRotateRoutines.Add(routineId);

            _currentState.IsRotating = true;

            var velocity = EnumToVector(axis) * speed;

            while (_currentState.IsRotating)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                SetRotation(transform.eulerAngles + velocity * Time.deltaTime);
                yield return WaitForEndOfFrame;
            }

            _currentRotateRoutines.Remove(routineId);
            OnCommonRotationFinished?.Invoke();
        }

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "Starts rotation of the object around the selected axis for the specified time with the specified speed. Use negative speed values to change the rotation direction.", Russian: "Запускает вращение объекта вокруг выбранной оси в течение указанного времени с заданной скоростью. Для изменения направления вращения используйте отрицательные значения скорости.")]
        [Action(English: "rotate around the axis", Russian: "вращаться вокруг оси")]
        [ArgsFormat(English: "{%} for {%} s. at a speed of {%} degrees/s.", Russian: "{%} в течение {%} сек. со скоростью {%} градусов/сек.")]
        public IEnumerator RotateAroundAxisForDurationWithSpeed(Axis axis, float duration, float speed)
        {
            var routineId = _routineId++;
            _currentRotateRoutines.Add(routineId);

            _currentState.IsRotating = true;

            var velocity = EnumToVector(axis) * speed;
            var travelTime = 0f;

            while (travelTime <= duration && _currentState.IsRotating)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                SetRotation(transform.eulerAngles + velocity * Time.deltaTime);

                travelTime += Time.deltaTime;
                yield return WaitForEndOfFrame;
            }
            
            _currentRotateRoutines.Remove(routineId);
            OnCommonRotationFinished?.Invoke();
        }

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(
            English: "Starts rotation of the object around the selected axis of another object with a specified speed.",
            Russian: "Запускает вращение объекта вокруг выбранной оси другого объекта с заданной скоростью.")]
        [Action(English: "rotate around the axis", Russian: "вращаться вокруг оси")]
        [ArgsFormat(English: "{%} of the object {%} at a speed of {%} degrees/s.", Russian: "{%} объекта {%} со скоростью {%} градусов/сек.")]
        public IEnumerator RotateAroundAnotherObjectAxisWithSpeed(Axis axis, Wrapper targetObject, float speed)
        {
            var destinationTransform = targetObject.GetGameObject()?.transform;

            if (destinationTransform == null)
            {
                yield break;
            }

            var routineId = _routineId++;
            _currentRotateRoutines.Add(routineId);

            _currentState.IsRotating = true;

            var velocity = EnumToVector(axis) * speed;

            while (_currentState.IsRotating)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                var eulerAngles = Quaternion.Euler(transform.eulerAngles + velocity * Time.deltaTime);
                RotateHierarchy(gameObject, eulerAngles, destinationTransform);
                yield return WaitForEndOfFrame;
            }

            _currentRotateRoutines.Remove(routineId);
            OnCommonRotationFinished?.Invoke();
            OnToWrapperRotationFinished?.Invoke(targetObject);
        }

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "Starts rotation of the object to another selected object at a specified speed.", Russian: "Запускает вращение объекта к другому выбранному объекту с указанной скоростью.")]
        [Action(English: "rotate to object", Russian: "повернуться к объекту")]
        [ArgsFormat(English: "{%} at a speed of {%} degrees/s.", Russian: "{%} со скоростью {%} градусов/сек.")]
        public IEnumerator LookAtObjectWithSpeed(Wrapper targetObject, float speed)
        {
            var routineId = _routineId++;
            _currentRotateRoutines.Add(routineId);

            _currentState.IsRotating = true;

            var destinationTransform = targetObject.GetGameObject()?.transform;

            if (destinationTransform == null)
            {
                yield break;
            }

            var thisTransform = transform;

            var startRotation = thisTransform.rotation;
            var targetRotation = Quaternion.LookRotation((destinationTransform.position - thisTransform.position).normalized,
                    Vector3.up);

            var rotationMaxTime = GetRotationFromToTime(startRotation, targetRotation, speed);

            var rotationCurrentTime = 0f;

            while (rotationCurrentTime <= rotationMaxTime && _currentState.IsRotating)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                SetRotation(Quaternion.Lerp(startRotation, targetRotation, rotationCurrentTime / rotationMaxTime).eulerAngles);
                rotationCurrentTime += Time.deltaTime;
                yield return WaitForEndOfFrame;
            }
            
            _currentRotateRoutines.Remove(routineId);
            OnCommonRotationFinished?.Invoke();
            OnToWrapperRotationFinished?.Invoke(targetObject);
            SetRotation(targetRotation.eulerAngles);
        }

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "Starts rotation of the object according to the parameters of the other selected object with the specified speed.", Russian: "Запускает вращение объекта в соответствии с параметрами другого выбранного объекта с указанной скоростью.")]
        [Action(English: "rotate the same way as object", Russian: "повернуться так же, как объект")]
        [ArgsFormat(English: "{%} at a speed of {%} degrees/s.", Russian: "{%} со скоростью {%} градусов/сек.")]
        public IEnumerator RotateAsObjectWithSpeed(Wrapper targetObject, float speed)
        {
            var routineId = _routineId++;
            _currentRotateRoutines.Add(routineId);

            _currentState.IsRotating = true;

            var destinationTransform = targetObject.GetGameObject()?.transform;

            if (destinationTransform == null)
            {
                yield break;
            }

            var thisTransform = transform;

            var startRotation = thisTransform.rotation;
            var targetRotation = destinationTransform.rotation;

            var rotationMaxTime = GetRotationFromToTime(startRotation, targetRotation, speed);

            if (rotationMaxTime == 0)
            {
                yield break;
            }

            var rotationCurrentTime = 0f;

            while (rotationCurrentTime <= rotationMaxTime && _currentState.IsRotating)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                SetRotation(Quaternion.Lerp(startRotation, targetRotation, rotationCurrentTime / rotationMaxTime).eulerAngles);
                rotationCurrentTime += Time.deltaTime;
                yield return WaitForEndOfFrame;
            }

            _currentRotateRoutines.Remove(routineId);

            OnCommonRotationFinished?.Invoke();
            OnAsWrapperRotationFinished?.Invoke(targetObject);

            SetRotation(targetRotation.eulerAngles);
        }

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "Starts rotation of the object to an angle in world axes defined by a vector with angles on each of the axes [0...360]. The rotation will be performed along the shortest path.", Russian: "Запускает вращение объекта к углу в мировых координатах, заданного вектором с углами по каждой из осей [0...360]. Поворот будет производится по наименьшему пути.")]
        [Action(English: "rotate to the angle", Russian: "повернуться к углу")]
        [ArgsFormat(English: "{%} at a speed of {%} degrees/s.", Russian: "{%} со скоростью {%} градусов/сек.")]
        public IEnumerator RotateToVectorWithSpeed(Vector3 eulerAngles, float speed)
        {
            var routineId = _routineId++;
            _currentRotateRoutines.Add(routineId);

            _currentState.IsRotating = true;

            var thisTransform = transform;

            var startRotation = thisTransform.rotation;
            var targetRotation = Quaternion.Euler(eulerAngles);

            var rotationMaxTime = GetRotationFromToTime(startRotation, targetRotation, speed);

            if (Mathf.Abs(rotationMaxTime) < Mathf.Epsilon)
            {
                yield break;
            }

            var rotationCurrentTime = 0f;

            while (rotationCurrentTime <= rotationMaxTime && _currentState.IsRotating)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                RotateHierarchy(gameObject, Quaternion.Lerp(startRotation, targetRotation, Mathf.Clamp01(rotationCurrentTime / rotationMaxTime)));
                rotationCurrentTime += Time.deltaTime;
                yield return WaitForEndOfFrame;
            }

            _currentRotateRoutines.Remove(routineId);

            OnCommonRotationFinished?.Invoke();
            OnToVectorRotationFinished?.Invoke(eulerAngles);

            SetRotation(eulerAngles);
        }

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "Starts rotation of the object around the selected local axis with a specified speed. Use negative speed values to change the rotation direction.", Russian: "Запускает вращение объекта вокруг выбранной локальной оси с заданной скоростью. Для изменения направления вращения используйте отрицательные значения скорости.")]
        [Action(English: "rotate around the axis", Russian: "повернуться вокруг оси")]
        [ArgsFormat(English: "{%} by {%} degrees at a speed of {%} degrees/s.", Russian: "{%} на {%} градусов со скоростью {%} градусов/сек.")]
        public IEnumerator RotateAroundAxisByAngleWithSpeed(Axis axis, float angle, float speed)
        {
            var routineId = _routineId++;
            _currentRotateRoutines.Add(routineId);

            _currentState.IsRotating = true;

            var currentRotationDoneAngle = 0f;

            var velocity = EnumToVector(axis) * speed;

            while (currentRotationDoneAngle <= angle && _currentState.IsRotating)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                SetRotation(transform.eulerAngles + velocity * Time.deltaTime);

                currentRotationDoneAngle += speed * Time.deltaTime;
                yield return WaitForEndOfFrame;
            }

            _currentRotateRoutines.Remove(routineId);

            OnCommonRotationFinished?.Invoke();
        }

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "Controls any rotation. A paused rotation can be continued with the \"Continue\" block.", Russian: "Управляет любым вращением. Приостановленное вращение можно возобновить блоком «Продолжить».")]
        [ActionGroup("RotateControl")]
        [Action(English: "stop any rotation", Russian: "завершить любое вращение")]
        public void StopAnyRotation()
        {
            _currentState.IsRotating = false;
            StopAllCoroutines();
        }

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "Controls any rotation. A paused rotation can be continued with the \"Continue\" block.", Russian: "Управляет любым вращением. Приостановленное вращение можно возобновить блоком «Продолжить».")]
        [ActionGroup("RotateControl")]
        [Action(English: "pause any rotation", Russian: "приостановить любое вращение")]
        public void PauseAnyRotation()
        {
            _currentState.IsPaused = true;
        }

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "Controls any rotation. A paused rotation can be continued with the \"Continue\" block.", Russian: "Управляет любым вращением. Приостановленное вращение можно возобновить блоком «Продолжить».")]
        [ActionGroup("RotateControl")]
        [Action(English: "continue any rotation", Russian: "продолжить любое вращение")]
        public void ContinueAnyRotation()
        {
            _currentState.IsPaused = false;
        }

        #endregion

        #region Checkers

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "Returns “true” if the specified object is currently rotating. Otherwise it returns \"false\".",Russian: "Возвращает «истину», если указанный объект вращается в данный момент. В противном случае возвращает “ложь”")]
        [Checker(English: "is rotating at the moment", Russian: "вращается в данный момент")]
        public bool IsRotatingNow()
        {
            return _currentRotateRoutines.Count > 0;
        }

        #endregion

        #region Variables

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "Returns the rotation angle of the object along the selected axis in world coordinates.", Russian: "Возвращает угол поворота указанного объекта по выбранной оси в мировых координатах.")]
        [VariableGroup("AngleOnAxis")]
        [Variable(English: "angle of rotation on the X axis", Russian: "угол поворота по оси X")]
        public float AngleOnXAxis => transform.eulerAngles.x;

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "Returns the rotation angle of the object along the selected axis in world coordinates.", Russian: "Возвращает угол поворота указанного объекта по выбранной оси в мировых координатах.")]
        [VariableGroup("AngleOnAxis")]
        [Variable(English: "angle of rotation on the Y axis", Russian: "угол поворота по оси Y")]
        public float AngleOnYAxis => transform.eulerAngles.y;

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "Returns the rotation angle of the object along the selected axis in world coordinates.", Russian: "Возвращает угол поворота указанного объекта по выбранной оси в мировых координатах.")]
        [VariableGroup("AngleOnAxis")]
        [Variable(English: "angle of rotation on the Z axis", Russian: "угол поворота по оси Z")]
        public float AngleOnZAxis => transform.eulerAngles.z;

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "Returns the rotation of the specified object in world coordinates as a vector.", Russian: "Возвращает поворот объекта в мировых координатах в виде вектора.")]
        [Variable(English: "rotation", Russian: "поворот")]
        public Vector3 Angle => transform.eulerAngles;

        #endregion

        #region Functions

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "Returns the angle of the object relative to another object along the selected axis.", Russian: "Возвращает угол поворота объекта относительно другого объекта по выбранной оси.")]
        [Function(English: "the rotation angle along the axis", Russian: "угол поворота по оси")]
        [ArgsFormat(English: "{%} relative to the object {%}", Russian: "{%} относительно объекта {%}")]
        public float AngleToObject(Axis axis, Wrapper targetObject)
        {
            var targetTransform = targetObject.GetGameObject()?.transform;

            var angle = 0f;

            if (targetTransform == null)
            {
                return angle;
            }

            var rotationToObject = (transform.rotation * Quaternion.Inverse(targetTransform.rotation)).eulerAngles;

            angle = axis switch
            {
                Axis.X => rotationToObject.x,
                Axis.Y => rotationToObject.y,
                Axis.Z => rotationToObject.z,
                _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
            };

            return angle;
        }

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "Returns the rotation of an object relative to another object as a vector.", Russian: "Возвращает поворот объекта относительно другого объекта в виде вектора.")]
        [Function(English: "rotation relative to the object", Russian: "поворот относительно объекта")]
        public Vector3 RotationToObject(Wrapper targetObject)
        {
            var targetTransform = targetObject.GetGameObject()?.transform;

            return targetTransform == null ? Vector3.zero : (transform.rotation * Quaternion.Inverse(targetTransform.rotation)).eulerAngles;
        }

        #endregion

        #region Events

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "The event is triggered when the object completes any rotation. The rotation is considered complete if the object has reached the rotation to the target point or if the rotation has been stopped by the corresponding block. The object for which the event was triggered is passed to the parameter.", Russian: "Событие срабатывает, когда указанный объект завершает любое вращение. Вращение считается завершенным, если объект достиг поворота к целевой точке, или если вращение было остановлено соответствующим блоком. В параметр передается объект, для которого сработало событие.")]
        [Event(English: "completed any rotation", Russian: "завершил любое вращение")]
        public event CommonRotationHandler OnCommonRotationFinished;

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "The event is triggered when the object has completed the rotation to the target object, or when it has rotated as the target object. The object for which the event was triggered and the object to which the rotation was completed (target object) are passed in parameters.", Russian: "Событие срабатывает, когда объект завершил поворот к целевому объекту, либо когда повернулся так же, как целевой объект. В параметры передается объект, для которого сработало событие, а также объект, к которому был завершен поворот (целевой объект).")]
        [EventGroup("ToWrapperRotation")]
        [Event(English: "completed rotating to the target object", Russian: "завершил поворот к целевому объекту")]
        public event ToWrapperRotationHandler OnToWrapperRotationFinished;

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "The event is triggered when the object has completed the rotation to the target object, or when it has rotated as the target object. The object for which the event was triggered and the object to which the rotation was completed (target object) are passed in parameters.", Russian: "Событие срабатывает, когда объект завершил поворот к целевому объекту, либо когда повернулся так же, как целевой объект. В параметры передается объект, для которого сработало событие, а также объект, к которому был завершен поворот (целевой объект).")]
        [EventGroup("ToWrapperRotation")]
        [Event(English: "completed rotating as the target object", Russian: "повернулся так же как целевой объект")]
        public event ToWrapperRotationHandler OnAsWrapperRotationFinished;

        [LogicGroup(English: "Rotation", Russian: "Вращение")]
        [LogicTooltip(English: "The event is triggered when the object completes the rotation to the target rotation. The object for which the event was triggered (rotating object) and the rotation, as a vector, to which the rotation was completed (target rotation) are passed in parameters.", Russian: "Событие срабатывает, когда объект завершает поворот к целевому вращению. В параметры передается объект, для которого сработало событие (вращающийся объект), а также вращение, в виде вектора, к которому был завершен поворот (целевое вращение).")]
        [Event(English: "completed rotating to the target rotation", Russian: "завершил поворот к целевому вращению")]
        public event ToVectorRotationHandler OnToVectorRotationFinished;

        #endregion

        #region PrivateHelpers
        private static void RotateHierarchy(GameObject gameObjectToRotate, Quaternion qTargetAngle, Transform customAnchor = default)
        {
            var anchor = gameObjectToRotate.transform;
            var anchorPosition = customAnchor == default ? anchor.position : customAnchor.position;

            var hierarchy = GetHierarchy(gameObjectToRotate);

            Debug.DrawLine(anchor.position, anchorPosition, Color.red);

            foreach (var child in hierarchy)
            {
                RotateTo(child.transform, anchorPosition, qTargetAngle);
            }
        }

        private static void RotateTo(Transform transform, Vector3 pivotPoint, Quaternion targetAngle)
        {
            transform.position = Quaternion.Inverse(transform.rotation) * targetAngle * (transform.position - pivotPoint) + pivotPoint;
            transform.rotation = targetAngle;
        }

        private static float GetShortestRotationAngle(float from, float to)
        {
            while (from < 0)
            {
                from += 360;
            }

            while (to < 0)
            {
                to += 360;
            }

            while (from >= 360 - Mathf.Epsilon)
            {
                from -= 360;
            }
            
            while (to >= 360 - Mathf.Epsilon)
            {
                to -= 360;
            }

            var smallAngleTo = from == 0 && Math.Abs(to - 360) < 1;
            var smallAngleFrom = to == 0 && Math.Abs(from - 360) < 1;
            var smallDifference = Math.Abs(from - to) < 1;
            
            if (smallDifference || smallAngleFrom || smallAngleTo)
            {
                return 0;
            }

            var left = 360 - from + to;
            var right = from - to;

            if (from >= to)
            {
                return left <= right ? left : -right;
            }

            if (to > 0)
            {
                left = to - from;
                right = (360 - to) + from;
            }
            else
            {
                left = 360 - to + from;
                right = to - from;
            }

            return left <= right ? left : -right;
        }

        private static float GetRotationFromToTime(Quaternion from, Quaternion to, float speed)
        {
            var startRotationEuler = from.eulerAngles;
            var targetRotationEuler = to.eulerAngles;

            var shortestAngleX = GetShortestRotationAngle(startRotationEuler.x, targetRotationEuler.x);
            var shortestAngleY = GetShortestRotationAngle(startRotationEuler.y, targetRotationEuler.y);
            var shortestAngleZ = GetShortestRotationAngle(startRotationEuler.z, targetRotationEuler.z);

            return Mathf.Max(Mathf.Max(Mathf.Abs(shortestAngleX), Mathf.Abs(shortestAngleY)), Mathf.Abs(shortestAngleZ)) / speed;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        #endregion
    }
}