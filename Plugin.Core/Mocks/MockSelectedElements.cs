using Plugin.Core.Interfaces;
using System.Collections.Generic;

namespace Plugin.Core.Mocks;

public class MockSelectedElements : IQuerySelectedElements { public List<long> GetSelectedElements() => [1, 2, 3, 4, 5, 6,]; }