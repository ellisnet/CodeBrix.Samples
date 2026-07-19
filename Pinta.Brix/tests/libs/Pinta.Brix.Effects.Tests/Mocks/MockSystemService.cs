using System;
using Pinta.Brix.Engine;

//was previously: namespace Pinta.Effects;
namespace Pinta.Brix.Effects.Tests;

public class MockSystemService : ISystemService
{
	public int RenderThreads { get; set; } = Environment.ProcessorCount;
	public OS OperatingSystem { get; } = OS.Other;
}
