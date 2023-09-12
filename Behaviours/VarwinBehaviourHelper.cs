using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ExampleApp.Public;

namespace ExampleApp.Core.Behaviours
{
    public class ExampleAppBehaviourHelper
    {
        public virtual bool CanAddBehaviour(GameObject gameObject, Type behaviourType)
        {
            IEnumerable<Type> requiredComponentTypes = Attribute.GetCustomAttributes(behaviourType, typeof(RequireComponentInChildrenAttribute))
                .OfType<RequireComponentInChildrenAttribute>().Select(x => x.RequiredComponent);

            var protectedComponentTypes = new List<Type>();

            foreach (var objectBehaviour in gameObject.GetComponentsInChildren<MonoBehaviour>())
            {
                protectedComponentTypes.AddRange(Attribute.GetCustomAttributes(objectBehaviour.GetType(), typeof(ProtectComponentAttribute))
                    .OfType<ProtectComponentAttribute>().Select(x => x.ProtectedComponent));
            }
            
            foreach (var type in requiredComponentTypes)
            {
                if (gameObject.GetComponentsInChildren(type).Length == 0)
                {
                    return false;
                }
                
                if (protectedComponentTypes.Contains(type))
                {
                    return false;
                }
            }

            if (gameObject.GetComponentInChildren<ExampleAppBot>())
            {
                return false;
            }

            return !gameObject.GetComponent(behaviourType);
        }
    }
}
