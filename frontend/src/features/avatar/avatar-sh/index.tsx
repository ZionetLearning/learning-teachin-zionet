import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useAvatarSpeech } from "@/hooks";
import avatar from "@/assets/avatar.svg";
import { lipsArray } from "@/assets/lips";
import { useStyles } from "./style";

export const AvatarSh = () => {
  const { t } = useTranslation();
  const classes = useStyles();
  const { currentVisemeSrc, speak } = useAvatarSpeech(lipsArray);
  const [text, setText] = useState("");

  return (
    <div>
      <div className={classes.wrapper}>
        <img src={avatar} alt="Avatar" className={classes.avatar} />

        <img src={currentVisemeSrc} alt="Lips" className={classes.lipsImage} />
      </div>
      <div style={{ marginTop: "20px" }}>
        <input
          type="text"
          placeholder={t("pages.avatarSh.writeSomethingHereInHebrew")}
          value={text}
          onChange={(e) => setText(e.target.value)}
          className={classes.input}
          dir="rtl"
        />
        <br />
        <button onClick={() => speak(text)} className={classes.button}>
          {t("pages.avatarSh.speak")}
        </button>
      </div>
    </div>
  );
};
