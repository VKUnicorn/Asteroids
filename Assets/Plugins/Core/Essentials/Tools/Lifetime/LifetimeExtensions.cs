using System;

namespace Core.Tools {
    public static class LifetimeExtensions {
        public static T AddTo<T>(this T disposable, ILifetime lifetime) where T : IDisposable {
            lifetime.Register(disposable);
            return disposable;
        }
    }
}