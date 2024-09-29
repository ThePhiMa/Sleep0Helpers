using System;

namespace Sleep0
{
    public interface IEventBus
    {
        void Publish<T>(T gameEvent) where T : IGameEvent;
        void Subscribe<T>(Action<T> handler) where T : IGameEvent;
        void Unsubscribe<T>(Action<T> handler) where T : IGameEvent;
    }
}