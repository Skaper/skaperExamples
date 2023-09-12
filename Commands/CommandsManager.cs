using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace ExampleApp.Commands
{
    public static class CommandsManager
    {
        private static LinkedList<Command> _commands = new LinkedList<Command>();
        private static LinkedListNode<Command> _current;
        public static event Action HistoryChanged;
        public static event Action OnUndo;
        public static event Action OnRedo;

        public static Command Current => _current?.Value;
        
        public static List<Command> CommandList => _commands.ToList();

        public static void AddCommand(Command newCommand)
        {
            if (_current == null)
            {
                _current = new LinkedListNode<Command>(newCommand);
                _commands.AddFirst(_current);
            }
            else
            {
                if (_current.Next != null)
                {
                    RemoveAfter(_current);
                }
                
                if (_current.Value.Undid)
                {
                    RemoveUndid();
                }
                
                if (_current == null)
                {
                    _current = new LinkedListNode<Command>(newCommand);
                    _commands.AddFirst(_current);
                }
                else
                {
                    _commands.AddAfter(_current, newCommand);
                    _current = _commands.Last;
                }
            }

            HistoryChanged?.Invoke();
        }
        
        private static void RemoveUndid()
        {
            while (_current != null && _current.Value.Undid)
            {
                _commands.RemoveLast();
                _current = _commands.Last;
            }
            
            HistoryChanged?.Invoke();
        }
        
        private static void RemoveAfter(LinkedListNode<Command> currentNext)
        {
            LinkedListNode<Command> cur = _commands.Last;
            while (cur != currentNext)
            {
                _commands.RemoveLast();
                cur = _commands.Last;
            }

            HistoryChanged?.Invoke();
        }

        public static void ClearHistory()
        {
            _commands = new LinkedList<Command>();
            _current = null;
            LogManager.GetCurrentClassLogger().Info($"Commands history was cleared!");
            HistoryChanged?.Invoke();
        }

        public static void Undo()
        {
            if (_current == null)
            {
                return;
            }

            if (_current.Value.Undid)
            {
                if (_current.Previous == null)
                {
                    return;
                }

                _current = _current.Previous;
            }

            ICommand command = _current.Value;
            command.Undo();
            HistoryChanged?.Invoke();
            OnUndo?.Invoke();
        }

        public static void Redo()
        {
            if (_current == null)
            {
                return;
            }

            if (_current.Value.Executed)
            {
                if (_current.Next == null)
                {
                    return;
                }

                _current = _current.Next;
            }

            ICommand command = _current.Value;
            command.Execute();
            HistoryChanged?.Invoke();
            OnRedo?.Invoke();
        }

        public static bool CanRedo()
        {
            if (_current == null)
            {
                return false;
            }

            if (_current == _commands.Last && _current.Value.Undid)
            {
                return true;
            }

            return _current.Next != null;
        }

        public static bool CanUndo()
        {
            if (_current == null)
            {
                return false;
            }
            
            if (_current == _commands.First 
                && _current.Value.Executed
                && CommandList.Count > 1)
            {
                return true;
            }

            if (CommandList.Count == 1 && !_current.Value.Undid)
            {
                return true;
            }

            return _current.Previous != null;
        }

        public static void RemoveCommand(Command command)
        {
            _commands.Remove(command);
            _current = _commands.Last;
        }
    }
}
