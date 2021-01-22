# Amphisbaena

[Amphisbaena](https://en.wikipedia.org/wiki/Amphisbaena) is a mythological serpent with a head at each end.

This **.Net 5** library is a Linq to [ChannelReader](https://docs.microsoft.com/en-us/dotnet/api/system.threading.channels.channelreader-1?view=net-5.0) from System.Threading.Channels. Unlike standard [LINQ to Objects](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/linq-to-objects) (which is extension over [IEnumerable<T>](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1?view=net-5.0)) this implementation is *not lazy* but *eager*
