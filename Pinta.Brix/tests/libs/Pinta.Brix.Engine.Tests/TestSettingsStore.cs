// TestSettingsStore.cs
//
// PintaCore's static constructor builds the palette manager, which reads
// settings, so touching PintaCore at all requires an open settings store.
// SettingsService is a process-global singleton, so it is pointed at a
// throwaway folder once per test assembly - never at the user's real
// ~/.config/Pinta.Brix/settings, which tests must never read or write.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using Pinta.Brix.Settings;

namespace Pinta.Brix.Engine.Tests;

internal static class TestSettingsStore
{
	[ModuleInitializer]
	internal static void Initialize ()
	{
		if (SettingsService.IsInitialized)
			return;

		SettingsService.Initialize (Path.Combine (
			Path.GetTempPath (),
			"PintaBrix.Engine.Tests_" + Guid.NewGuid ().ToString ("N")));
	}
}
