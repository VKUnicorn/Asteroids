using System;
using System.Runtime.CompilerServices;
using ObservableCollections;

namespace R3 {
    public static class R3Extensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Execute(this ReactiveCommand command) => command.Execute(Unit.Default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable Subscribe(this Observable<Unit> observable, Action callback) => observable.Subscribe(callback, static (_, cb) => cb.Invoke());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable Subscribe<T>(this Observable<T> observable, ReactiveCommand<T> otherCommand) => observable.Subscribe(otherCommand, static (state, oCmd) => oCmd.Execute(state));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable Subscribe<T>(this Observable<T> observable, ReactiveProperty<T> property) => observable.Subscribe(property, static (state, prop) => prop.Value = state);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDisposable Subscribe<T>(this Observable<T> observable, Subject<T> subject) => observable.Subscribe(subject, static (state, subj) => subj.OnNext(state));
        
        public static Observable<TR> Select<T1, T2, TR>(this Observable<Tuple<T1, T2>> source, Func<T1, T2, TR> selector) => source.Select(tuple => selector.Invoke(tuple.Item1, tuple.Item2));

        public static Observable<TR> Select<T1, T2, T3, TR>(this Observable<Tuple<T1, T2, T3>> source, Func<T1, T2, T3, TR> selector) => source.Select(tuple => selector.Invoke(tuple.Item1, tuple.Item2, tuple.Item3));

        public static Observable<TR> Select<T1, T2, T3, T4, TR>(this Observable<Tuple<T1, T2, T3, T4>> source, Func<T1, T2, T3, T4, TR> selector) => source.Select(tuple => selector.Invoke(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));

        public static Observable<TR> Select<T1, T2, TR>(this Observable<ValueTuple<T1, T2>> source, Func<T1, T2, TR> selector) => source.Select(tuple => selector.Invoke(tuple.Item1, tuple.Item2));

        public static Observable<TR> Select<T1, T2, T3, TR>(this Observable<ValueTuple<T1, T2, T3>> source, Func<T1, T2, T3, TR> selector) => source.Select(tuple => selector.Invoke(tuple.Item1, tuple.Item2, tuple.Item3));

        public static Observable<TR> Select<T1, T2, T3, T4, TR>(this Observable<ValueTuple<T1, T2, T3, T4>> source, Func<T1, T2, T3, T4, TR> selector) => source.Select(tuple => selector.Invoke(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));

        public static Observable<ValueTuple<T1, T2>> Where<T1, T2>(this Observable<ValueTuple<T1, T2>> source, Func<T1, T2, bool> predicate) => source.Where(tuple => predicate.Invoke(tuple.Item1, tuple.Item2));

        public static Observable<ValueTuple<T1, T2, T3>> Where<T1, T2, T3>(this Observable<ValueTuple<T1, T2, T3>> source, Func<T1, T2, T3, bool> predicate) => source.Where(tuple => predicate.Invoke(tuple.Item1, tuple.Item2, tuple.Item3));

        public static Observable<ValueTuple<T1, T2, T3, T4>> Where<T1, T2, T3, T4>(this Observable<ValueTuple<T1, T2, T3, T4>> source, Func<T1, T2, T3, T4, bool> predicate) => source.Where(tuple => predicate.Invoke(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));

        public static Observable<T> Do<T>(this Observable<T> observable, ReactiveProperty<T> callback) => observable.Do(callback, static (state, prop) => prop.Value = state);
        
        public static Observable<Unit> Do(this Observable<Unit> observable, Action callback) => observable.Do(callback, static (_, cb) => cb.Invoke());

        public static Observable<Tuple<T1, T2>> Do<T1, T2>(this Observable<Tuple<T1, T2>> source, Action<T1, T2> onNext) => source.Do(tuple => onNext(tuple.Item1, tuple.Item2));

        public static Observable<Tuple<T1, T2, T3>> Do<T1, T2, T3>(this Observable<Tuple<T1, T2, T3>> source, Action<T1, T2, T3> onNext) => source.Do(tuple => onNext(tuple.Item1, tuple.Item2, tuple.Item3));

        public static Observable<Tuple<T1, T2, T3, T4>> Do<T1, T2, T3, T4>(this Observable<Tuple<T1, T2, T3, T4>> source, Action<T1, T2, T3, T4> onNext) => source.Do(tuple => onNext(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));

        public static Observable<ValueTuple<T1, T2>> Do<T1, T2>(this Observable<ValueTuple<T1, T2>> source, Action<T1, T2> onNext) => source.Do(tuple => onNext(tuple.Item1, tuple.Item2));

        public static Observable<ValueTuple<T1, T2, T3>> Do<T1, T2, T3>(this Observable<ValueTuple<T1, T2, T3>> source, Action<T1, T2, T3> onNext) => source.Do(tuple => onNext(tuple.Item1, tuple.Item2, tuple.Item3));

        public static Observable<ValueTuple<T1, T2, T3, T4>> Do<T1, T2, T3, T4>(this Observable<ValueTuple<T1, T2, T3, T4>> source, Action<T1, T2, T3, T4> onNext) => source.Do(tuple => onNext(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));

        public static Observable<T> DoAssign<T>(this Observable<T> source, IObserver<T> observer) => source.Do(v => observer.OnNext(v), e => observer.OnError(e), _ => observer.OnCompleted());

        public static IDisposable Subscribe<T1, T2>(this Observable<Tuple<T1, T2>> source, Action<T1, T2> onNext) => source.Subscribe(tuple => onNext(tuple.Item1, tuple.Item2));

        public static IDisposable Subscribe<T1, T2, T3>(this Observable<Tuple<T1, T2, T3>> source, Action<T1, T2, T3> onNext) => source.Subscribe(tuple => onNext(tuple.Item1, tuple.Item2, tuple.Item3));

        public static IDisposable Subscribe<T1, T2, T3, T4>(this Observable<Tuple<T1, T2, T3, T4>> source, Action<T1, T2, T3, T4> onNext) => source.Subscribe(tuple => onNext(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));

        public static IDisposable Subscribe<T1, T2>(this Observable<ValueTuple<T1, T2>> source, Action<T1, T2> onNext) => source.Subscribe(tuple => onNext(tuple.Item1, tuple.Item2));

        public static IDisposable Subscribe<T1, T2, T3>(this Observable<ValueTuple<T1, T2, T3>> source, Action<T1, T2, T3> onNext) => source.Subscribe(tuple => onNext(tuple.Item1, tuple.Item2, tuple.Item3));

        public static IDisposable Subscribe<T1, T2, T3, T4>(this Observable<ValueTuple<T1, T2, T3, T4>> source, Action<T1, T2, T3, T4> onNext) => source.Subscribe(tuple => onNext(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));

        public static Observable<Unit> ObserveCollectionChanged<T>(this IObservableCollection<T> list, bool notifyCurrent = false) {
            return notifyCurrent 
                       ? list.ObserveChanged().AsUnitObservable().Prepend(Unit.Default) 
                       : list.ObserveChanged().AsUnitObservable();
        }        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Observable<T> SkipLatestValueOnSubscribe<T>(this ReadOnlyReactiveProperty<T> source) => source.Skip(1);
    }
}