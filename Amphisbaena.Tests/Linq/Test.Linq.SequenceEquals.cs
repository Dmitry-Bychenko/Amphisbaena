using Amphisbaena.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Amphisbaena.Tests.Linq {
  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Min And Max
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  [TestCategory("Linq.MinAndMax")]
  [TestClass]
  public sealed class SequenceEqualsTest {
    
    [TestMethod("Reader {5, 7, 9} == {5, 7, 9}")]
    public async Task SequenceEqualsToReader() {
      int[] data = new int[] { 5, 7, 9 };

      int[] other = new int[] { 5, 7, 9 };
          
      Assert.IsTrue(await data.ToChannelReader().SequenceEquals(other), "Reader to IEnumerable");
      Assert.IsTrue(await data.ToChannelReader().SequenceEquals(other.ToChannelReader()), "Reader to Reader");
    }

    [TestMethod("Reader {5, 7, 9} != {5, 7}")]
    public async Task SequenceNotEqualsToReader() {
      int[] data = new int[] { 5, 7, 9 };

      int[] other = new int[] { 5, 7 };

      Assert.IsFalse(await data.ToChannelReader().SequenceEquals(other), "Reader to IEnumerable");
      Assert.IsFalse(await data.ToChannelReader().SequenceEquals(other.ToChannelReader()), "Reader to Reader");
    }
  }
}