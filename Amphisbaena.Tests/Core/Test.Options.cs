﻿using Amphisbaena.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Collections.Concurrent;
using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Amphisbaena.Tests.Core {
  
  /// <summary>
  /// 
  /// </summary>
  [TestClass]
  public class OptionsTest {
    #region Public

    [TestMethod("Create Balancer")]
    public void CreateBalancer() {
      ChannelParallelOptions op = new ChannelParallelOptions();

      var channels = Enumerable
        .Range(0, 12)
        .Select(_ => Channel.CreateUnbounded<int>())
        .ToArray();

      var balancer = op.CreateBalancer<int>(channels);

      Assert.AreEqual(Environment.ProcessorCount, op.DegreeOfParallelism);
    }


    #endregion Public
  }
}