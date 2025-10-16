using FluentAssertions;
using IntegrationTests.Fixtures;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Media;

[Collection("IntegrationTests")]
public class SpeechIntegrationTests(
    HttpClientFixture httpClientFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : MediaTestBase(httpClientFixture, outputHelper, signalRFixture)
{
    [Fact(DisplayName = "GET /media-manager/speech/token - Should return a token")]
    public async Task Get_Speech_Token_Should_Return_Token()
    {
        var tokenData = await GetSpeechTokenDataAsync();
        tokenData.Token.Should().NotBeNullOrWhiteSpace();
        tokenData.Region.Should().NotBeNullOrWhiteSpace();
    }

    [Fact(DisplayName = "Upload TTS audio using token, then feed to STT recognizer")]
    public async Task Token_Allows_TTS_And_STT_Roundtrip()
    {
        var tokenData = await GetSpeechTokenDataAsync();
        //log the token for debugging (it is short-lived)
        OutputHelper.WriteLine($"Obtained speech token: {tokenData}");
        tokenData.Token.Should().NotBeNullOrWhiteSpace();
        tokenData.Region.Should().NotBeNullOrWhiteSpace();
        
        var text = "hello world";

        // --- TTS synthesize into memory ---
        var speechConfig = SpeechConfig.FromAuthorizationToken(tokenData.Token, tokenData.Region);
        speechConfig.SpeechSynthesisVoiceName = "en-US-JennyNeural";

        var pullStream = AudioOutputStream.CreatePullStream();
        using var synthAudioConfig = AudioConfig.FromStreamOutput(pullStream);
        using var synthesizer = new SpeechSynthesizer(speechConfig, synthAudioConfig);

        var ttsResult = await synthesizer.SpeakTextAsync(text);
        ttsResult.Reason.Should().Be(ResultReason.SynthesizingAudioCompleted);

        // --- feed TTS output back to STT recognizer ---
        var pushStream = AudioInputStream.CreatePushStream();
        _ = Task.Run(() =>
        {
            try
            {
                pushStream.Write(ttsResult.AudioData);
                pushStream.Close();
            }
            catch (Exception ex)
            {
                OutputHelper.WriteLine("Failed to write to push stream");
                // Fail the test immediately if writing fails
                throw new InvalidOperationException("Failed to write TTS audio into the STT input stream.", ex);
            }
        });

        using var sttAudioConfig = AudioConfig.FromStreamInput(pushStream);
        using var recognizer = new SpeechRecognizer(speechConfig, sttAudioConfig);

        var sttResult = await recognizer.RecognizeOnceAsync();

        // --- Assertions ---
        sttResult.Should().NotBeNull();
        sttResult.Reason.Should().Be(ResultReason.RecognizedSpeech);
        sttResult.Text.Should().NotBeNullOrEmpty();
        sttResult.Text.Should().ContainEquivalentOf("hello world"); // case-insensitive contain
    }

}
