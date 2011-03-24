using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Myre.Entities.Behaviours
{
    interface IManagerHandler
    {
        IBehaviourManager Manager { get; set; }
        void Add(Behaviour behaviour);
        void Remove(Behaviour behaviour);
    }

    class ManagerHandler<T>
        : IManagerHandler
        where T : Behaviour
    {
        private IBehaviourManager<T> manager;

        public IBehaviourManager Manager
        {
            get { return manager; }
            set { manager = value as IBehaviourManager<T>; }
        }

        public void Add(T behaviour)
        {
            if (behaviour.CurrentManager.Handler != null)
                behaviour.CurrentManager.Handler.Remove(behaviour);

            manager.Add(behaviour);

            behaviour.CurrentManager.Handler = this;
            behaviour.CurrentManager.ManagedAs = typeof(T);
        }

        public void Remove(T behaviour)
        {
            if (behaviour.CurrentManager.Handler != this)
                return;

            behaviour.CurrentManager.Handler = null;
            behaviour.CurrentManager.ManagedAs = null;

            manager.Remove(behaviour);
        }

        #region IManagerHandler Members

        void IManagerHandler.Add(Behaviour behaviour)
        {
            Add(behaviour as T);
        }

        void IManagerHandler.Remove(Behaviour behaviour)
        {
            Remove(behaviour as T);
        }

        #endregion
    }

    class BehaviourManagerContainer
        : IEnumerable<IBehaviourManager>
    {
        struct PrivateList
        {
            public object List;
            public object ReadOnly;
        }

        private List<IBehaviourManager> managers;
        private Dictionary<Type, IBehaviourManager> byType;
        private Dictionary<Type, IManagerHandler> byBehaviour;
        private Dictionary<Type, PrivateList> catagorised;

        public BehaviourManagerContainer()
        {
            managers = new List<IBehaviourManager>();
            byType = new Dictionary<Type, IBehaviourManager>();
            byBehaviour = new Dictionary<Type, IManagerHandler>();
            catagorised = new Dictionary<Type, PrivateList>();
        }

        public void Add(IBehaviourManager manager)
        {
            var managerType = manager.GetType();

            managers.Add(manager);
            byType[managerType] = manager;

            foreach (var type in manager.GetManagedTypes())
            {
                IManagerHandler handler = null;
                if (!byBehaviour.TryGetValue(type, out handler))
                {
                    var handlerType = typeof(ManagerHandler<>).MakeGenericType(type);
                    handler = Activator.CreateInstance(handlerType) as IManagerHandler;
                    byBehaviour[type] = handler;
                }

                handler.Manager = manager;
            }

            CatagoriseManager(manager);
        }

        private void CatagoriseManager(IBehaviourManager manager)
        {
            foreach (var item in IterateBaseTypesAndInterfaces(manager.GetType()))
            {
                PrivateList list;
                if (catagorised.TryGetValue(item, out list))
                    AddToList(list, item, manager);
            }
        }

        private IEnumerable<Type> IterateBaseTypesAndInterfaces(Type type)
        {
            for (var t = type; t != null; t = t.BaseType)
                yield return t;

            foreach (var t in type.GetInterfaces())
                yield return t;
        }

        private void AddToList(PrivateList list, Type type, IBehaviourManager manager)
        {
            var listType = list.List.GetType();
            var addMethod = listType.GetMethod("Add", new Type[] { type });
            addMethod.Invoke(list.List, new object[] { manager });
        }

        public bool Remove(IBehaviourManager manager)
        {
            var removed = managers.Remove(manager);

            if (removed)
            {
                var managerType = manager.GetType();
                byType.Remove(managerType);

                foreach (var type in manager.GetManagedTypes())
                    byBehaviour[type].Manager = null;

                foreach (var type in IterateBaseTypesAndInterfaces(managerType))
                {
                    PrivateList list;
                    if (catagorised.TryGetValue(type, out list))
                        RemoveFromList(list, type, manager);
                }
            }

            return removed;
        }

        private void RemoveFromList(PrivateList list, Type type, IBehaviourManager manager)
        {
            var listType = list.List.GetType();
            var removeMethod = listType.GetMethod("Remove", new Type[] { type });
            removeMethod.Invoke(list.List, new object[] { manager });
        }

        public bool Contains(Type managerType)
        {
            return byType.ContainsKey(managerType);
        }

        public bool Contains(IBehaviourManager manager)
        {
            return managers.Contains(manager);
        }

        public bool ContainsForBehaviour(Type behaviourType)
        {
            return byBehaviour.ContainsKey(behaviourType);
        }

        public IBehaviourManager Get(Type managerType)
        {
            return byType[managerType];
        }

        public bool TryGet(Type managerType, out IBehaviourManager manager)
        {
            return byType.TryGetValue(managerType, out manager);
        }

        public IManagerHandler GetByBehaviour(Type behaviourType)
        {
            return byBehaviour[behaviourType];
        }

        public bool TryGetByBehaviour(Type behaviourType, out IManagerHandler manager)
        {
            return byBehaviour.TryGetValue(behaviourType, out manager);
        }

        public void Clear()
        {
            managers.Clear();
            byBehaviour.Clear();
            byType.Clear();
        }

        public IManagerHandler Find(Type behaviourType, IBehaviourManager manager = null)
        {
            var behaviour = typeof(Behaviour);
            while (behaviour.IsAssignableFrom(behaviourType))
            {
                IManagerHandler handler = null;
                if (TryGetByBehaviour(behaviourType, out handler) && (manager == null || handler.Manager == manager))
                    return handler;

                behaviourType = behaviourType.BaseType;
            }

            return null;
        }

        public ReadOnlyCollection<T> FindByType<T>()
        {
            var type = typeof(T);

            PrivateList list;
            if (!catagorised.TryGetValue(type, out list))
            {
                list = CreatePrivateList(type);
                catagorised[type] = list;
            }

            return list.ReadOnly as ReadOnlyCollection<T>;
        }

        private PrivateList CreatePrivateList(Type type)
        {
            var listType = typeof(List<>).MakeGenericType(type);
            var readOnlyType = typeof(ReadOnlyCollection<>).MakeGenericType(type);

            PrivateList list;
            list.List = Activator.CreateInstance(listType);
            list.ReadOnly = Activator.CreateInstance(readOnlyType, list.List);

            var addMethod = list.List.GetType().GetMethod("Add", new Type[] { type });
            foreach (var manager in managers)
            {
                var managerType = manager.GetType();
                if (type.IsAssignableFrom(managerType))
                    addMethod.Invoke(list.List, new object[] { manager });
            }

            return list;
        }

        #region IEnumerable<IBehaviourManager> Members

        public IEnumerator<IBehaviourManager> GetEnumerator()
        {
            return managers.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
