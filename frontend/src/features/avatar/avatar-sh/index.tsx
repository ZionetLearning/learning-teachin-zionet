import { useEffect, useState } from "react";
import avatar from "./assets/avatar.svg";

type SvgModule = { default: string };

const lips = import.meta.glob("./assets/lips/*.svg", { eager: true });

const lipsArray = Object.values(lips).map((mod) => (mod as SvgModule).default);

export const AvatarSh = () => {
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
    utterance.pitch = 10.1;

    const handleVoiceAssignment = () => {
      const voices = window.speechSynthesis.getVoices();
      console.log("Available voices:", voices.map((v) => v.name));
      const ziraVoice = voices.find(v => v.name === "Microsoft Zira - English (United States)");
      if (ziraVoice) {
        utterance.voice = ziraVoice;
      } else {
        console.warn("Zira not found, using default voice");
      }

      utterance.onstart = () => setIsSpeaking(true);
      utterance.onend = () => setIsSpeaking(false);
      window.speechSynthesis.speak(utterance);
    };

    if (window.speechSynthesis.getVoices().length === 0) {
      window.speechSynthesis.addEventListener("voiceschanged", handleVoiceAssignment, { once: true });
    } else {
      handleVoiceAssignment();
    }
  };

  return (
    <div style={{ textAlign: "center", marginTop: "40px" }}>
      <div style={{ position: "relative", width: "300px", height: "300px", margin: "0 auto" }}>
        <img src={avatar} alt="Avatar" style={{ width: "100%", height: "100%" }} />
        <img
          src={currentLips}
          alt="Lips"
          style={{
            position: "absolute",
            top: "42%",
            left: "40%",
            width: "20%",
            height: "20%",
            pointerEvents: "none",
          }}
        />
      </div>
      <div style={{ marginTop: "20px" }}>
        <input
          type="text"
          placeholder="Write something"
          value={text}
          onChange={(e) => setText(e.target.value)}
          style={{ fontSize: "18px", padding: "10px", width: "300px" }}
        />
        <br />
        <button
          onClick={speak}
          style={{
            marginTop: "10px",
            fontSize: "20px",
            padding: "10px 20px",
            cursor: "pointer",
          }}
        >
          Speak
        </button>
      </div>
    </div>
  );
};
