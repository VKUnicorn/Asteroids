using System;
using System.Collections.Generic;
using Morpeh;

namespace Nodes.Battle {
    /// <summary>
    /// Event lives for one tick. Send: for the systems later in the current tick, SendNextFrame: for all systems in the next tick
    /// </summary>
    public sealed class WorldEvents {
        private readonly Dictionary<Type, IEventsList> _events = new ();

        public void Send<T>(T value) where T : class, IEvent => GetEvents<T>().Current.Add(value);

        public void SendNextFrame<T>(T value) where T : class, IEvent => GetEvents<T>().Next.Add(value);

        public IReadOnlyList<T> Get<T>() where T : class, IEvent => GetEvents<T>().Current;

        public void FrameStart() {
            foreach (var events in _events.Values) {
                events.FrameStart();
            }
        }

        public void FrameComplete() {
            foreach (var events in _events.Values) {
                events.FrameComplete();
            }
        }

        private EventsList<T> GetEvents<T>() where T : class, IEvent {
            if (_events.TryGetValue(typeof(T), out var events)) {
                return (EventsList<T>)events;
            }

            var created = new EventsList<T>();
            _events.Add(typeof(T), created);
            return created;
        }

        private interface IEventsList {
            void FrameStart();
            void FrameComplete();
        }

        private class EventsList<T> : IEventsList {
            public readonly List<T> Current = new ();
            public readonly List<T> Next = new ();

            public void FrameStart() {
                Current.AddRange(Next);
                Next.Clear();
            }

            public void FrameComplete() => Current.Clear();
        }
    }
}
