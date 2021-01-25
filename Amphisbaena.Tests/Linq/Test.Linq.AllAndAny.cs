using Amphisbaena.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Amphisbaena.Tests.Linq {
  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Factory Tests
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  [TestCategory("Linq.AllAndAny")]
  [TestClass()]
  public sealed class AllAndAnyTest {
    [TestMethod("All On Empty")]
    public async Task AllEmpty() {
      bool actual = await (Array.Empty<int>()
        .ToChannelReader()
        .All(x => true));

      Assert.IsTrue(actual);
    }

    [TestMethod("All Even in Array")]
    public async Task AllEven() {
      bool actual = await (new int[] { 2, 124, 8}
        .ToChannelReader()
        .All(x => x % 2 == 0));

      Assert.IsTrue(actual);
    }

    [TestMethod("Not All Even in Array")]
    public async Task AllNotEven() {
      bool actual = await (new int[] { 2, 124, 7, 8 }
        .ToChannelReader()
        .All(x => x % 2 == 0));

      Assert.IsFalse(actual);
    }

    [TestMethod("Any On Empty")]
    public async Task AnyEmpty() {
      bool actual = await (Array.Empty<int>()
        .ToChannelReader()
        .Any());

      Assert.IsFalse(actual);
    }

    [TestMethod("Not Any Even on Array")]
    public async Task NotAnyEven() {
      bool actual = await (new int[] { 3, 5, 137, 9}
        .ToChannelReader()
        .Any(x => x % 2 == 0));

      Assert.IsFalse(actual);
    }

    [TestMethod("Any Even on Array")]
    public async Task AnyEven() {
      bool actual = await (new int[] { 3, 5, 12, 137, 9 }
        .ToChannelReader()
        .Any(x => x % 2 == 0));

      Assert.IsTrue(actual);
    }

    [TestMethod("Default on Empty")]
    public async Task Empty() {
      int[] data = Array.Empty<int>();

      int[] result = await data
        .ToChannelReader()
        .DefaultIfEmpty(77)
        .ToArrayAsync();

      Assert.IsTrue(result.Length == 1 && result[0] == 77);
    }

    [TestMethod("Default on Not Empty")]
    public async Task NotEmpty() {
      int[] data = new int[] { 88, 93};

      int[] result = await data
        .ToChannelReader()
        .DefaultIfEmpty(77)
        .ToArrayAsync();

      Assert.IsTrue(result.Length == data.Length && result.SequenceEqual(result));
    }
  }

}
