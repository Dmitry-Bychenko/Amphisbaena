# Amphisbaena

![Amphisbaena](https://github.com/Dmitry-Bychenko/Amphisbaena/blob/master/Images/Amphisbaena.png "Amphisbaena")

[Amphisbaena](https://en.wikipedia.org/wiki/Amphisbaena) is a mythological serpent with a head at each end. 

For a long time have we good old [LINQ to Objects](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/linq-to-objects) which is *lazy* and *synchronous*. This **.Net 5** library is a LINQ to [ChannelReader](https://docs.microsoft.com/en-us/dotnet/api/system.threading.channels.channelreader-1?view=net-5.0) from [System.Threading.Channels](https://docs.microsoft.com/en-us/dotnet/api/system.threading.channels?view=net-5.0); and this implementation is *eager* and *asynchronous*.

Despite different execution model, I've tried to preserve the [syntax](https://github.com/Dmitry-Bychenko/Amphisbaena/wiki/Linq) as much as I can:
  
  ```c#
  // 1 + 2 + 3 + ... + 100
  var sum = await Enumerable
    .Range(1, 100)
    .ToChannelReader()
    .Aggregate((s, a) => s + a);
 ```
 
However, `async` brings some specific:

```c#
var strangeSum = await Enumerable
  .Range(1, 1000)
  .ToChannelReader()
  .ToBatch((batch, item, index) => batch.Count < 100, 
            new ChannelParallelOptions() { DegreeOfParallelism = 2 })
  .ActForEach(batch => logger.Debug($"{batch.Length}"),
              batch => batch.Length < 100)
  .ForEach(batch => batch.Sum(), 
           new ChannelParallelOptions() { DegreeOfParallelism = 4 })
  .DetachAttach(item => item > 20_000,
                process => process.Select(item => item * 2))
  .Aggregate((s, a) => s + a);
```
  
