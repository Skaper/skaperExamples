using System.Collections.Generic;
using ExampleApp.Data;

namespace ExampleApp.Commands
{
    public class DeleteCommand : Command
    {
        private List<SpawnInitParams> _spawnInitParams = new List<SpawnInitParams>();
        
        public DeleteCommand(List<ObjectController> controllers)
        {
            SaveObjects(controllers);

            foreach (var objectController in controllers)
            {
                _spawnInitParams.Add(objectController.GetSpawnInitParams());
            }

            CommandsManager.AddCommand(this);
        }

        protected override void Execute()
        {
            foreach (var oc in GetObjects())
            {
                oc?.Delete();
            }
            
            ProjectData.ObjectsAreChanged = true;
        }

        protected override void Undo()
        {
            foreach (var spawnInitParams in _spawnInitParams)
            {
                Spawner.Instance.SpawnAsset(spawnInitParams);
            }
            
            ProjectData.ObjectsAreChanged = true;
        }
    }
}
