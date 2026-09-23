using UnityEngine;

// R3 の Unity 向けパッケージ（R3.Unity）に在るもののうち、 このサンプルが呼ぶぶんだけ。
namespace R3
{
    /// <summary>
    /// Unity の Inspector に出せる ReactiveProperty。本物は R3.Unity に在る。
    /// ここは値を持って読み書きできれば足りる
    /// </summary>
    [System.Serializable]
    public class SerializableReactiveProperty<T> : ReactiveProperty<T>
    {
        public SerializableReactiveProperty() : base(default) { }
        public SerializableReactiveProperty(T value) : base(value) { }
    }
}

namespace R3
{
    /// <summary>
    /// 購読を component の寿命に紐づける。本物は R3.Unity に在って、
    /// component が壊れたときに Dispose する。ここは何もしない
    /// </summary>
    public static class UnityDisposableExtensions
    {
        public static T AddTo<T>(this T disposable, Component component) where T : System.IDisposable
            => disposable;

        public static T AddTo<T>(this T disposable, GameObject gameObject) where T : System.IDisposable
            => disposable;
    }
}

namespace R3.Triggers
{
    public static class ObservableTriggerExtensions
    {
        public static Observable<Collider2D> OnTriggerEnter2DAsObservable(this Component component)
            => Observable.Empty<Collider2D>();

        public static Observable<Collider2D> OnTriggerStay2DAsObservable(this Component component)
            => Observable.Empty<Collider2D>();

        public static Observable<Collider2D> OnTriggerExit2DAsObservable(this Component component)
            => Observable.Empty<Collider2D>();
    }
}
