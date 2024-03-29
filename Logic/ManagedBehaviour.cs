using UnityEngine;

namespace Sleep0.Logic
{
    public class ManagedBehaviour : MonoBehaviour
    {
        public abstract class AManagedBehaviour : MonoBehaviour, IManagedObject
        {
            private int _updateOrder = 0;
            public int UpdateOrder
            {
                get { return _updateOrder; }
            }

            protected virtual void OnEnable()
            {
                this.RegisterInManager();
            }

            protected virtual void OnDisable()
            {
                this.UnregisterInManager();
            }
        }
    }
}