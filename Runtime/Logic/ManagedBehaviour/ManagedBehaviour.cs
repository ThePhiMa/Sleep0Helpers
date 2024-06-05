using UnityEngine;

namespace Sleep0.Logic
{
    public abstract class ManagedBehaviour : MonoBehaviour, IManagedObject
    {
        protected int _UpdateOrder = 0;

        public virtual int UpdateOrder
        {
            get { return _UpdateOrder; }
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