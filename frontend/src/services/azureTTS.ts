import * as sdk from "microsoft-cognitiveservices-speech-sdk";

export const speakHebrew = async (
  text: string,
  voiceName?: string,
  ratePercent?: number,
): Promise<void> => {
  if (!text.trim()) {
    throw new Error("Text cannot be empty");
  }

  const speechKey = import.meta.env.VITE_AZURE_SPEECH_KEY;
  const speechRegion = import.meta.env.VITE_AZURE_REGION;

  if (!speechKey || !speechRegion) {
    throw new Error("Azure Speech credentials not configured");
  }

  const speechConfig = sdk.SpeechConfig.fromSubscription(
    speechKey,
    speechRegion,
  );
  speechConfig.speechSynthesisVoiceName = voiceName || "he-IL-HilaNeural";

  const synthesizer = new sdk.SpeechSynthesizer(speechConfig);
  const ssml = buildSsml(text, speechConfig.speechSynthesisVoiceName!, ratePercent ?? 0);

  /*return new Promise((resolve, reject) => {
    synthesizer.speakTextAsync(
      text,
      () => {
        synthesizer.close();
        resolve();
      },
      (err) => {
        synthesizer.close();
        reject(new Error(err));
      },
    );
  });*/
  return new Promise((resolve, reject) => {
    synthesizer.speakSsmlAsync(
      ssml,
      () => { synthesizer.close(); resolve(); },
      (err) => { synthesizer.close(); reject(new Error(err)); }
    );
  });
};

function buildSsml(text: string, voiceName = "he-IL-HilaNeural", ratePercent = 0) {
  return `
<speak version="1.0" xml:lang="he-IL">
  <voice name="${voiceName}">
    <prosody rate="${ratePercent}%">${text}</prosody>
  </voice>
</speak>`.trim();
}

export const speakHebrewNormal = (text: string, voiceName?: string) =>
  speakHebrew(text, voiceName, 0);

export const speakHebrewSlow = (text: string, voiceName?: string) =>
  speakHebrew(text, voiceName, -50);
