import { useState } from "react";
import * as sdk from "microsoft-cognitiveservices-speech-sdk";
import avatar from "./assets/avatar.svg";

import { useStyles } from "./style";

type SvgModule = { default: string };

const lips = import.meta.glob("./assets/lips/*.svg", { eager: true });

const lipsArray = Object.values(lips).map((mod) => (mod as SvgModule).default);

// מיפוי ישיר מ-Viseme ID לתמונה
const visemeMap: Record<number, string> = lipsArray.reduce(
  (acc, curr, idx) => {
    acc[idx] = curr;
    return acc;
  },
  {} as Record<number, string>
);

export const AvatarSh = () => {
  const classes = useStyles();
  const [currentViseme, setCurrentViseme] = useState<number>(0);
  const [text, setText] = useState("");

  const speakWithAzure = () => {
    if (!text.trim()) return;

    const speechKey = import.meta.env.VITE_AZURE_SPEECH_KEY!;
    const speechRegion = import.meta.env.VITE_AZURE_REGION!;
    const speechConfig = sdk.SpeechConfig.fromSubscription(
      speechKey,
      speechRegion
    );

    speechConfig.speechSynthesisVoiceName = "he-IL-HilaNeural";
    speechConfig.setProperty(
      "SpeechServiceConnection_SynthVoiceVisemeEvent",
      "true"
    );

    const audioConfig = sdk.AudioConfig.fromDefaultSpeakerOutput();
    const synthesizer = new sdk.SpeechSynthesizer(speechConfig, audioConfig);

    const visemes: { offset: number; visemeId: number }[] = [];

    synthesizer.visemeReceived = (_, e) => {
      console.log(
        `Viseme ID: ${e.visemeId}, offset: ${e.audioOffset / 10000}ms`
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
      }
    );
  };

  return (
    <div>
      <div className={classes.wrapper}>
        <img src={avatar} alt="Avatar" className={classes.avatar} />
        <img
          src={visemeMap[currentViseme]}
          alt="Lips"
          className={classes.lipsImage}
        />
      </div>
      <div style={{ marginTop: "20px" }}>
        <input
          type="text"
          placeholder="כתוב פה משהו בעברית"
          value={text}
          onChange={(e) => setText(e.target.value)}
          className={classes.input}
          dir="rtl"
        />
        <br />
        <button onClick={speakWithAzure} className={classes.button}>
          דברי
        </button>
      </div>
    </div>
  );
};
