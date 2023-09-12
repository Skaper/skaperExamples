using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExampleApp.Public;

namespace ExampleApp.Core.Behaviours.ConstructorLib
{
    [ExampleAppComponent(English: "Scale", Russian: "Масштабирование")]
    public class ScaleBehaviour : ConstructorExampleAppBehaviour
    {
        private readonly List<int> _currentScaleRoutines = new();
        private int _routineId;
        private readonly BehaviourState _currentState = new();
        
        public delegate void CommonScaleHandler();

        #region Actions

        [LogicGroup(English: "Scale", Russian: "Масштабирование")]
        [LogicTooltip(English: "Instantly sets the scale of the specified object.", Russian: "Мгновенно задает масштаб указанного объекта.")]
        [Action(English: "set scale to", Russian: "задать масштаб")]
        public void SetScale(Vector3 targetScale)
        {
            var currentScale = transform.localScale;
            ScaleHierarchy(gameObject, new Vector3(targetScale.x / currentScale.x, targetScale.y / currentScale.y, targetScale.z / currentScale.z));
        }

        [LogicGroup(English: "Scale", Russian: "Масштабирование")]
        [LogicTooltip(English: "Scales the object to the specified values within the specified time. The change is relative to the current (at the time the block is triggered) scale of the object.", Russian: "Масштабирует объект до заданных значений в течение указанного времени. Изменение происходит относительно текущего (на момент срабатывания блока)  масштаба объекта.")]
        [Action(English: "scale up to", Russian: "масштабировать до")]
        [ArgsFormat(English: "{%} for {%} s", Russian: "{%} в течение {%} с")]
        public IEnumerator ScaleUpToForTime(Vector3 targetScale, float duration)
        {
            var routineId = _routineId++;
            _currentScaleRoutines.Add(routineId);

            _currentState.IsScaling = true;

            var velocity = (targetScale - transform.localScale) / duration;

            var scalingTime = 0f;

            while (scalingTime <= duration && _currentState.IsScaling)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                SetScale(transform.localScale + velocity * Time.deltaTime);

                scalingTime += Time.deltaTime;
                yield return WaitForEndOfFrame;
            }

            _currentScaleRoutines.Remove(routineId);

            OnCommonScalingFinished?.Invoke();
        }

        [LogicGroup(English: "Scale", Russian: "Масштабирование")]
        [LogicTooltip(English: "Scales the object by the specified number of times within the specified time. The change is relative to the current (at the time the block is triggered) scale of the object.", Russian: "Масштабирует объект в заданное количество раз в течение указанного времени. Изменение происходит относительно текущего (на момент срабатывания блока)  масштаба объекта.")]
        [Action(English: "scale by a factor of", Russian: "масштабировать в")]
        [ArgsFormat(English: "{%} for {%} s", Russian: "{%} раз в течение {%} с")]
        public IEnumerator ScaleByAFactorForTime(float scaleFactor, float duration)
        {
            var routineId = _routineId++;
            _currentScaleRoutines.Add(routineId);

            _currentState.IsScaling = true;

            var currentScale = transform.localScale;

            var targetScale = currentScale * scaleFactor;

            var velocity = (targetScale - currentScale) / duration;

            var scalingTime = 0f;

            while (scalingTime <= duration && _currentState.IsScaling)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                SetScale(transform.localScale + velocity * Time.deltaTime);

                scalingTime += Time.deltaTime;
                yield return WaitForEndOfFrame;
            }

            _currentScaleRoutines.Remove(routineId);

