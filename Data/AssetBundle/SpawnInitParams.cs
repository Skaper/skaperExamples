using System;
using System.Collections.Generic;
using ExampleApp.Data.ServerData;
using ExampleApp.Models.Data;

namespace ExampleApp.Data
{
    /// <summary>
    /// Initialization parameters for object spawn
    /// </summary>
    public class SpawnInitParams : IJsonSerializable
    {
        /// <summary>
        /// Object type
        /// </summary>
        public int IdObject;

        /// <summary>
        /// Object name
        /// </summary>
        public string Name;

        /// <summary>
        /// Scene Id
        /// </summary>
        public int IdScene;
        
        /// <summary>
        /// Photon Id
        /// </summary>
        public int IdPhoton;

        /// <summary>
        /// Object transform
        /// </summary>
        public Dictionary<int, TransformDT> Transforms;

        /// <summary>
        /// Joints data
        /// </summary>
        public JointData Joints;

        /// <summary>
        /// Inspector properties data
        /// </summary>
        public List<InspectorPropertyData> InspectorPropertiesData;

        /// <summary>
        /// Object index in hierarchy
        /// </summary>
        /// TODO: Переделать на ulong
        public int Index = -1;
        
        /// <summary>
        /// Is hierarchy locked in play mode?
        /// </summary>
        public bool LockChildren;

        /// <summary>
        /// Is this object disabled?
        /// </summary>
        public bool IsDisabled;

        /// <summary>
        /// Is this object disabled in hierarchy?
        /// </summary>
        public bool IsDisabledInHierarchy;
        
        /// <summary>
        /// If true it allows to select object only from hierarchy 
        /// </summary>
        public bool DisableSelectabilityInEditor;
        
        /// <summary>
        /// Hide this object instance in Blockly Editor
        /// </summary>
        public bool DisableSceneLogic;
        
        /// <summary>
        /// Object instance Id (if 0, id will be set automatically on server)
        /// </summary>
        /// TODO: Переделать на ulong
        public int IdInstance = 0;

        /// <summary>
        /// Object server Id (if 0, no such object exists on server)
        /// </summary>
        /// TODO: Переделать на ulong
        public int IdServer = 0;

        /// <summary>
        /// Object parent Id
        /// </summary>
        /// TODO: Переделать на ulong
        public int? ParentId;

        /// <summary>
        /// Is object embedded?
        /// </summary>
        public bool Embedded;

        /// <summary>
        /// Is object a part of a scene template?
        /// </summary>
        public bool SceneTemplateObject;
        
        /// <summary>
        /// Is a silent internal spawn?
        /// </summary>
        public bool InternalSpawn = false;

        /// <summary>
        /// Guid that generates on spawn request
        /// </summary>
        [NonSerialized]
        public string SpawnGuid;
    }
}
