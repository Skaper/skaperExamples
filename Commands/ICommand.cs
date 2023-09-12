using System.Collections.Generic;
using ExampleApp.Data;

namespace ExampleApp.Commands
{
    public interface ICommand
    {
        void Execute();

        void Undo();
    }

    public abstract class Command : ICommand
    {
        private List<int> _saveId = new List<int>();
        private List<int> _saveIdGroup = new List<int>();
        protected Dictionary<int, SpawnInitParams> SpawnInitParams;

        public bool Executed { get; private set; }
        public bool Undid { get; private set; }
        protected abstract void Execute();

        protected abstract void Undo();

        void ICommand.Execute()
        {
            Execute();
            Executed = true;
            Undid = false;
        }

        void ICommand.Undo()
        {
            Undo();
            Executed = false;
            Undid = true;
        }

        protected void SaveObjects(List<ObjectController> o)
        {
            SpawnInitParams = new Dictionary<int, SpawnInitParams>();
            
            foreach (var oc in o)
            {
                SaveId(oc.Id, oc.IdScene);

                if (oc.RootGameObject)
                {
                    var baseTypes = oc.RootGameObject.GetComponentsInChildren<ObjectBehaviourWrapper>();
                    foreach (var wrapper in baseTypes)
                    {
                        ObjectController objectController = wrapper.OwdObjectController;
                        SpawnInitParams.Add(objectController.Id, objectController.GetSpawnInitParams());
                    }
                }
            }
        }

        protected void SaveObject(SpawnInitParams param)
        {
            if (param.IdInstance != 0)
            {
                SaveId(param.IdInstance, param.IdScene);
            }
            else if (param.IdScene != 0)
            {
                SaveId(GameStateData.GetNextObjectIdInScene(), param.IdScene);
            }
        }

        private void SaveId(int idInstance, int idScene)
        {
            if (!_saveId.Contains(idInstance))
            {
                _saveId.Add(idInstance);
                _saveIdGroup.Add(idScene);
            }
        }
        
        public List<ObjectController> GetObjects()
        {
            var returnObjectControllers = new List<ObjectController>();

            for (int i = 0; i < _saveIdGroup.Count; i++)
            {
                if (_saveIdGroup[i] == 0)
                {
                    continue;
                }

                var obj = GameStateData.GetObjectControllerInSceneById(_saveId[i]);
                if (obj != null)
                {
                    returnObjectControllers.Add(obj);
                }
            }

            return returnObjectControllers.Count > 0 ? returnObjectControllers : null;
        }
    }
}