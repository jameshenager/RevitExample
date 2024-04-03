using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Wpf.Common;
public class ObservableCollectionEx<T> : ObservableCollection<T>
{
    private bool _notificationSuppressed;
    private bool _suppressNotification;
    public bool SuppressNotification
    {
        get => _suppressNotification;
        set
        {
            _suppressNotification = value;
            if (_suppressNotification || !_notificationSuppressed) { return; }
            OnCollectionChanged(new(NotifyCollectionChangedAction.Reset));
            _notificationSuppressed = false;
        }
    }

    public void AddRange(IEnumerable<T> list)
    {
        if (list == null) { throw new ArgumentNullException(nameof(list)); }

        _suppressNotification = true;

        foreach (var item in list) { Add(item); }
        _suppressNotification = false;
        OnCollectionChanged(new(NotifyCollectionChangedAction.Reset));
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (SuppressNotification) { _notificationSuppressed = true; return; }
        base.OnCollectionChanged(e);
    }

    public void ReplaceWith(IEnumerable<T> run)
    {
        Clear();
        AddRange(run);
    }
}