using UnityEngine;
using ExampleApp.PlatformAdapter;
using ExampleApp.Public;
using System;
using System.Linq;

namespace ExampleApp.Core.Behaviours.ConstructorLib
{
    public class InteractionBehaviourHelper : ExampleAppBehaviourHelper
    {
        public override bool CanAddBehaviour(GameObject gameObject, Type behaviourType)
        {
            if (!base.CanAddBehaviour(gameObject, behaviourType))
            {
                return false;
            }

            return !gameObject.GetComponentInChildren<JointPoint>();
        }
    }
    
    [RequireComponent(typeof(InteractableObjectBehaviour))]
    [RequireComponentInChildren(typeof(Rigidbody))]
    [RequireComponentInChildren(typeof(Collider))]
    [ExampleAppComponent(English: "Interactivity", Russian: "Интерактивность")]
    public class InteractionBehaviour : ConstructorExampleAppBehaviour, IGrabStartAware, IGrabEndAware, IUseStartAware, IUseEndAware, ITouchStartAware, ITouchEndAware
    {
        private bool _isGrabbed;
        private bool _isUsed;
        private bool _isTouched;
        private bool _isTeleportArea;
        
        private ControllerInteraction.ControllerHand _grabbedHand;
        private ControllerInteraction.ControllerHand _usedHand;

        private const string TeleportAbleTag = "TeleportArea";
        private const string NotTeleportAbleTag = "NotTeleport";
        
        private InteractableObjectBehaviour _interactableObject;
        public InteractableObjectBehaviour InteractableObject
        {
            get
            {
                if (_interactableObject)
                {
                    return _interactableObject;
                }
                
                _interactableObject = gameObject.GetComponent<InteractableObjectBehaviour>();
                return _interactableObject;
            }
        }

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            _isTeleportArea = transform.Cast<Transform>().Any(child => child.tag.Equals(TeleportAbleTag));
        }

        #region Enums

        public enum Teleportation
        {
            [Item(English: "can be teleported to", Russian: "можно телепортироваться")]
            TeleportEnable,
            [Item(English: "can’t be teleported to", Russian: "нельзя телепортироваться")]
            TeleportDisable,
        }
        
        public enum GrabStatus
        {
            [Item(English: "was grabbed", Russian: "взят в руку")]
            Grabbed,
            [Item(English: "was ungrabbed", Russian: "выпущена из руки")]
            Ungrabbed,
        }
        
        public enum Grabbing
        {
            [Item(English: "can be grabbed", Russian: "можно взять в руку")]
            GrabEnable,
            [Item(English: "can’t be grabbed", Russian: "нельзя взять в руку")]
            GrabDisable,
        }
        
        public enum UseStatus
        {
            [Item(English: "started being used", Russian: "начал использоваться")]
            Used,
            [Item(English: "ended being used", Russian: "перестал использоваться")]
            UnUsed,
        }
        
        public enum Using
        {
            [Item(English: "can be used", Russian: "можно использовать")]
            UseEnable,
            [Item(English: "can’t be used", Russian: "нельзя использовать")]
            UseDisable,
        }
        
        public enum Touching
        {
            [Item(English: "can be touched", Russian: "можно дотронуться")]
            TouchEnable,
            [Item(English: "can’t be touched", Russian: "нельзя дотронуться")]
            TouchDisable,
        }

        #endregion

        #region Checkers

        [LogicGroup(English: "Interactivity", Russian: "Интерактивность")]
        [LogicTooltip(English: "Returns true if the specified object is currently in the hand. Otherwise it returns false.", Russian: "Возвращает истину, если объект находится в руке в данный момент. В противном случае возвращает ложь.")]
        [Checker(English: "is in the player's hand", Russian: "находится в руке игрока")]
        public bool IsGrabbed()
        {
            return _isGrabbed;
        }
        
        [LogicGroup(English: "Interactivity", Russian: "Интерактивность")]
        [LogicTooltip(English: "Returns true if the specified object is currently used by the player. Otherwise it returns false.", Russian: "Возвращает истину, если указанный объект используется игроком в данный момент. В противном случае возвращает ложь.")]
        [Checker(English: "is used by the player", Russian: "используется игроком")]
        public bool IsUsing()
        {
            return _isUsed;
        }
        
        [LogicGroup(English: "Interactivity", Russian: "Интерактивность")]
        [LogicTooltip(English: "Returns true if the object is touched at the moment. Otherwise it returns false.", Russian: "Возвращает истину, если до объекта дотрагиваются в данный момент. В противном случае возвращает ложь.")]
        [Checker(English: "touches the player's hand", Russian: "касается руки игрока")]
        public bool IsTouching()
        {
            return _isTouched;
        }

        #endregion
        
        #region Variables

        [LogicGroup(English: "Interactivity", Russian: "Интерактивность")]
        [LogicTooltip(English: "Sets whether the player can teleport or walk around the object.", Russian: "Задает, можно ли игроку телепортироваться или ходить по объекту.")]
        [Variable(English: "", Russian: "на объект")]
        public Teleportation TeleportationSetter
        {
            set
            {
                _isTeleportArea = value == Teleportation.TeleportEnable;
                var targetTag = value == Teleportation.TeleportEnable ? TeleportAbleTag : NotTeleportAbleTag;
                transform.tag = targetTag;
                foreach (Transform child in transform)
                {
                    child.tag = targetTag;
                }
            }
        }

