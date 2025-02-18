using System;
using SharpGen.Runtime;
using Xunit;

namespace Interface;

public class MethodTests
{
    [Fact]
    public void PointerSizeMethodReturnTest()
    {
        using (var target = Functions.GetPassThroughMethodTest())
        {
            Assert.Equal((nuint)25, target.PassThrough(25));
        }
    } 
        
    [Fact]
    public void LongMethodReturnTest()
    {
        using (var target = Functions.GetPassThroughMethodTest())
        {
            Assert.Equal(new NativeLong(25), target.PassThroughLong(new NativeLong(25)));
        }
    }
}