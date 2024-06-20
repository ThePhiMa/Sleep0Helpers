using UnityEngine;

namespace Sleep0.Logic
{
    public interface IManagedObject
    {
        public int UpdateOrder { get; }
    }

    public static class IUpdatableExtensions
    {
        /// <summary>
        /// Shortcut for <c>MonobehaviourManager.Instance.Register(<paramref name="updatable"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateManager.Register"/>
        public static void RegisterInManager(this IManagedObject updatable)
        {
            Debug.Assert(MonobehaviourManager.Instance != null, "MonobehaviourManager.Instance is null");
            MonobehaviourManager.Instance.Register(updatable);
        }

        /// <summary>
        /// Shortcut for <c>MonobehaviourManager.Instance.Unregister(<paramref name="updatable"/>)</c>.
        /// </summary>
        /// <seealso cref="UpdateManager.Unregister"/>
        public static void UnregisterInManager(this IManagedObject updatable)
        {
            Debug.Assert(MonobehaviourManager.Instance != null, "MonobehaviourManager.Instance is null");
            MonobehaviourManager.Instance.Unregister(updatable);
        }
    }
}