        [LogicGroup(English: "Interactivity", Russian: "Интерактивность")]
        [LogicTooltip(English: "Sets whether the player can grab  the object.", Russian: "Задает, можно ли игроку брать объект в руки.")]
        [Variable(English: "", Russian: "")]
        public Grabbing GrabbingSetter 
        {
            set => InteractableObject.IsGrabbable = value == Grabbing.GrabEnable;
        }
        
        [LogicGroup(English: "Interactivity", Russian: "Интерактивность")]
        [LogicTooltip(English: "Sets whether or not the player can interact with an object using the mechanics of using it (clicking on the object).", Russian: "Задает, можно ли игроку взаимодействовать с объектом с помощью механики использования (нажатия на объект).")]
        [Variable(English: "", Russian: "")]
        public Using UsingSetter 
        {
            set => InteractableObject.IsUsable = value == Using.UseEnable;
        }
        
        [LogicGroup(English: "Interactivity", Russian: "Интерактивность")]
        [LogicTooltip(English: "Sets whether the player can interact with an object using touch mechanics.", Russian: "Задает, можно ли игроку взаимодействовать с объектом с помощью механики касания.")]
        [Variable(English: "", Russian: "")]
        public Touching TouchingSetter 
        {
            set => InteractableObject.IsTouchable = value == Touching.TouchEnable;
        }
        
        [ExampleAppInspector(English: "Сan teleport", Russian: "Можно телепортироваться")]
        public bool TeleportationInspector
        {
            get => _isTeleportArea;
            set => TeleportationSetter = value ? Teleportation.TeleportEnable : Teleportation.TeleportDisable;
        }
        
        [ExampleAppInspector(English: "Grabbable", Russian: "Можно брать в руку")]
        public bool GrabbableInspector
        {
            get => InteractableObject.IsGrabbable;
            set => InteractableObject.IsGrabbable = value;
        }

        [ExampleAppInspector(English: "Usable", Russian: "Можно использовать")]
        public bool UsableInspector
        {
            get => InteractableObject.IsUsable;
            set => InteractableObject.IsUsable = value;
        }

        [ExampleAppInspector(English: "Touchable", Russian: "Можно дотронуться")]
        public bool TouchableInspector
        {
            get => InteractableObject.IsTouchable;
            set => InteractableObject.IsTouchable = value;
        }

        #endregion

        #region Events

        public delegate void HandInteractionHandler([Parameter(English: "interaction hand", Russian: "рука взаимодействия")] ControllerInteraction.ControllerHand hand);

        [LogicGroup(English: "Interactivity", Russian: "Интерактивность")]
        [LogicTooltip(English: "The event is triggered when the player takes the specified object in his hand. The parameters include the object and the hand it was taken with.", Russian: "Событие срабатывает, когда игрок берет в руку указанный объект. В параметры передается объект и рука, которой он был взят.")]
        [EventGroup("Grab")]
        [Event(English: "was grabbed", Russian: "взят в руку")]
        public event HandInteractionHandler OnObjectGrabbed;
        
        [LogicGroup(English: "Interactivity", Russian: "Интерактивность")]
        [LogicTooltip(English: "The event is triggered when the player takes the specified object in his hand. The parameters include the object and the hand it was taken with.", Russian: "Событие срабатывает, когда игрок берет в руку указанный объект. В параметры передается объект и рука, которой он был взят.")]
        [EventGroup("Grab")]
        [Event(English: "was ungrabbed", Russian: "выпущен из руки")]
        public event HandInteractionHandler OnObjectUnGrabbed;
        
        [LogicGroup(English: "Interactivity", Russian: "Интерактивность")]
        [LogicTooltip(English: "The event is triggered when the player takes the specified object in his hand. The parameters include the object and the hand it was taken with.", Russian: "Событие срабатывает, когда игрок берет в руку указанный объект. В параметры передается объект и рука, которой он был взят.")]
        [EventGroup("Use")]
        [Event(English: "started being used", Russian: "начал использоваться")]
        public event HandInteractionHandler OnObjectUsed;
        
        [LogicGroup(English: "Interactivity", Russian: "Интерактивность")]
        [LogicTooltip(English: "The event is triggered when the player takes the specified object in his hand. The parameters include the object and the hand it was taken with.", Russian: "Событие срабатывает, когда игрок берет в руку указанный объект. В параметры передается объект и рука, которой он был взят.")]
        [EventGroup("Use")]
        [Event(English: "ended being used", Russian: "перестал использоваться")]
        public event HandInteractionHandler OnObjectNotUsed; 
        
        #endregion
        
        public void OnGrabStart(GrabingContext context)
        {
            if (context.Hand is ControllerInteraction.ControllerHand.Left or ControllerInteraction.ControllerHand.Right)
            {
                _isGrabbed = true;
                _grabbedHand = context.Hand;
                OnObjectGrabbed?.Invoke(_grabbedHand);
            }
        }

        public void OnGrabEnd()
        {
            _isGrabbed = false;
            OnObjectUnGrabbed?.Invoke(_grabbedHand);
            _grabbedHand = default;
        }

        public void OnUseStart(UsingContext context)
        {
            if (context.Hand is ControllerInteraction.ControllerHand.Left or ControllerInteraction.ControllerHand.Right)
            {
                _isUsed = true;
                _usedHand = context.Hand;
                OnObjectUsed?.Invoke(context.Hand);
            }
        }

        public void OnUseEnd()
        {
            _isUsed = false;
            OnObjectNotUsed?.Invoke(_usedHand);
            _usedHand = default;
        }

        public void OnTouchStart()
        {
            _isTouched = true;
        }

        public void OnTouchEnd()
        {
            _isTouched = false;
        }
    }
}
