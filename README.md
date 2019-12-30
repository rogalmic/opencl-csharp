# opencl-csharp

Prime calculation example using extension method for array (ClooExtensions.cs). Published to native binary via [CoreRT](https://github.com/dotnet/corert) - see `tasks.json`.

C#:
```cs
    // Prepare data - 2,3,4,5,6,7,8,9,10,11,12,13,14,...
    int[] Primes = Enumerable.Range(2, 1000000).ToArray();

    Primes.OpenCLForEach(IsPrimeKernelFunctionCodeString);
    // Expected result - 2,3,0,5,0,7,0,0,0,11,0,13,0,...
```

Kernel code string contents:
```cpp
kernel void GetIfPrime(global int* message)
{
    int index = get_global_id(0);
    int upperl=(int)sqrt((float)message[index]);
    for(int i=2;i<=upperl;i++)
    {
        if(message[index]%i==0)
        {
            message[index]=0;
            return;
        }
    }
}
```

Requirements:
- newest `VSCode` with C#/Omnisharp extension **OR** newest `Visual Studio 2017`
- .NET Core 3.1 SDK installed ( cmd: `dotnet --info` )
- Windows 10 SDK + Visual studio build tools for c++
- GPU driver [with OpenCL support](https://www.khronos.org/conformance/adopters/conformant-products#opencl)
- for linux/macos create `Cloo.dll.config`:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <dllmap os="linux" dll="opencl.dll" target="libOpenCL.so"/>
  <dllmap os="osx" dll="opencl.dll" target="/System/Library/Frameworks/OpenCL.framework/OpenCL" />
</configuration>
```

The project targets .NET Core, but code files should work with .NET 4.7.2 as well (after installing `Cloo` nuget package).