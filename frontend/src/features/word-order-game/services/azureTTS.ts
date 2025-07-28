import * as sdk from "microsoft-cognitiveservices-speech-sdk";

const VOICE_DEFAULT = "he-IL-HilaNeural";
const cache = new Map<string, string>();

const ssml = (text: string, voice = VOICE_DEFAULT, ratePercent = 0) => {
  return `
<speak version="1.0" xml:lang="he-IL">
  <voice name="${voice}">
    <prosody rate="${ratePercent}%">${text}</prosody>
  </voice>
</speak>`.trim();
}

const arrayBufferToUrl = (buf: ArrayBuffer, mime = "audio/mp3") => {
  const blob = new Blob([buf], { type: mime });
  return URL.createObjectURL(blob);
};

/*
  Plays a Hebrew sentence. On first play it uses the SDK (plays + caches).
  Next plays reuse the cached audio (no service call).
*/
export const playSentenceCached = async (
  text: string,
  voiceName?: string,
  ratePercent?: number
): Promise<void> => {
  const sentence = text.trim();
  if (!sentence) throw new Error("Text cannot be empty");

  const voice = voiceName || VOICE_DEFAULT;
  const rate = ratePercent ?? 0;
  const key = `${voice}|${rate}|${sentence}`;

  // Reuse cached audio if available
  const cachedUrl = cache.get(key);
  if (cachedUrl) {
    const audio = new Audio(cachedUrl);
    await audio.play();
    return;
  }

  const speechKey = import.meta.env.VITE_AZURE_SPEECH_KEY!;
  const speechRegion = import.meta.env.VITE_AZURE_REGION!;
  const speechConfig = sdk.SpeechConfig.fromSubscription(speechKey, speechRegion);
  speechConfig.speechSynthesisVoiceName = voice;

  const synthesizer = new sdk.SpeechSynthesizer(speechConfig);
  const textSsml = ssml(sentence, voice, rate);

  await new Promise<void>((resolve, reject) => {
    synthesizer.speakSsmlAsync(
      textSsml,
      (result) => {
        try {
          // Cache for future replays
          const url = arrayBufferToUrl(result.audioData);
          cache.set(key, url);
          resolve();
        } catch (e) {
          reject(e);
        } finally {
          synthesizer.close();
        }
      },
      (err) => {
        synthesizer.close();
        reject(new Error(err));
      }
    );
  });
}

// Clear all cached audio (call on "Next" or when leaving the page)
export const clearSpeechCache = () => {
  for (const url of cache.values()) URL.revokeObjectURL(url);
  cache.clear();
}

// clear cache for a specific sentence/voice/rate.
export const clearSentenceFromCache = (
  text: string,
  voiceName?: string,
  ratePercent?: number
) => {
  const key = `${voiceName || VOICE_DEFAULT}|${ratePercent ?? 0}|${text.trim()}`;
  const url = cache.get(key);
  if (url) {
    URL.revokeObjectURL(url);
    cache.delete(key);
  }
}
