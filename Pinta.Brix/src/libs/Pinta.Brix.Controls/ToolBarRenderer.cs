// ToolBarRenderer.cs
//
// Renders the engine's descriptor-based tool-options toolbar (ToolBar /
// ToolBarItem model) into real CodeBrix.Platform controls, two-way binding
// their state back onto the descriptors.

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Pinta.Brix.Engine;
using EngineToolBar = Pinta.Brix.Engine.ToolBar;

namespace Pinta.Brix.Controls;

public sealed class ToolBarRenderer : IDisposable
{
	private readonly EngineToolBar model;
	private readonly StackPanel panel;

	public ToolBarRenderer (EngineToolBar model, StackPanel panel)
	{
		this.model = model;
		this.panel = panel;
		model.ItemsChanged += OnItemsChanged;
		Rebuild ();
	}

	public void Dispose ()
	{
		model.ItemsChanged -= OnItemsChanged;
	}

	private void OnItemsChanged (object? sender, EventArgs e)
		=> Rebuild ();

	private void Rebuild ()
	{
		panel.Children.Clear ();
		foreach (ToolBarItem item in model.Items) {
			UIElement? element = CreateElement (item);
			if (element is not null)
				panel.Children.Add (element);
		}
	}

	private static UIElement? CreateElement (ToolBarItem item) => item switch {
		ToolBarLabel label => new TextBlock {
			Text = label.Text,
			VerticalAlignment = VerticalAlignment.Center,
			Margin = new Thickness (4, 0, 4, 0),
		},
		ToolBarSeparator => new Border {
			Width = 1,
			Margin = new Thickness (6, 4, 6, 4),
			Background = new SolidColorBrush (Windows.UI.Color.FromArgb (0x40, 0x80, 0x80, 0x80)),
		},
		ToolBarImage image => CreateImage (image),
		ToolBarDropDownButton dropDown => CreateDropDown (dropDown),
		ToolBarComboBox combo => CreateCombo (combo),
		ToolBarSpinButton spin => CreateSpin (spin),
		_ => null,
	};

	private static UIElement CreateImage (ToolBarImage image)
	{
		// V1 renders the current tool's icon as a 16px image when available.
		Image element = new () {
			Width = 16,
			Height = 16,
			VerticalAlignment = VerticalAlignment.Center,
		};
		if (image.IconName is { } icon)
			element.Source = IconImageSource.Create (icon, 16);
		return element;
	}

	private static UIElement CreateDropDown (ToolBarDropDownButton dropDown)
	{
		ComboBox combo = new () {
			VerticalAlignment = VerticalAlignment.Center,
			Margin = new Thickness (2, 0, 2, 0),
		};
		foreach (ToolBarDropDownItem entry in dropDown.Items)
			combo.Items.Add (entry.Text);
		combo.SelectedIndex = dropDown.SelectedIndex;

		bool updating = false;
		combo.SelectionChanged += (_, _) => {
			if (updating || combo.SelectedIndex < 0)
				return;
			updating = true;
			dropDown.SelectedIndex = combo.SelectedIndex;
			updating = false;
		};
		dropDown.SelectedItemChanged += (_, _) => {
			if (updating)
				return;
			updating = true;
			combo.SelectedIndex = dropDown.SelectedIndex;
			updating = false;
		};
		return combo;
	}

	private static UIElement CreateCombo (ToolBarComboBox comboModel)
	{
		ComboBox combo = new () {
			MinWidth = comboModel.Width,
			IsEditable = comboModel.IsEditable,
			VerticalAlignment = VerticalAlignment.Center,
			Margin = new Thickness (2, 0, 2, 0),
		};
		foreach (string entry in comboModel.Items)
			combo.Items.Add (entry);
		combo.SelectedIndex = comboModel.SelectedIndex;

		bool updating = false;
		combo.SelectionChanged += (_, _) => {
			if (updating || combo.SelectedIndex < 0)
				return;
			updating = true;
			comboModel.SelectedIndex = combo.SelectedIndex;
			updating = false;
		};
		comboModel.SelectedItemChanged += (_, _) => {
			if (updating)
				return;
			updating = true;
			combo.SelectedIndex = comboModel.SelectedIndex;
			updating = false;
		};
		return combo;
	}

	private static UIElement CreateSpin (ToolBarSpinButton spin)
	{
		StackPanel host = new () {
			Orientation = Orientation.Horizontal,
			VerticalAlignment = VerticalAlignment.Center,
		};
		TextBox box = new () {
			Text = spin.Value.ToString ("0.##"),
			MinWidth = 48,
			VerticalAlignment = VerticalAlignment.Center,
		};
		Button down = new () { Content = "−", Padding = new Thickness (6, 2, 6, 2) };
		Button up = new () { Content = "+", Padding = new Thickness (6, 2, 6, 2) };

		bool updating = false;
		box.TextChanged += (_, _) => {
			if (updating)
				return;
			if (double.TryParse (box.Text, out double parsed)) {
				updating = true;
				spin.Value = parsed;
				updating = false;
			}
		};
		down.Click += (_, _) => spin.Value -= spin.Step;
		up.Click += (_, _) => spin.Value += spin.Step;
		spin.ValueChanged += (_, _) => {
			if (updating)
				return;
			updating = true;
			box.Text = spin.Value.ToString ("0.##");
			updating = false;
		};

		host.Children.Add (box);
		host.Children.Add (down);
		host.Children.Add (up);
		return host;
	}
}
