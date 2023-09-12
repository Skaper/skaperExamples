using System;
using UnityEngine;

namespace ExampleApp.Core.Behaviours
{
    public class ExampleAppBehaviourContainer
    {
        public readonly Type BehaviourType;
        private readonly ExampleAppBehaviourHelper _behaviourHelper;

        public ExampleAppBehaviourContainer(Type behaviourType) : this(behaviourType, new ExampleAppBehaviourHelper())
        {
        }
        
        public ExampleAppBehaviourContainer(Type behaviourType, ExampleAppBehaviourHelper behaviourHelper)
        {
            BehaviourType = behaviourType;
            _behaviourHelper = behaviourHelper;
        }

        public bool CanAddBehaviour(GameObject gameObject)
        {
            return _behaviourHelper.CanAddBehaviour(gameObject, BehaviourType);
        }
    }
}
