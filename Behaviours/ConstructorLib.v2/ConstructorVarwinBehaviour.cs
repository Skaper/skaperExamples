using System.Collections.Generic;
using UnityEngine;
using ExampleApp.Public;

namespace ExampleApp.Core.Behaviours.ConstructorLib
{
    public abstract class ConstructorExampleAppBehaviour : ExampleAppBehaviour
    {
        protected  readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
        public enum Axis
        {
            [Item("X")] X,
            [Item("Y")] Y,
            [Item("Z")] Z,
        }
        
        protected class BehaviourState
        {
            public bool IsPaused;
            public bool IsMoving;
            public bool IsRotating;
            public bool IsScaling;
            public bool IsPerforming;
        }
        
        protected static List<Transform> FillLockedHierarchyList(ObjectController highestLocked, List<Transform> transformsToMove)
        {
            transformsToMove.Add(highestLocked.gameObject.transform);

            foreach (var objectController in highestLocked.Children)
            {
                transformsToMove = FillLockedHierarchyList(objectController, transformsToMove);
            }

            return transformsToMove;
        }
        
        protected static Vector3 EnumToVector(Axis axisDirection)
        {
            return axisDirection switch
            {
                Axis.X => Vector3.right,
                Axis.Y => Vector3.up,
                Axis.Z => Vector3.forward,
                _ => Vector3.zero
            };
        }
        
        protected static List<Transform> GetHierarchy(GameObject objInHierarchy)
        {
            var lockedInHierarchyObjects = new List<Transform>();

            var objectController = objInHierarchy.GetWrapper()?.GetObjectController();

            if (objectController is {LockChildren: true})
            {
                var highestLockedParent = objectController;

                while (highestLockedParent.Parent && highestLockedParent.Parent.LockChildren)
                {
                    highestLockedParent = highestLockedParent.Parent;
                }

                lockedInHierarchyObjects = FillLockedHierarchyList(highestLockedParent, lockedInHierarchyObjects);
            }
            else
            {
                lockedInHierarchyObjects.Add(objInHierarchy.transform);
            }

            return lockedInHierarchyObjects;
        }
    }
}