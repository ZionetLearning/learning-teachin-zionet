import * as sdk from "microsoft-cognitiveservices-speech-sdk";

export const speakHebrew = async (text: string, voiceName?: string): Promise<void> => {
    if (!text.trim()) {
        throw new Error("Text cannot be empty");
    }

    const speechKey = import.meta.env.VITE_AZURE_SPEECH_KEY;
    const speechRegion = import.meta.env.VITE_AZURE_REGION;

    if (!speechKey || !speechRegion) {
        throw new Error("Azure Speech credentials not configured");
    }

    const speechConfig = sdk.SpeechConfig.fromSubscription(speechKey, speechRegion);
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
            }
        );
    });
};