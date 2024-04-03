using System.Collections.Specialized;
using Wpf.Common;

namespace CoreTests;

public class WpfUnitTests
{
    [Fact]
    public void AddRange_ShouldAddItemsAndRaiseSingleEvent()
    {
        var collectionChangedRaised = false;

        var collection = new ObservableCollectionEx<string>();
        collection.CollectionChanged += (sender, args) =>
        {
            Assert.Equal(NotifyCollectionChangedAction.Reset, args.Action);
            collectionChangedRaised = true;
        };

        var itemsToAdd = new List<string> { "item1", "item2", "item3", };

        collection.AddRange(itemsToAdd);

        Assert.True(collection.SequenceEqual(itemsToAdd));
        Assert.True(collectionChangedRaised);
    }
}