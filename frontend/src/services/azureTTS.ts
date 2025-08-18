import * as sdk from "microsoft-cognitiveservices-speech-sdk";

export const speakHebrew = async (
  text: string,
  voiceName?: string,
): Promise<void> => {
  // In Cypress test environment, skip actual Azure TTS network call for determinism
  if (typeof window !== "undefined") {
    const w = window as Window & { Cypress?: boolean };
    if (w.Cypress) {
      return Promise.resolve();
    }
  }

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

  return new Promise((resolve, reject) => {
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
  });
};
