// EffectOptionsDialog.cs
//
// The reflection-generated effect/adjustment configuration dialog: builds an
// options panel from an effect's EffectData members and their dialog
// attributes, firing PropertyChanged so the live-preview system re-renders
// as values change. Ports the role of the upstream reflection dialog onto
// ContentDialog; unsupported member types degrade to a read-only note.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pinta.Brix.Engine;

namespace Pinta.Brix.Controls;

public static class EffectOptionsDialog
{
	public static async Task<bool> ShowAsync (BaseEffect effect, XamlRoot xamlRoot)
	{
		if (effect.EffectData is not { } data)
			return true;

		StackPanel panel = new () { Spacing = 10, MinWidth = 340 };

		foreach (MemberInfo member in GetDialogMembers (data)) {
			FrameworkElement? row = CreateRow (data, member);
			if (row is not null)
				panel.Children.Add (row);
		}

		ContentDialog dialog = new () {
			Title = effect.Name,
			Content = new ScrollViewer {
				Content = panel,
				VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
				MaxHeight = 480,
			},
			PrimaryButtonText = "OK",
			CloseButtonText = "Cancel",
			DefaultButton = ContentDialogButton.Primary,
			XamlRoot = xamlRoot,
		};

		ContentDialogResult result = await dialog.ShowAsync ();
		return result == ContentDialogResult.Primary;
	}

	private static IEnumerable<MemberInfo> GetDialogMembers (EffectData data)
	{
		Type type = data.GetType ();
		foreach (MemberInfo member in type.GetMembers (BindingFlags.Public | BindingFlags.Instance)) {
			if (member is not PropertyInfo and not FieldInfo)
				continue;
			if (member is PropertyInfo { CanWrite: false })
				continue;
			if (member.DeclaringType == typeof (EffectData) || member.DeclaringType == typeof (ObservableObject))
				continue;
			if (member.GetCustomAttribute<SkipAttribute> () is not null)
				continue;
			yield return member;
		}
	}

	private static string GetCaption (MemberInfo member)
		=> member.GetCustomAttribute<CaptionAttribute> ()?.Caption
			?? AddSpaces (member.Name);

	private static string AddSpaces (string name)
		=> string.Concat (name.Select ((c, i) => i > 0 && char.IsUpper (c) ? " " + c : c.ToString ()));

	private static Type MemberType (MemberInfo member)
		=> member switch {
			PropertyInfo p => p.PropertyType,
			FieldInfo f => f.FieldType,
			_ => typeof (object),
		};

	private static object? GetValue (EffectData data, MemberInfo member)
		=> member switch {
			PropertyInfo p => p.GetValue (data),
			FieldInfo f => f.GetValue (data),
			_ => null,
		};

	private static void SetValue (EffectData data, MemberInfo member, object? value)
	{
		switch (member) {
			case PropertyInfo p:
				p.SetValue (data, value);
				break;
			case FieldInfo f:
				f.SetValue (data, value);
				break;
		}
		data.FirePropertyChanged (member.Name);
	}

	private static FrameworkElement? CreateRow (EffectData data, MemberInfo member)
	{
		Type type = MemberType (member);
		string caption = GetCaption (member);

		if (type == typeof (int))
			return CreateNumericRow (data, member, caption, isInteger: true);
		if (type == typeof (double))
			return CreateNumericRow (data, member, caption, isInteger: false);
		if (type == typeof (bool))
			return CreateCheckRow (data, member, caption);
		if (type.IsEnum)
			return CreateEnumRow (data, member, caption, type);
		if (type == typeof (string) && member.GetCustomAttribute<StaticListAttribute> () is { } list)
			return CreateStaticListRow (data, member, caption, list);
		if (type == typeof (DegreesAngle))
			return CreateAngleRow (data, member, caption);
		if (type == typeof (RandomSeed))
			return CreateSeedRow (data, member, caption);

		// Unsupported member type (e.g. point/offset/color pickers): note it so
		// the effect is still usable with its default value.
		return new TextBlock {
			Text = $"{caption}: (not yet editable in this port)",
			Opacity = 0.6,
		};
	}

