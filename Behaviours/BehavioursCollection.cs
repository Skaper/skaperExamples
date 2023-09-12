using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ExampleApp.Core.Behaviours.ConstructorLib;

namespace ExampleApp.Core.Behaviours
{
    public static class BehavioursCollection
    {
        private static HashSet<ExampleAppBehaviourContainer> _ExampleAppBehaviourContainers;

        private static HashSet<ExampleAppBehaviourContainer> BehaviourContainers
        {
            get
            {
                if (_ExampleAppBehaviourContainers == null)
                {
                    _ExampleAppBehaviourContainers = new HashSet<ExampleAppBehaviourContainer>
                    {
                        // v1
                        new(typeof(MaterialChangeBehaviour), new MaterialChangeBehaviourHelper()),
                        new(typeof(MovableBehaviour)),
                        new(typeof(ScalableBehaviour)),
                        new(typeof(InteractableBehaviour), new InteractableBehaviourHelper()),
                        new(typeof(LightBehaviour)),
                        // v2
                        new(typeof(RotateBehaviour)),
                        new(typeof(PhysicsBehaviour)),
                        new(typeof(ScaleBehaviour)),
                        new(typeof(MotionBehaviour)),
                        new(typeof(VisualizationBehaviour), new VisualizationBehaviourHelper()),
                        new(typeof(InteractionBehaviour), new InteractionBehaviourHelper()),
                    };
                }

                return _ExampleAppBehaviourContainers;
            }
        }

        public static List<System.Type> GetAllBehavioursTypes() => BehaviourContainers.Select(x => x.BehaviourType).ToList();

        public static List<string> GetBehaviours(ObjectController objectController)
        {
            var behaviours = new List<string>();

            foreach (var behaviourContainer in BehaviourContainers)
            {
                var behaviour = objectController.RootGameObject.GetComponentInChildren(behaviourContainer.BehaviourType) as ExampleAppBehaviour;
                if (behaviour == null)
                {
                    continue;
                }
                
                var wrapper = objectController.WrappersCollection.Get(objectController.Id);
                wrapper.AddBehaviour(behaviourContainer.BehaviourType, behaviour);
                behaviours.Add(behaviourContainer.BehaviourType.FullName);
            }

            return behaviours;
        }

        public static List<string> AddBehaviours(ObjectController objectController)
        {
            var behaviours = new List<string>();
            if (CanAddBehaviours(objectController))
            {
                AddBehaviours(objectController.RootGameObject, objectController.WrappersCollection.Get(objectController.Id), behaviours);
            }

            return behaviours;
        }

        public static bool IsAnOldTypeEnumBehaviour(Type behaviourType)
        {
            if(BehaviourContainers.FirstOrDefault(x => x.BehaviourType == behaviourType) == null)
            {
                return false;
            }

            return behaviourType == typeof(MaterialChangeBehaviour)
                   || behaviourType == typeof(MovableBehaviour)
                   || behaviourType == typeof(ScalableBehaviour)
                   || behaviourType == typeof(InteractableBehaviour)
                   || behaviourType == typeof(LightBehaviour)
                   || behaviourType == typeof(ScalableBehaviour)
                   || behaviourType == typeof(MaterialChangeBehaviourHelper)
                   || behaviourType == typeof(InteractableBehaviourHelper);
        }

        private static void AddBehaviours(GameObject gameObject, Wrapper wrapper, List<string> behaviours)
        {
            foreach (var behaviourContainer in BehaviourContainers)
            {
                if (behaviourContainer.CanAddBehaviour(gameObject))
                {
                    var behaviour = (ExampleAppBehaviour) gameObject.AddComponent(behaviourContainer.BehaviourType);
                    wrapper.AddBehaviour(behaviourContainer.BehaviourType, behaviour);
                    behaviours.Add(behaviourContainer.BehaviourType.FullName);
                }
            }
        }

        private static bool CanAddBehaviours(ObjectController objectController)
        {
            return !(objectController.IsEmbedded
                     || objectController.IsSceneTemplateObject
                     || !objectController.ExampleAppObjectDescriptor
                     || !objectController.ExampleAppObjectDescriptor.AddBehavioursAtRuntime
                     || objectController.ExampleAppObjectDescriptor.Components.ComponentReferences.Any(x => x.Type.ToString().Contains("ExampleApp.ConstructorLib.v1")));
        }
    }
}

