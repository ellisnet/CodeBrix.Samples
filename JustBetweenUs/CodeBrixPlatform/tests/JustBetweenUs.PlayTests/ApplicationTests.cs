using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using JustBetweenUs.ViewModels;
using SilverAssertions;
using Xunit;

namespace JustBetweenUs.PlayTests;

public sealed partial class ApplicationTests : PageTest, IClassFixture<AppFixture>, IAsyncLifetime
{
    private readonly AppFixture _fixture;
    private const string Aes = "AES Standard Encryption (Secure)";
    private const string TripleDes = "Triple DES (Obsolete, insecure)";
    private const string Twofish = "Twofish Encryption (Very secure)";
    private const string AesVector = "b0i/ESAGVpJeXb4M1G6WSn90b9FD+USbDS0lfeqNmRXNYYfcRKDXiOGObJ+cPSsIdYD+icTy7wEgh0Kmgt2MZUqUenokqZFsd4LHsc9pEDpKo8GAOX1E0ec3PzWeHZyI4pSWeizD1BSyJwNCq7QP5gARE1/K955ROHA2zpjC9BV978S5rWDL0uXDVsKb9F1+yXiShh96fdU9WtJNIQFSTIVhRnJ34DhYOxXhHbVUS3bWNelhX08zwTC5d6DoqdOo2fAwWbsKJZcJSp088V+vKS9LPDfp/MVr+bohkrVPTMqmezyb4s8oCU/gNMIvR4XIxUotDFZuaaXx7oIvIiSHxMfah0paQFYelTeoLtlOIqWYaCHlNGqNlwq92Ht9JMToMmS8DaIJA5SO8Sz/nBiSMA==";
    private const string AesVectorPlaintext = "AES is an example of a 'symmetric-key' encryption algorithm; where both the sender and the receiver use the same key to encrypt/decrypt the message - i.e. it is not an example of a public/private key encryption algorithm.\nhttps://en.wikipedia.org/wiki/Symmetric-key_algorithm";
    private Locator Key => Page.GetByRole(AriaRole.Textbox, new() { Name = "Encryption key", Exact = true });
    private Locator Input => Page.GetByRole(AriaRole.Textbox, new() { Name = "Input text", Exact = true });
    private Locator Output => Page.GetByRole(AriaRole.Textbox, new() { Name = "Processed text", Exact = true });
    private Locator Encrypt => Page.GetByRole(AriaRole.Button, new() { Name = "Encrypt", Exact = true });
    private Locator Decrypt => Page.GetByRole(AriaRole.Button, new() { Name = "Decrypt", Exact = true });
    private Locator Copy => Page.GetByRole(AriaRole.Button, new() { Name = "Copy to Clipboard", Exact = true });
    private Locator Dialog => Page.GetByRole(AriaRole.Dialog);

    public ApplicationTests(AppFixture fixture) : base(fixture.Application) => _fixture = fixture;
    public async ValueTask InitializeAsync() => await _fixture.ResetAsync();
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private async Task CloseDialogAsync()
    {
        await Dialog.GetByRole(AriaRole.Button, new() { Name = "OK", Exact = true }).ClickAsync();
        await Expect(Dialog).ToHaveCountAsync(0);
    }

