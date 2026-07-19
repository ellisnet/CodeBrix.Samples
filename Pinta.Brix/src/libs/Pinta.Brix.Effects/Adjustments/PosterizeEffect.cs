/////////////////////////////////////////////////////////////////////////////////
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See license-pdn.txt for full licensing and attribution details.             //
//                                                                             //
// Ported to Pinta by: Krzysztof Marecki <marecki.krzysztof@gmail.com>         //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading.Tasks;
using Pinta.Brix.Engine.Drawing;
using Pinta.Brix.Engine;

//was previously: namespace Pinta.Effects;
namespace Pinta.Brix.Effects;

public sealed class PosterizeEffect : BaseEffect
{
	UnaryPixelOps.PosterizePixel? op = null;

	public sealed override bool IsTileable => true;

	public override string Icon => Icons.AdjustmentsPosterize;

	public override string Name => Translations.GetString ("Posterize");

	public override bool IsConfigurable => true;

	public override string AdjustmentMenuKey => "P";

	public PosterizeData Data => (PosterizeData) EffectData!;  // NRT - Set in constructor

	private readonly IChromeService chrome;

	public PosterizeEffect (IServiceProvider services)
	{
		chrome = services.GetService<IChromeService> ();

		EffectData = new PosterizeData ();
	}

	public override Task<bool> LaunchConfiguration ()
	{
		// Pinta.Brix note: upstream launched the custom PosterizeDialog here;
		// the custom effect dialogs are ported later with the UI layer. Until
		// then configuration reports "cancelled" so the effect is a safe no-op.
		//was previously:
		//	using PosterizeDialog dialog = PosterizeDialog.New (chrome);
		//	dialog.Title = Name;
		//	dialog.IconName = Icon;
		//	dialog.EffectData = Data; // TODO: Delegate `EffectData` changes to event handlers or similar
		//
		//	Gtk.ResponseType response = await dialog.RunAsync ();
		//
		//	dialog.Destroy ();
		//
		//	return Gtk.ResponseType.Ok == response;
		return Task.FromResult (false);
	}

	public override void Render (ImageSurface src, ImageSurface dest, ReadOnlySpan<RectangleI> rois)
	{
		op ??= new UnaryPixelOps.PosterizePixel (Data.Red, Data.Green, Data.Blue);

		op.Apply (dest, src, rois);
	}
}

public sealed class PosterizeData : EffectData
{
	public int Red { get; set; } = 16;
	public int Green { get; set; } = 16;
	public int Blue { get; set; } = 16;
}
