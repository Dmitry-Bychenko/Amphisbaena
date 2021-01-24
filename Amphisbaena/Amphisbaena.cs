//---------------------------------------------------------------------------------------------------------------------
//
// Dmitry Bychenko, 2021
//
// Amphisbaena is a mythological serpent with a head at each end.
// 
// This .Net 5 library is a Linq to ChannelReader from System.Threading.Channels. Unlike standard LINQ to Objects 
// (which is an extension over IEnumerable<>) this implementation is not lazy but eager and asynchronous:
//
// Typical simple query is
//   
// IEnumerable<double> source = ...
//
// var average = await source
//   .ToChannelReader()
//   .Aggregate((s, a) => s + a);
//
// var average = (await source
//   .Statistics())
//   .Average;
//
// Copyright (c) 2021, Dmitry Bychenko
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
//---------------------------------------------------------------------------------------------------------------------

using System;

[assembly: CLSCompliant(true)]

namespace Amphisbaena {
}