	private static FrameworkElement CreateNumericRow (EffectData data, MemberInfo member, string caption, bool isInteger)
	{
		double min = member.GetCustomAttribute<MinimumValueAttribute> ()?.Value ?? (isInteger ? 0 : 0.0);
		double max = member.GetCustomAttribute<MaximumValueAttribute> () is { } maxAttr ? maxAttr.Value : 100;
		double increment = member.GetCustomAttribute<IncrementValueAttribute> ()?.Value ?? (isInteger ? 1 : 0.01);

		double current = Convert.ToDouble (GetValue (data, member) ?? 0);

		StackPanel row = new () { Spacing = 2 };
		TextBlock valueLabel = new ();
		row.Children.Add (new TextBlock { Text = caption });
		Slider slider = new () {
			Minimum = min,
			Maximum = max,
			StepFrequency = increment,
			Value = current,
		};
		valueLabel.Text = isInteger ? $"{(int) current}" : $"{current:0.##}";
		slider.ValueChanged += (_, e) => {
			object newValue = isInteger ? (object) (int) Math.Round (e.NewValue) : e.NewValue;
			valueLabel.Text = isInteger ? $"{(int) Math.Round (e.NewValue)}" : $"{e.NewValue:0.##}";
			SetValue (data, member, newValue);
		};
		Grid grid = new ();
		grid.ColumnDefinitions.Add (new ColumnDefinition { Width = new GridLength (1, GridUnitType.Star) });
		grid.ColumnDefinitions.Add (new ColumnDefinition { Width = GridLength.Auto });
		Grid.SetColumn (slider, 0);
		Grid.SetColumn (valueLabel, 1);
		valueLabel.MinWidth = 40;
		valueLabel.Margin = new Thickness (8, 0, 0, 0);
		grid.Children.Add (slider);
		grid.Children.Add (valueLabel);
		row.Children.Add (grid);
		return row;
	}

	private static FrameworkElement CreateCheckRow (EffectData data, MemberInfo member, string caption)
	{
		CheckBox check = new () {
			Content = caption,
			IsChecked = (bool) (GetValue (data, member) ?? false),
		};
		check.Checked += (_, _) => SetValue (data, member, true);
		check.Unchecked += (_, _) => SetValue (data, member, false);
		return check;
	}

	private static FrameworkElement CreateEnumRow (EffectData data, MemberInfo member, string caption, Type enumType)
	{
		StackPanel row = new () { Spacing = 2 };
		row.Children.Add (new TextBlock { Text = caption });
		ComboBox combo = new () { HorizontalAlignment = HorizontalAlignment.Stretch };
		Array values = Enum.GetValues (enumType);
		foreach (object value in values)
			combo.Items.Add (AddSpaces (value.ToString () ?? string.Empty));
		combo.SelectedIndex = Array.IndexOf (values, GetValue (data, member));
		combo.SelectionChanged += (_, _) => {
			if (combo.SelectedIndex >= 0)
				SetValue (data, member, values.GetValue (combo.SelectedIndex));
		};
		row.Children.Add (combo);
		return row;
	}

	private static FrameworkElement CreateStaticListRow (EffectData data, MemberInfo member, string caption, StaticListAttribute listAttr)
	{
		StackPanel row = new () { Spacing = 2 };
		row.Children.Add (new TextBlock { Text = caption });
		ComboBox combo = new () { HorizontalAlignment = HorizontalAlignment.Stretch };

		// The attribute names a static member on the data type holding the choices.
		string[] choices =
			data.GetType ().GetProperty (listAttr.DictionaryName, BindingFlags.Public | BindingFlags.Static)?.GetValue (null) is IEnumerable<string> items
			? [.. items]
			: [];
		foreach (string choice in choices)
			combo.Items.Add (choice);
		combo.SelectedIndex = Array.IndexOf (choices, (string?) GetValue (data, member) ?? string.Empty);
		combo.SelectionChanged += (_, _) => {
			if (combo.SelectedIndex >= 0)
				SetValue (data, member, choices[combo.SelectedIndex]);
		};
		row.Children.Add (combo);
		return row;
	}

	private static FrameworkElement CreateAngleRow (EffectData data, MemberInfo member, string caption)
	{
		StackPanel row = new () { Spacing = 2 };
		row.Children.Add (new TextBlock { Text = caption });
		DegreesAngle current = (DegreesAngle) (GetValue (data, member) ?? new DegreesAngle (0));
		Slider slider = new () {
			Minimum = 0,
			Maximum = 360,
			StepFrequency = 1,
			Value = current.Degrees,
		};
		slider.ValueChanged += (_, e) => SetValue (data, member, new DegreesAngle (e.NewValue));
		row.Children.Add (slider);
		return row;
	}

	private static FrameworkElement CreateSeedRow (EffectData data, MemberInfo member, string caption)
	{
		StackPanel row = new () { Orientation = Orientation.Horizontal, Spacing = 8 };
		row.Children.Add (new TextBlock { Text = caption, VerticalAlignment = VerticalAlignment.Center });
		Button reseed = new () { Content = "Reseed" };
		Random random = new ();
		reseed.Click += (_, _) => SetValue (data, member, new RandomSeed (random.Next ()));
		row.Children.Add (reseed);
		return row;
	}
}
