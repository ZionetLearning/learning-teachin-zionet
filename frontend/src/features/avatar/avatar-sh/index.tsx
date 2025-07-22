import { useEffect, useState } from "react";
import avatar from "./assets/avatar.svg";
import { useStyles } from "./style";

type SvgModule = { default: string };

const lips = import.meta.glob("./assets/lips/*.svg", { eager: true });

const lipsArray = Object.values(lips).map((mod) => (mod as SvgModule).default);

export const AvatarSh = () => {
  const classes = useStyles();

  const [isSpeaking, setIsSpeaking] = useState(false);
  const [currentLips, setCurrentLips] = useState(lipsArray[0]);
  const [text, setText] = useState("");

  useEffect(() => {
    if (!isSpeaking) {
      setCurrentLips(lipsArray[0]);
      return;
    }
    let index = 0;
    const interval = setInterval(() => {
      index = (index + 1) % lipsArray.length;
      setCurrentLips(lipsArray[index]);
    }, 100);
    return () => clearInterval(interval);
  }, [isSpeaking]);

  const speak = () => {
    if (!text.trim()) return;

    const utterance = new SpeechSynthesisUtterance(text);
    utterance.lang = "en-US";
    utterance.rate = 0.95;
    utterance.pitch = 15;

    const handleVoiceAssignment = () => {
      const voices = window.speechSynthesis.getVoices();

      const ziraVoice = voices.find(
        (v) => v.name === "Microsoft Zira - English (United States)"
      );
      if (ziraVoice) {
        utterance.voice = ziraVoice;
      } else {
        console.warn("Zira not found, using default voice");
      }

      utterance.onstart = () => setIsSpeaking(true);
      utterance.onend = () => {
        setIsSpeaking(false);
        window.speechSynthesis.cancel(); // force stop to cut trailing audio artifacts
      };
      window.speechSynthesis.speak(utterance);
    };

    if (window.speechSynthesis.getVoices().length === 0) {
      window.speechSynthesis.addEventListener(
        "voiceschanged",
        handleVoiceAssignment,
        { once: true }
      );
    } else {
      handleVoiceAssignment();
    }
  };

  return (
    <div>
      <div className={classes.wrapper}>
        <img src={avatar} alt="Avatar" className={classes.avatar} />
        <img src={currentLips} alt="Lips" className={classes.lipsImage} />
      </div>
      <div style={{ marginTop: "20px" }}>
        <input
          type="text"
          placeholder="Write something"
          value={text}
          onChange={(e) => setText(e.target.value)}
          className={classes.input}
        />
        <br />
        <button onClick={speak} className={classes.button}>
          Speak
        </button>
      </div>
    </div>
  );
};
