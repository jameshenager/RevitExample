using System;

namespace Plugin.Core;

public interface IMockableEvent { void Raise(); }

public class MockableEvent : IMockableEvent
{
    public Action RaiseAction { get; set; }
    public void Raise() => RaiseAction?.Invoke();
}
//comment