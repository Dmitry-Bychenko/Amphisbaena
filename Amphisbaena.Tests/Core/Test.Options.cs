using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Amphisbaena.Tests.Core {

  /// <summary>
  /// 
  /// </summary>
  [TestClass]
  public class OptionsTest {
    #region Public

    [TestMethod("Create Balancer")]
    public void CreateBalancer() {
      ChannelParallelOptions op = new ();

      Assert.AreEqual(Environment.ProcessorCount, op.DegreeOfParallelism);
    }


    #endregion Public
  }
}
