using System;
using System.Linq;
using Cloo;

public static class ClooExtensions
{
    /// <summary>
    /// Run kernel against all elements. Uses first device and first kernel.
    /// </summary>
    /// <typeparam name="TSource">Struct type that corresponds to kernel function type</typeparam>
    /// <param name="array">Array of elements to process</param>
    /// <param name="kernelCode">The code of kernel function</param>
    public static void OpenCLForEach<TSource>(this TSource[] array, string kernelCode) where TSource : struct
    {
        array.OpenCLForEach(kernelCode, k => true);
    }

    /// <summary>
    /// Run kernel against all elements. Uses first device.
    /// </summary>
    /// <typeparam name="TSource">Struct type that corresponds to kernel function type</typeparam>
    /// <param name="array">Array of elements to process</param>
    /// <param name="kernelCode">The code of kernel function</param>
    /// <param name="kernelSelector">Method that selects kernel by function name</param>
    public static void OpenCLForEach<TSource>(this TSource[] array, string kernelCode, Func<string, bool> kernelSelector) where TSource : struct
    {
        array.OpenCLForEach(kernelCode, kernelSelector, d => true);
    }

    /// <summary>
    /// Run kernel against all elements.
    /// </summary>
    /// <typeparam name="TSource">Struct type that corresponds to kernel function type</typeparam>
    /// <param name="array">Array of elements to process</param>
    /// <param name="kernelCode">The code of kernel function</param>
    /// <param name="kernelSelector">Method that selects kernel by function name</param>
    /// <param name="deviceSelector">Method that selects device by name</param>
    public static void OpenCLForEach<TSource>(this TSource[] array, string kernelCode, Func<string, bool> kernelSelector, Func<string, bool> deviceSelector) where TSource : struct
    {
        var device = ComputePlatform.Platforms.SelectMany(p => p.Devices).First(d => deviceSelector(d.Name));

        var properties = new ComputeContextPropertyList(device.Platform);
        using (var context = new ComputeContext(new[] { device }, properties, null, IntPtr.Zero))
        using (var program = new ComputeProgram(context, kernelCode))
        {
            program.Build(new[] { device }, null, null, IntPtr.Zero);

            var kernels = program.CreateAllKernels().ToList();
            try
            {
                var kernel = kernels.First(k => kernelSelector(k.FunctionName));

                using (var primesBuffer = new ComputeBuffer<TSource>(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.UseHostPointer, array))
                {
                    kernel.SetMemoryArgument(0, primesBuffer);

                    using (var queue = new ComputeCommandQueue(context, context.Devices[0], 0))
                    {
                        queue.Execute(kernel, null, new long[] { primesBuffer.Count }, null, null);
                        queue.Finish();

                        queue.ReadFromBuffer(primesBuffer, ref array, true, null);
                    }
                }
            }
            finally
            {
                kernels.ForEach(k => k.Dispose());
            }
        }
    }
}
