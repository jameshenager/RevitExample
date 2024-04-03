using Plugin.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace Plugin.Core.Mocks;

public class MockElementSelector : IElementSelector
{
    public void SetSelectedElements(List<long> elementIds) => _ = 1;
    public Action<List<long>> SelectionUpdated { get; set; }
}
//ToDo: Put all of these mocks in another project so the RevitPlugin never needs to see them. This goes hand-in-hand with creating separate Runners.