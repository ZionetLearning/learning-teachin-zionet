import { useState } from "react";
import { useAvatarSpeech } from "./hooks";
import avatar from "./assets/avatar.svg";
import { useStyles } from "./style";

type SvgModule = { default: string };
const lips = import.meta.glob("./assets/lips/*.svg", { eager: true });
const lipsArray = Object.values(lips).map((mod) => (mod as SvgModule).default);

export const AvatarSh = () => {
  const classes = useStyles();
  const { currentVisemeSrc, speak } = useAvatarSpeech(lipsArray);
  const [text, setText] = useState("");

  return (
    <div>
      <div className={classes.wrapper}>
        <img src={avatar} alt="Avatar" className={classes.avatar} />

        <img
          src={currentVisemeSrc}
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
        <button onClick={()=>speak(text)} className={classes.button}>
          דברי
        </button>
      </div>
    </div>
  );
}