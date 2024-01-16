namespace Sleep0.Logic
{
    public abstract class ParameteredManagedBehaviour<T> : ManagedBehaviour
    {
        private T _param;

        public abstract void OnCreate(T param);
    }

    public abstract class ParameteredManagedBehaviour<T, U> : ManagedBehaviour
    {
        private T _param1;
        private U _param2;

        public abstract void OnCreate(T param1, U param2);
    }

    public abstract class ParameteredManagedBehaviour<T, U, V> : ManagedBehaviour
    {
        private T _param1;
        private U _param2;
        private V _param3;

        public abstract void OnCreate(T param1, U param2, V param3);
    }

    public abstract class ParameteredManagedBehaviour<T, U, V, W> : ManagedBehaviour
    {
        private T _param1;
        private U _param2;
        private V _param3;
        private W _param4;

        public abstract void OnCreate(T param1, U param2, V param3, W param4);
    }
}