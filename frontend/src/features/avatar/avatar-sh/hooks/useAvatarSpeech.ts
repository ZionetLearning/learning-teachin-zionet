import { useState, useMemo } from "react";
import * as sdk from "microsoft-cognitiveservices-speech-sdk";

export const useAvatarSpeech = (lipsArray: string[]) => {
  const [currentViseme, setCurrentViseme] = useState<number>(0);

  const visemeMap = useMemo(() => {
    return lipsArray.reduce(
      (acc, path, index) => {
        acc[index] = path;
        return acc;
      },
      {} as Record<number, string>,
    );
  }, [lipsArray]);

  const speak = (text: string) => {
    if (!text.trim()) return;

    const speechKey = import.meta.env.VITE_AZURE_SPEECH_KEY!;
    const speechRegion = import.meta.env.VITE_AZURE_REGION!;
    const speechConfig = sdk.SpeechConfig.fromSubscription(
      speechKey,
      speechRegion,
    );

    speechConfig.speechSynthesisVoiceName = "he-IL-HilaNeural";
    speechConfig.setProperty(
      "SpeechServiceConnection_SynthVoiceVisemeEvent",
      "true",
    );

    const audioConfig = sdk.AudioConfig.fromDefaultSpeakerOutput();
    const synthesizer = new sdk.SpeechSynthesizer(speechConfig, audioConfig);

    const visemes: { offset: number; visemeId: number }[] = [];

    synthesizer.visemeReceived = (_, e) => {
      visemes.push({ visemeId: e.visemeId, offset: e.audioOffset / 10000 });
    };

    synthesizer.synthesisCompleted = () => {
      setCurrentViseme(0);
      synthesizer.close();
    };

    synthesizer.speakTextAsync(
      text,
      () => {
        if (visemes.length) {
          visemes.forEach(({ visemeId, offset }) => {
            setTimeout(() => {
              setCurrentViseme(visemeId);
            }, offset);
          });

          const totalDuration = Math.max(...visemes.map((v) => v.offset));
          setTimeout(() => setCurrentViseme(0), totalDuration + 500);
        }
      },
      (err) => {
        console.error("Speech error:", err);
        setCurrentViseme(0);
        synthesizer.close();
      },
    );
  };

  return {
    currentViseme,
    speak,
    currentVisemeSrc: visemeMap[currentViseme] ?? lipsArray[0], // fallback to neutral
  };
};

/*
  const speakWithAzure = () => {
    if (!text.trim()) return;

    const speechKey = import.meta.env.VITE_AZURE_SPEECH_KEY!;
    const speechRegion = import.meta.env.VITE_AZURE_REGION!;
    const speechConfig = sdk.SpeechConfig.fromSubscription(
      speechKey,
      speechRegion,
    );

    speechConfig.speechSynthesisVoiceName = "he-IL-HilaNeural";
    speechConfig.setProperty(
      "SpeechServiceConnection_SynthVoiceVisemeEvent",
      "true",
    );

    const audioConfig = sdk.AudioConfig.fromDefaultSpeakerOutput();
    const synthesizer = new sdk.SpeechSynthesizer(speechConfig, audioConfig);

    const visemes: { offset: number; visemeId: number }[] = [];

    synthesizer.visemeReceived = (_, e) => {
      console.log(
        `Viseme ID: ${e.visemeId}, offset: ${e.audioOffset / 10000}ms`,
      );
      visemes.push({ visemeId: e.visemeId, offset: e.audioOffset / 10000 });
    };

    synthesizer.synthesisCompleted = () => {
      console.log("Finished speaking");
      setCurrentViseme(0);
      synthesizer.close();
    };

    synthesizer.speakTextAsync(
      text,
      () => {
        if (visemes.length) {
          visemes.forEach(({ visemeId, offset }) => {
            setTimeout(() => {
              setCurrentViseme(visemeId);
            }, offset);
          });

          const totalDuration = Math.max(...visemes.map((v) => v.offset));
          setTimeout(() => setCurrentViseme(0), totalDuration + 500);
        }
      },
      (err) => {
        console.error("Speech error:", err);
        setCurrentViseme(0);
        synthesizer.close();
      },
    );
  };
*/
