using System;
using System.Linq;
using CodeBrix.Platform.GameEngine.Assets.Providers;
using CodeBrix.Platform.GameEngine.Audio;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Assets.Tests;

public class SoundEffectsTests : IDisposable
{
    public void Dispose()
    {
        foreach (string key in SoundEffects.Keys.Values) { AudioResourceManager.Instance.Unload(key); }
        AudioSystem.Shutdown();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Keys_cover_every_sound_effect_once() =>
        SoundEffects.Keys.Keys.OrderBy(effect => effect).Should().Equal(Enum.GetValues<SoundEffect>());

    [Fact]
    public void Keys_are_distinct_and_all_come_from_AssetKeys_Sfx()
    {
        //Arrange
        var sfxConstants = AssetKeyCatalog.AllKeys.Where(key => typeof(AssetKeys.Sfx)
            .GetFields().Any(field => (string)field.GetRawConstantValue() == key)).ToList();

        //Act
        var keys = SoundEffects.Keys.Values.ToList();

        //Assert
        keys.Should().OnlyHaveUniqueItems();
        keys.Should().BeEquivalentTo(sfxConstants);
    }

    [Fact]
    public void KeyOf_rejects_an_undefined_value() =>
        ((Action)(() => SoundEffects.KeyOf((SoundEffect)999))).Should().Throw<ArgumentOutOfRangeException>();

    [Fact]
    public void every_sound_key_resolves_as_audio()
    {
        //Arrange
        TestAssets.Register();
        GameAssetProviderRegistry providers = TestAssets.Engine.Managers.AssetProviders;

        //Act
        var notAudio = SoundEffects.Keys.Values
            .Where(key => !providers.TryDescribe(key, out GameAssetDescriptor descriptor) || descriptor.Kind != GameAssetKind.Audio)
            .ToList();

        //Assert
        notAudio.Should().BeEmpty();
    }

    [Fact]
    public void LoadSounds_registers_every_effect_under_its_key()
    {
        //Arrange
        TestAssets.Register();

        //Act
        BrixInvadersSounds sounds = BrixInvadersAssets.LoadSounds(TestAssets.Engine);

        //Assert
        sounds.Resources.Should().HaveCount(Enum.GetValues<SoundEffect>().Length);
        foreach (SoundEffect effect in Enum.GetValues<SoundEffect>())
        {
            string key = BrixInvadersSounds.KeyOf(effect);
            sounds[effect].Key.Should().Be(key);
            AudioResourceManager.Instance.TryGet(key, out AudioResource registered).Should().BeTrue();
            registered.Should().BeSameAs(sounds[effect]);
            sounds[effect].Duration.Should().BeGreaterThan(TimeSpan.Zero);
        }
    }
}
