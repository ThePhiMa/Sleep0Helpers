using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sleep0.Logic
{
    [DefaultExecutionOrder(-50)]
    public class MonobehaviourManager : SingletonMonoBehaviour<MonobehaviourManager>
    {
        // Update methods
        private readonly List<IManagedUpdatable> _updatableObjects = new List<IManagedUpdatable>();
        private readonly List<IManagedLateUpdatable> _lateUpdatableObjects = new List<IManagedLateUpdatable>();
        private readonly List<IManagedFixedUpdatable> _fixedUpdatableObjects = new List<IManagedFixedUpdatable>();

        private void Update()
        {
            foreach (IManagedUpdatable updatable in _updatableObjects)
            {
                try
                {
#if ENABLE_PROFILER_MARKERS
                    using (ProfilerMarkerMap.Get("ManagedUpdate", updatable))
#endif
                    updatable.ManagedUpdate();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private void LateUpdate()
        {
            foreach (IManagedLateUpdatable lateUpdatable in _lateUpdatableObjects)
            {
                try
                {
#if ENABLE_PROFILER_MARKERS
                    using (ProfilerMarkerMap.Get("ManagedLateUpdate", lateUpdatable))
#endif
                    lateUpdatable.ManagedLateUpdate();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private void FixedUpdate()
        {
            foreach (IManagedFixedUpdatable fixedUpdatable in _fixedUpdatableObjects)
            {
                try
                {
#if ENABLE_PROFILER_MARKERS
                    using (ProfilerMarkerMap.Get("ManagedFixedUpdate", fixedUpdatable))
#endif
                    fixedUpdatable.ManagedFixedUpdate();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public void Register(IManagedObject obj)
        {
            if (obj is IManagedUpdatable updatable)
            {
                _updatableObjects.Add(updatable);
                _updatableObjects.OrderBy(x => x.UpdateOrder).ToList();
            }
            if (obj is IManagedLateUpdatable lateUpdatable)
            {
                _lateUpdatableObjects.Add(lateUpdatable);
                _lateUpdatableObjects.OrderBy(x => x.UpdateOrder).ToList();
            }
            if (obj is IManagedFixedUpdatable fixedUpdatable)
            {
                _fixedUpdatableObjects.Add(fixedUpdatable);
                _fixedUpdatableObjects.OrderBy(x => x.UpdateOrder).ToList();
            }
        }

        public void Unregister(IManagedObject obj)
        {
            if (obj is IManagedUpdatable updatable)
            {
                _updatableObjects.Remove(updatable);
            }
            if (obj is IManagedLateUpdatable lateUpdatable)
            {
                _lateUpdatableObjects.Remove(lateUpdatable);
            }
            if (obj is IManagedFixedUpdatable fixedUpdatable)
            {
                _fixedUpdatableObjects.Remove(fixedUpdatable);
            }
        }

        public void Clear()
        {
            _updatableObjects.Clear();
            _lateUpdatableObjects.Clear();
            _fixedUpdatableObjects.Clear();
        }
    }
}