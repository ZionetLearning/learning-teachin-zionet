import { useTranslation } from "react-i18next";
import { useStyles } from "./style";

interface SpeakerProps {
  disabled?: boolean;
  onClick: () => void;
}

export const Speaker = ({ disabled = false, onClick }: SpeakerProps) => {
  const { t } = useTranslation();
  const classes = useStyles();
  const label = t("pages.wordOrderGame.playSentence");

  return (
    <div className={classes.speakerWrap}>
      <button
        type="button"
        className={classes.speakerBtn}
        onClick={onClick}
        aria-label={label}
        title={label}
        disabled={disabled}
      >
        {/* Speaker icon with waves */}
        <svg
          className={classes.icon}
          width="34"
          height="34"
          viewBox="0 0 24 24"
          aria-hidden="true"
        >
          {/* Speaker body */}
          <path
            d="M11 5L7.5 8H4a1 1 0 0 0-1 1v6a1 1 0 0 0 1 1h3.5L11 19V5z"
            fill="currentColor"
          />
          {/* Waves */}
          <path
            className="wave"
            d="M14 9c1.333 1.333 1.333 4.667 0 6"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.8"
            strokeLinecap="round"
          />
          <path
            className="wave"
            d="M16.5 7c2.5 2.5 2.5 7.5 0 10"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.6"
            strokeLinecap="round"
          />
        </svg>
      </button>

      <div className={classes.speakerLabel}>{label}</div>
    </div>
  );
};