    private async Task SelectModeAsync(string mode)
    {
        await Page.GetByRole(AriaRole.Combobox, new() { Name = "Encryption mode", Exact = true }).ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = mode, Exact = true }).ClickAsync();
        // The bound view-model property proves selection reached the application.
        await _fixture.Application.WaitForAsync(() => ((MainViewModel)_fixture.View.DataContext).SelectedEncryptionMode.Description,
            actual => actual == mode, description: "selected encryption mode");
    }

    private async Task<string> EncryptAsync(string text)
    {
        await Input.FillAsync(text);
        await Encrypt.ClickAsync();
        await Expect(Output).ToHaveValueAsync(new Regex(@"^[A-Za-z0-9+/]+={0,2}$"));
        return await Output.InputValueAsync();
    }

    [Fact]
    public async Task Startup_loads_default_key_and_initial_command_states()
    {
        await Expect(Key).ToHaveValueAsync("27544076");
        await Expect(Input).ToHaveValueAsync("");
        await Expect(Output).ToHaveValueAsync("");
        await Expect(Encrypt).ToBeDisabledAsync();
        await Expect(Decrypt).ToBeDisabledAsync();
        await Expect(Copy).ToBeDisabledAsync();
        (await Page.EvaluateAsync(() => ((MainViewModel)_fixture.View.DataContext).SelectedEncryptionMode.Description)).Should().Be(Aes);
    }

    [Fact]
    public async Task Startup_information_dialog_can_be_dismissed()
    {
        await Page.SetContentAsync(() => new Views.MainPage());
        await Expect(Dialog).ToContainTextAsync("This application is adapted from a sample provided by Paul Ainsworth.");
        await CloseDialogAsync();
        await Expect(Input).ToBeVisibleAsync();
        await Input.FillAsync("The dialog no longer blocks the application.");
        await Expect(Encrypt).ToBeEnabledAsync();
    }

    [Theory]
    [InlineData("", "message")]
    [InlineData("   ", "message")]
    [InlineData("key", "")]
    [InlineData("key", " \t\n ")]
    public async Task Blank_key_or_message_disables_both_actions(string key, string message)
    {
        await Key.FillAsync(key);
        await Input.FillAsync(message);
        await Expect(Encrypt).ToBeDisabledAsync();
        await Expect(Decrypt).ToBeDisabledAsync();
        await Expect(Output).ToHaveValueAsync("");
    }

    [Theory]
    [InlineData(Aes)]
    [InlineData(TripleDes)]
    [InlineData(Twofish)]
    public async Task Every_algorithm_roundtrips_unicode_multiline_text(string mode)
    {
        const string message = "Just between us: café, Ελληνικά, 日本語, 🔐\nSecond line\n  Keep surrounding whitespace.  ";
        await SelectModeAsync(mode);
        await Key.FillAsync("PlayTest key");
        var encrypted = await EncryptAsync(message);
        encrypted.Should().NotBe(message);
        Convert.FromBase64String(encrypted).Should().NotBeEmpty();
        await Expect(Copy).ToBeEnabledAsync();
        await Input.FillAsync(encrypted);
        await Decrypt.ClickAsync();
        await Expect(Output).ToHaveValueAsync(message);
    }

    [Theory]
    [InlineData(Aes)]
    [InlineData(TripleDes)]
    [InlineData(Twofish)]
    public async Task Key_whitespace_is_trimmed_in_both_directions(string mode)
    {
        await SelectModeAsync(mode);
        await Key.FillAsync("  shared key  ");
        var encrypted = await EncryptAsync("Key trimming works.");
        await Key.FillAsync("shared key");
        await Input.FillAsync(encrypted);
        await Decrypt.ClickAsync();
        await Expect(Output).ToHaveValueAsync("Key trimming works.");
    }

    [Theory]
    [InlineData(Aes)]
    [InlineData(Twofish)]
    public async Task Randomized_algorithms_produce_distinct_results_for_repeated_plaintext(string mode)
    {
        await SelectModeAsync(mode);
        var first = await EncryptAsync("The same message twice.");
        await Encrypt.ClickAsync();
        await Expect(Output).Not.ToHaveValueAsync(first);
        var second = await Output.InputValueAsync();
        await Input.FillAsync(second);
        await Decrypt.ClickAsync();
        await Expect(Output).ToHaveValueAsync("The same message twice.");
    }

    [Fact]
    public async Task Triple_des_decrypts_existing_interoperability_vector()
    {
        await SelectModeAsync(TripleDes);
        await Input.FillAsync("8NyqFvVx6VYo17mdk0htiYWHZs+FD+vjp3oAdzIxWnNH62IQ/m0KJAgDa5lBOPfSQl4uov6LR6uq+wjYHwRCMypPxxJXJcuNnymQhSGKKrj9dB2OYdaUx8YwNd0nQi6Y1lr7EDthH4MMzuHdK6EOsgGVZNgbJs1gO+cidQX6BibawN/GD11N0gmFZ5WaGj+mpuqPwb+VK4ge3/CczcUEvAgpVIcdVY7f");
        await Decrypt.ClickAsync();
        await Expect(Output).ToHaveValueAsync("You have correctly decrypted the secret message!\nThe history of encryption and secret messages is VERY interesting:\nhttps://en.wikipedia.org/wiki/Encryption#History");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Aes_decrypts_existing_vector_and_recovers_after_wrong_key(bool tryWrongKey)
    {
        await Input.FillAsync(AesVector);
        if (tryWrongKey)
        {
            await Key.FillAsync("a deliberately incorrect key");
            await Decrypt.ClickAsync();
            await Expect(Dialog).ToContainTextAsync("Error while decrypting:");
            await Expect(Output).ToHaveValueAsync("");
            await CloseDialogAsync();
            await Key.FillAsync("27544076");
        }
        await Decrypt.ClickAsync();
        await Expect(Output).ToHaveValueAsync(AesVectorPlaintext);
    }

    [Fact]
    public async Task Long_message_roundtrips_without_truncation()
    {
        var message = string.Concat(System.Linq.Enumerable.Repeat("Line with Unicode café 🔐 and spaces.\n", 150));
        var encrypted = await EncryptAsync(message);
        await Input.FillAsync(encrypted);
        await Decrypt.ClickAsync();
        await Expect(Output).ToHaveValueAsync(message);
    }

    [Fact]
    public async Task Keyboard_paste_reads_the_isolated_application_clipboard()
    {
        var encrypted = await EncryptAsync("Copied then pasted.");
        await Copy.ClickAsync();
        await Expect(Dialog).ToContainTextAsync("copied to the system clipboard");
        await CloseDialogAsync();
        await Input.FillAsync("");
        await Input.PressAsync("ControlOrMeta+V");
        await Expect(Input).ToHaveValueAsync(encrypted);
        await Decrypt.ClickAsync();
        await Expect(Output).ToHaveValueAsync("Copied then pasted.");
    }

    [Fact]
    public async Task Plaintext_decryption_shows_information_and_preserves_previous_output()
    {
        var previous = await EncryptAsync("Preserve the previous result.");
        await Input.FillAsync("hello!");
        await Decrypt.ClickAsync();
        await Expect(Dialog).ToContainTextAsync("The specified text does not look like it is encrypted.");
        await Expect(Output).ToHaveValueAsync(previous);
        await CloseDialogAsync();
    }

    [Theory]
    [InlineData(Aes)]
    [InlineData(TripleDes)]
    [InlineData(Twofish)]
    public async Task Malformed_ciphertext_shows_error_then_app_can_recover(string mode)
    {
        await SelectModeAsync(mode);
        await Input.FillAsync("AQID");
        await Decrypt.ClickAsync();
        await Expect(Dialog).ToContainTextAsync("Error while decrypting:");
        await Expect(Output).ToHaveValueAsync("");
        await CloseDialogAsync();
        var encrypted = await EncryptAsync("Recovery after an error.");
        await Input.FillAsync(encrypted);
        await Decrypt.ClickAsync();
        await Expect(Output).ToHaveValueAsync("Recovery after an error.");
    }

    [Fact]
    public async Task Decryption_tolerates_clipboard_control_character_and_wrapped_base64()
    {
        var encrypted = await EncryptAsync("Clipboard transport preserves my message.");
        await Input.FillAsync("\u0001" + encrypted[..12] + "\n" + encrypted[12..]);
        await Decrypt.ClickAsync();
        await Expect(Output).ToHaveValueAsync("Clipboard transport preserves my message.");
    }

    [Fact]
    public async Task Copy_updates_clipboard_and_only_notifies_once_per_page()
    {
        var encrypted = await EncryptAsync("Copied content.");
        await Copy.ClickAsync();
        await Expect(Dialog).ToContainTextAsync("The processed text has been copied to the system clipboard.");
        (await Page.ClipboardTextAsync()).Should().Be(encrypted);
        await CloseDialogAsync();
        await Input.FillAsync(encrypted);
        await Decrypt.ClickAsync();
        await Expect(Output).ToHaveValueAsync("Copied content.");
        await Copy.ClickAsync();
        (await Page.ClipboardTextAsync()).Should().Be("Copied content.");
        await Expect(Dialog).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task Keyboard_editing_updates_bindings_and_commands()
    {
        await Input.FillAsync("replace me");
        await Input.PressAsync("ControlOrMeta+A");
        await Page.Keyboard.InsertTextAsync("typed message");
        await Expect(Input).ToHaveValueAsync("typed message");
        await Expect(Encrypt).ToBeEnabledAsync();
        await Input.PressAsync("ControlOrMeta+A");
        await Input.PressAsync("Backspace");
        await Expect(Input).ToHaveValueAsync("");
        await Expect(Encrypt).ToBeDisabledAsync();
    }

    [Fact]
    public async Task Processed_text_is_read_only_to_keyboard_input()
    {
        var encrypted = await EncryptAsync("Read-only output.");
        await Output.PressAsync("ControlOrMeta+A");
        await Output.PressAsync("Backspace");
        await Expect(Output).ToHaveValueAsync(encrypted);
    }

    [Fact]
    public async Task Operating_system_dialog_reports_real_application_information()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "Operating system information", Exact = true }).ClickAsync();
        await Expect(Dialog).ToContainTextAsync("Currently running on:");
        await Expect(Dialog).ToContainTextAsync("Operating system description:");
        await Expect(Dialog).ToContainTextAsync("DotNet version:");
        await CloseDialogAsync();
    }

    [Fact]
    public async Task Virtual_screen_and_png_have_the_fixed_selected_orientation()
    {
        var bytes = await Page.ScreenshotAsync(new() { Path = Path.Combine("TestResults", "PlayTest", "JustBetweenUs.png") });
        // PNG's IHDR stores width/height in network byte order at offsets 16/20.
        System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(16, 4)).Should().Be(_fixture.Application.Width);
        System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(20, 4)).Should().Be(_fixture.Application.Height);
        ((_fixture.Application.Width == 1920 && _fixture.Application.Height == 1080) ||
            (_fixture.Application.Width == 1080 && _fixture.Application.Height == 1920)).Should().BeTrue();
        await Expect(Encrypt).ToBeVisibleAsync();
        await Expect(Copy).ToBeVisibleAsync();
    }
}