            OnCommonScalingFinished?.Invoke();
        }

        [LogicGroup(English: "Scale", Russian: "Масштабирование")]
        [LogicTooltip(English: "Controls any scaling. A paused scaling can be continued with the  “Continue” block.", Russian: "Управляет любым масштабированием. Приостановленное масштабирование можно возобновить блоком “Продолжить”.")]
        [ActionGroup("ScaleControl")]
        [Action(English: "stop any scaling", Russian: "завершить любое масштабирование")]
        public void StopAnyScaling()
        {
            _currentState.IsScaling = false;
            StopAllCoroutines();
        }

        [LogicGroup(English: "Scale", Russian: "Масштабирование")]
        [LogicTooltip(English: "Controls any scaling. A paused scaling can be continued with the  “Continue” block.", Russian: "Управляет любым масштабированием. Приостановленное масштабирование можно возобновить блоком “Продолжить”.")]
        [ActionGroup("ScaleControl")]
        [Action(English: "pause any scaling", Russian: "приостановить любое масштабирование")]
        public void PauseAnyScaling()
        {
            _currentState.IsPaused = true;
        }

        [LogicGroup(English: "Scale", Russian: "Масштабирование")]
        [LogicTooltip(English: "Controls any scaling. A paused scaling can be continued with the  “Continue” block.", Russian: "Управляет любым масштабированием. Приостановленное масштабирование можно возобновить блоком “Продолжить”.")]
        [ActionGroup("ScaleControl")]
        [Action(English: "continue any scaling", Russian: "продолжить любое масштабирование")]
        public void ContinueAnyScaling()
        {
            _currentState.IsPaused = false;
        }

        #endregion

        #region Checkers

        [LogicGroup(English: "Scale", Russian: "Масштабирование")]
        [LogicTooltip(English: "Returns “true” if the object is currently scaled. Otherwise it returns “false”.", Russian: "Возвращает “истину”, если объект масштабируется в данный момент. В противном случае возвращает “ложь”.")]
        [Checker(English: "is scaling at the moment", Russian: "масштабируется в данный момент")]
        public bool IsScalingNow()
        {
            return _currentScaleRoutines.Count > 0;
        }

        #endregion

        #region Events

        [LogicGroup(English: "Scale", Russian: "Масштабирование")]
        [LogicTooltip(English: "The event is triggered when the specified object completes any scaling. The rotation is considered complete if the object has reached the target scale or if the scaling was stopped by the corresponding block. The object for which the event was triggered is passed to the parameter.", Russian: "Событие срабатывает, когда указанный объект завершает любое масштабирование. Вращение считается завершенным, если объект достиг целевого масштаба, или если масштабирование было остановлено соответствующим блоком. В параметр передается объект, для которого сработало событие. ")]
        [Event(English: "completed any scaling", Russian: "завершил любое масштабирование")]
        public event CommonScaleHandler OnCommonScalingFinished;

        #endregion

        #region Variables

        [LogicGroup(English: "Scale", Russian: "Масштабирование")]
        [LogicTooltip(English: "Returns the scale of the specified object along the selected axis in world coordinates.", Russian: "Возвращает масштаб указанного объекта по выбранной оси в мировых координатах.")]
        [VariableGroup("AxisScale")]
        [Variable(English: "scale on the axis X", Russian: "масштаб по оси X")]
        public float ScaleX => transform.localScale.x;

        [LogicGroup(English: "Scale", Russian: "Масштабирование")]
        [LogicTooltip(English: "Returns the scale of the specified object along the selected axis in world coordinates.", Russian: "Возвращает масштаб указанного объекта по выбранной оси в мировых координатах.")]
        [VariableGroup("AxisScale")]
        [Variable(English: "scale on the axis Y", Russian: "масштаб по оси Y")]
        public float ScaleY => transform.localScale.y;

        [LogicGroup(English: "Scale", Russian: "Масштабирование")]
        [LogicTooltip(English: "Returns the scale of the specified object along the selected axis in world coordinates.", Russian: "Возвращает масштаб указанного объекта по выбранной оси в мировых координатах.")]
        [VariableGroup("AxisScale")]
        [Variable(English: "scale on the axis Z", Russian: "масштаб по оси Z")]
        public float ScaleZ => transform.localScale.z;

        [LogicGroup(English: "Scale", Russian: "Масштабирование")]
        [LogicTooltip(English: "Returns the scale of the specified object in world coordinates as a vector.", Russian: "Возвращает масштаб указанного объекта в мировых координатах в виде вектора.")]
        [Variable(English: "scale", Russian: "масштаб")]
        public Vector3 Scale => transform.localScale;

        #endregion

        #region PrivateHelpers

        private static void ScaleHierarchy(GameObject gameObjectToScale, Vector3 scaleDelta)
        {
            foreach (var objectToScaleTransform in GetHierarchy(gameObjectToScale))
            {
                objectToScaleTransform.localScale = Vector3.Scale(objectToScaleTransform.localScale, scaleDelta);
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        #endregion
    }
}