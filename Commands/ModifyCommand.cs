using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using ExampleApp.Data.ServerData;
using ExampleApp.Models.Data;
using ExampleApp.Public;

namespace ExampleApp.Commands
{
    public class ModifyCommand : Command
    {
        private readonly Dictionary<int, TransformDT> _previousTransformState;
        private readonly Dictionary<int, TransformDT> _newTransformState;
        private readonly Action<ObjectController> _callback;
        private readonly JointData _saveJointData;
        
        public ModifyCommand(
            List<ObjectController> objectControllers, 
            Dictionary<int, TransformDT> previousTransformState, 
            Dictionary<int, TransformDT> newTransformState, 
            JointData jointData = null, 
            Action<ObjectController> callback = null,
            bool addCommand = true
        )
        {
            _previousTransformState = previousTransformState;
            _newTransformState = newTransformState;
            _callback = callback;
            _saveJointData = jointData;
            _callback = callback;
            SaveObjects(objectControllers);
            if (addCommand)
            {
                CommandsManager.AddCommand(this);
            }
        }

        protected override void Execute()
        {
            ExecuteInternal(_newTransformState);
        }

        protected override void Undo()
        {
            ExecuteInternal(_previousTransformState);
        }

        private void ExecuteInternal(Dictionary<int, TransformDT> transformDTs)
        {
            var allObjects = GetObjects();

            foreach (ObjectController objectController in allObjects)
            {
                if (objectController == null)
                {
                    continue;
                }

                if (!objectController.gameObject)
                {
                    continue;
                }

                Transform affectedTransform = objectController.GetAffectedTransform();
                transformDTs[objectController.Id].ToLocalTransformUnity(affectedTransform);

                objectController.UpdateTransforms();
            }
            
            ProjectData.ObjectsAreChanged = true;
        }
    }
}