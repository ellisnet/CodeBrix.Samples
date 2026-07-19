using System;
using System.Threading.Tasks;
using Pinta.Brix.Engine;

//was previously: namespace Pinta.Effects;
namespace Pinta.Brix.Effects.Tests;

internal sealed class MockChromeManager : IChromeService
{
	public Task<bool> LaunchSimpleEffectDialog (
		BaseEffect effect,
		IWorkspaceService workspace)
	{
		throw new NotImplementedException ();
	}
}
