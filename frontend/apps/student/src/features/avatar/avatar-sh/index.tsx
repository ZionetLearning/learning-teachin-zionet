import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useAvatarSpeech } from "@student/hooks";
import avatar from "@student/assets/avatar.svg";
import { lipsArray } from "@student/assets/lips";
import { useStyles } from "./style";

export const AvatarSh = () => {
  const { t } = useTranslation();
  const classes = useStyles();
  const { currentVisemeSrc, speak } = useAvatarSpeech({ lipsArray });
  const [text, setText] = useState("");

  return (
    <div data-testid="avatar-sh-page">
      <div className={classes.wrapper}>
        <img
          src={avatar}
          alt="Avatar"
          className={classes.avatar}
          data-testid="avatar-sh-avatar"
        />

        <img
          src={currentVisemeSrc}
          alt="Lips"
          className={classes.lipsImage}
          data-testid="avatar-sh-lips"
        />
      </div>
      <div style={{ marginTop: "20px" }}>
        <input
          type="text"
          placeholder={t("pages.avatarSh.writeSomethingHereInHebrew")}
          value={text}
          onChange={(e) => setText(e.target.value)}
          className={classes.input}
          dir="rtl"
          data-testid="avatar-sh-input"
        />
        <br />
        <button
          onClick={() => speak(text)}
          className={classes.button}
          data-testid="avatar-sh-speak"
        >
          {t("pages.avatarSh.speak")}
        </button>
      </div>
    </div>
  );
};
