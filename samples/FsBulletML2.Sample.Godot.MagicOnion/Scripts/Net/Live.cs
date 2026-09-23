using System;
using System.Collections.Generic;

namespace FsBulletML2.Sample.Godot.MagicOnion
{
    /// <summary>
    /// いまの値 を持っていて、変わったら知らせる箱。
    /// </summary>
    public sealed class Live<T>
    {
        readonly List<Action<T>> listeners = new();
        T current;

        public Live(T initial) => current = initial;

        public T Value
        {
            get => current;
            set
            {
                current = value;
                // 写して回す。 流している最中 に購読 を解く相手 が居る
                // （Node が消えるとき）。元 の並び を回すと落ちる
                foreach (var listener in listeners.ToArray())
                {
                    listener(value);
                }
            }
        }

        /// <summary>
        /// 変わったら呼ばれる。
        /// 返る物 を捨てると解けなくなるので、Node の側 で持って <c>_ExitTree</c> で <c>Dispose</c> する。
        /// </summary>
        public IDisposable Subscribe(Action<T> onNext)
        {
            listeners.Add(onNext);
            onNext(current);
            return new Unsubscriber(listeners, onNext);
        }

        sealed class Unsubscriber : IDisposable
        {
            readonly List<Action<T>> listeners;
            readonly Action<T> target;

            public Unsubscriber(List<Action<T>> listeners, Action<T> target)
            {
                this.listeners = listeners;
                this.target = target;
            }

            public void Dispose() => listeners.Remove(target);
        }
    }

    /// <summary>
    /// 流れるだけ（いまの値 を持たない）。1 コマ ぶん を配るのに使う。
    ///
    /// <see cref="Live{T}"/> と混ぜない。 コマ は「出来事」で、
    /// 購読した瞬間 に前 のコマ が流れると古い並び を 1 回 描くことになる。
    /// </summary>
    public sealed class Stream<T>
    {
        readonly List<Action<T>> listeners = new();

        public void Publish(T value)
        {
            foreach (var listener in listeners.ToArray())
            {
                listener(value);
            }
        }

        public IDisposable Subscribe(Action<T> onNext)
        {
            listeners.Add(onNext);
            return new Unsubscriber(listeners, onNext);
        }

        sealed class Unsubscriber : IDisposable
        {
            readonly List<Action<T>> listeners;
            readonly Action<T> target;

            public Unsubscriber(List<Action<T>> listeners, Action<T> target)
            {
                this.listeners = listeners;
                this.target = target;
            }

            public void Dispose() => listeners.Remove(target);
        }
    }
}
