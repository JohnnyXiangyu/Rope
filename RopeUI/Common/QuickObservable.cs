using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Channels;

namespace RopeUI.Common;
public class QuickObservable<T> : IObservable<T>
{
    private readonly Channel<T> _dataChannel = Channel.CreateUnbounded<T>();
    private readonly IObservable<T> _observable;

    public QuickObservable()
    {
        _observable = _dataChannel.Reader.ReadAllAsync().ToObservable().Publish().RefCount();
    }

    public bool BroadCast(T data) => _dataChannel.Writer.TryWrite(data);

    public IDisposable Subscribe(IObserver<T> observer) => _observable.Subscribe(observer);
}
