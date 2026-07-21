// ColorPickerDialog.cs
//
// Pinta.Brix note: upstream hand-rolls a 1,013-line colour picker over a Cairo
// DrawingArea because GTK has no colour picker worth using. CodeBrix.Platform
// ships one - ColorPicker, with the spectrum, the sliders, the hex entry and
// the alpha channel - so this dialog wraps that instead of reimplementing it,
// and adds the pieces upstream has that the platform control does not: the
// recently-used strip and the current palette.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Pinta.Brix.Engine;
using Drawing = Pinta.Brix.Engine.Drawing;

namespace Pinta.Brix.Controls;

/// <summary>A dialog for choosing a colour.</summary>
public static class ColorPickerDialog
{
	private const int SwatchSize = 22;

	/// <summary>
	/// Shows the picker.
	/// </summary>
	/// <param name="title">The dialog's title.</param>
	/// <param name="initial">The colour to start on.</param>
	/// <param name="xamlRoot">The root to attach the dialog to.</param>
	/// <returns>The chosen colour, or null when the user cancelled.</returns>
	public static async Task<Drawing.Color?> ShowAsync (string title, Drawing.Color initial, XamlRoot xamlRoot)
	{
		ColorPicker picker = new () {
			Color = ToWindowsColor (initial),
			IsAlphaEnabled = true,
			IsHexInputVisible = true,
			IsColorChannelTextInputVisible = true,
			IsColorSliderVisible = true,
			IsMoreButtonVisible = false,
			MinWidth = 320,
		};

		StackPanel panel = new () { Spacing = 10 };
		panel.Children.Add (picker);

		// Upstream's picker carries both strips, and they are the fastest way
		// back to a colour already in use.
		AddSwatchStrip (panel, "Recently used", PintaCore.Palette.RecentlyUsedColors, picker);
		AddSwatchStrip (panel, "Palette", PintaCore.Palette.CurrentPalette.Colors, picker);

		ContentDialog dialog = new () {
			Title = title,
			Content = new ScrollViewer { Content = panel, MaxHeight = 560 },
			PrimaryButtonText = "OK",
			CloseButtonText = "Cancel",
			DefaultButton = ContentDialogButton.Primary,
			XamlRoot = xamlRoot,
		};

		if (await dialog.ShowAsync () != ContentDialogResult.Primary)
			return null;

		return FromWindowsColor (picker.Color);
	}

	private static void AddSwatchStrip (
		Panel parent,
		string header,
		IReadOnlyList<Drawing.Color> colors,
		ColorPicker picker)
	{
		if (colors.Count == 0)
			return;

		parent.Children.Add (new TextBlock { Text = header, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });

		// A wrapping strip: a fixed-width host with the swatches flowed into
		// rows, which is what upstream's 500px-wide swatch box amounts to.
		Grid strip = new () { MaxWidth = 500 };

		const int columns = 20;

		for (int i = 0; i < columns; i++)
			strip.ColumnDefinitions.Add (new ColumnDefinition { Width = GridLength.Auto });

		int rows = (int) Math.Ceiling (colors.Count / (double) columns);

		for (int i = 0; i < rows; i++)
			strip.RowDefinitions.Add (new RowDefinition { Height = GridLength.Auto });

		for (int i = 0; i < colors.Count; i++) {

			Drawing.Color color = colors[i];

			Button swatch = new () {
				Width = SwatchSize,
				Height = SwatchSize,
				MinWidth = SwatchSize,
				MinHeight = SwatchSize,
				Padding = new Thickness (0),
				Margin = new Thickness (1),
				Background = new SolidColorBrush (ToWindowsColor (color)),
			};

			ToolTipService.SetToolTip (swatch, color.ToHex ());
			swatch.Click += (_, _) => picker.Color = ToWindowsColor (color);

			Grid.SetRow (swatch, i / columns);
			Grid.SetColumn (swatch, i % columns);
			strip.Children.Add (swatch);
		}

		parent.Children.Add (strip);
	}

	private static Windows.UI.Color ToWindowsColor (Drawing.Color color) => Windows.UI.Color.FromArgb (
		(byte) Math.Clamp (color.A * 255, 0, 255),
		(byte) Math.Clamp (color.R * 255, 0, 255),
		(byte) Math.Clamp (color.G * 255, 0, 255),
		(byte) Math.Clamp (color.B * 255, 0, 255));

	private static Drawing.Color FromWindowsColor (Windows.UI.Color color) => new (
		color.R / 255.0,
		color.G / 255.0,
		color.B / 255.0,
		color.A / 255.0);
}
