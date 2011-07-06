using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace Myre.Graphics.ModelViewer.Model
{
    public interface IAction
    {
        string Name { get; }
        float Progress { get; set; }
        bool IsComplete { get; set; }
    }

    public interface IStatusManager
    {
        IAction Start(string name);
        event Action<IStatusManager> StatusChanged;
        string Status { get; }
        float Progress { get; }
        bool IsWorking { get; }
    }

    public class ApplicationStatus
        : IStatusManager
    {
        #region Action
        class Action
            : IAction
        {
            private string name;
            private float progress;
            private bool complete;

            private ApplicationStatus statusManager;

            public string Name
            {
                get { return name; }
                set
                {
                    if (name != value)
                    {
                        lock (statusManager.lockObject)
                            name = value;
                        statusManager.Update();
                    }
                }
            }

            public float Progress
            {
                get { return progress; }
                set
                {
                    if (progress != value)
                    {
                        lock (statusManager.lockObject)
                            progress = Clamp(value);
                        statusManager.Update();
                    }
                }
            }

            public bool IsComplete
            {
                get { return complete; }
                set
                {
                    if (complete != value)
                    {
                        lock (statusManager.lockObject)
                            complete = value;
                        statusManager.Update();
                    }
                }
            }

            public Action(ApplicationStatus statusManager, string name)
            {
                this.name = name;
                this.statusManager = statusManager;
                this.progress = -1;
            }

            private float Clamp(float value)
            {
                if (value < 0)
                    return 0;

                if (value > 1)
                    return 1;

                return value;
            }
        }
        #endregion

        private Stack<Action> activeActions;
        private object lockObject;

        public event Action<IStatusManager> StatusChanged;

        public string Status { get; private set; }
        public float Progress { get; private set; }
        public bool IsWorking { get; private set; }

        public ApplicationStatus()
        {
            lockObject = new object();
            activeActions = new Stack<Action>();

            InitialState();
        }

        private void InitialState()
        {
            Status = "Ready";
            Progress = 0;
            IsWorking = false;
        }

        private void Update()
        {
            lock (lockObject)
            {
                while (activeActions.Count > 0 && activeActions.Peek().IsComplete)
                    activeActions.Pop();

                if (activeActions.Count > 0)
                {
                    var action = activeActions.Peek();
                    Status = action.Name;
                    Progress = action.Progress;
                    IsWorking = true;
                }
                else
                {
                    InitialState();
                }

                if (StatusChanged != null)
                    StatusChanged(this);
            }
        }

        public IAction Start(string actionName)
        {
            lock (lockObject)
            {
                var action = new Action(this, actionName);
                activeActions.Push(action);

                Update();
                return action;
            }
        }
    }
}
