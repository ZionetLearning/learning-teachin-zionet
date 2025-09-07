using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using IntegrationTests.Fixtures;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Media;

[Collection("Shared test collection")]
public class MediaIntegrationTests(
    SharedTestFixture sharedFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : MediaTestBase(sharedFixture, outputHelper, signalRFixture), IAsyncLifetime
{
    [Fact(DisplayName = "GET /media-manager/speech/token - Should return a token")]
    public async Task Get_Speech_Token_Should_Return_Token()
    {
        var token = await GetSpeechTokenAsync();
        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "Upload TTS audio using token, then feed to STT recognizer")]
    public async Task Token_Allows_TTS_And_STT_Roundtrip()
    {
        var token = await GetSpeechTokenAsync();
        token.Should().NotBeNullOrWhiteSpace();

        var region = "eastus";

        // 1) Create audio by synthesizing "hello world"
        var audioConfig = await CreateSpeechAudioConfigAsync(token, region, "hello world");

        // 2) Recognize that audio again with STT (loopback test)
        var speechConfig = SpeechConfig.FromAuthorizationToken(token, region);
        using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);

        var result = await recognizer.RecognizeOnceAsync();

        result.Should().NotBeNull();
        result.Reason.Should().Be(ResultReason.RecognizedSpeech);
        result.Text.Should().NotBeNull();
        result.Text.Should().ContainEquivalentOf("hello world");
    }

    private static async Task<AudioConfig> CreateSpeechAudioConfigAsync(string token, string region, string text)
    {
        // Use the token to auth
        var speechConfig = SpeechConfig.FromAuthorizationToken(token, region);
        speechConfig.SpeechSynthesisVoiceName = "en-US-JennyNeural"; // pick a neural voice

        // We will synthesize into a memory stream and then push to STT recognizer
        var pullStream = AudioOutputStream.CreatePullStream();
        using var audioConfig = AudioConfig.FromStreamOutput(pullStream);
        using var synthesizer = new SpeechSynthesizer(speechConfig, audioConfig);

        var result = await synthesizer.SpeakTextAsync(text);
        result.Reason.Should().BeOneOf(ResultReason.SynthesizingAudioCompleted);
        
        // Copy audio bytes into a push stream so we can feed it back into STT recognizer
        var pushStream = AudioInputStream.CreatePushStream();
        _ = Task.Run(() =>
        {
            try
            {
                pushStream.Write(result.AudioData);
                pushStream.Close();
            }
            catch
            {
                /* ignore */
            }
        });

        return AudioConfig.FromStreamInput(pushStream);
    }

}
