using System.Collections.Generic;
using System.Linq;
using ExampleApp.Data;

namespace ExampleApp.Commands
{
    public class SpawnCommand : Command
    {
        private readonly List<SpawnInitParams> _spawnInitParams = new List<SpawnInitParams>();

        /// <summary>
        /// Spawn group objects command
        /// </summary>
        public SpawnCommand(List<SpawnInitParams> spawnInitParams)
        {
            foreach (var sp in spawnInitParams)
            {
                _spawnInitParams.Add(sp);
            }
            
            CommandsManager.AddCommand(this);
        }

        /// <summary>
        /// Spawn object command
        /// </summary>
        public SpawnCommand(SpawnInitParams spawnInitParams)
        {
            _spawnInitParams.Add(spawnInitParams);
            CommandsManager.AddCommand(this);
        }

        protected override void Execute()
        {
            if (SpawnInitParams != null)
            {
                SpawnObjects(SpawnInitParams.Values.ToList());
            }
            else
            {
                SpawnObjects(_spawnInitParams);
            }
        }

        private void SpawnObjects(List<SpawnInitParams> spawnInitParams)
        {
            foreach (var spawnInitParam in spawnInitParams)
            {
                Spawner.Instance.SpawnAsset(spawnInitParam);
                SaveObject(spawnInitParam);
            }
            
            ProjectData.ObjectsAreChanged = true;
        }

        protected override void Undo()
        {
            var objects = GetObjects();

            SaveObjects(objects);
            foreach (var oc in objects)
            {
                oc?.Delete();
            }
            
            ProjectData.ObjectsAreChanged = true;
        }
    }
}
