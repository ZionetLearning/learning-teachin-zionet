import { useTranslation } from "react-i18next";
import { useStyles } from "./style";
import type { DifficultyLevel } from "../../types";

interface LevelSelectionProps {
  onLevelSelect: (level: DifficultyLevel) => void;
  isLoading: boolean;
}

export const LevelSelection = ({
  onLevelSelect,
  isLoading,
}: LevelSelectionProps) => {
  const { t } = useTranslation();
  const classes = useStyles();

  const getLevelInfo = (level: DifficultyLevel) => {
    switch (level) {
      case "easy":
        return {
          icon: "🌱",
          label: "Easy",
          description: "Single Hebrew words\nBasic vocabulary",
        };
      case "medium":
        return {
          icon: "🌿",
          label: "Medium",
          description: "Short phrases\nCommon expressions",
        };
      case "hard":
        return {
          icon: "🌳",
          label: "Hard",
          description: "Full sentences\nComplex vocabulary",
        };
      default:
        return {
          icon: "🌱",
          label: "Easy",
          description: "Single Hebrew words",
        };
    }
  };

  return (
    <div className={classes.levelSelection}>
      <div>
        <h3 className={classes.levelTitle}>
          {t("pages.typingPractice.chooseYourDifficultyLevel")}
        </h3>
        <p className={classes.levelDescription}>
          {t("pages.typingPractice.selectDifficultyLevel")}
          <br />
          {t("pages.typingPractice.youWillListenToHebrewAudio")}
        </p>
      </div>

      <div className={classes.levelButtons}>
        {(["easy", "medium", "hard"] as DifficultyLevel[]).map((level) => {
          const levelInfo = getLevelInfo(level);
          return (
            <button
              key={level}
              className={classes.levelButton}
              onClick={() => onLevelSelect(level)}
              disabled={isLoading}
            >
              <div className={classes.levelButtonIcon}>{levelInfo.icon}</div>
              <div className={classes.levelButtonLabel}>{levelInfo.label}</div>
              <div className={classes.levelButtonDescription}>
                {levelInfo.description}
              </div>
            </button>
          );
        })}
      </div>
    </div>
  );
};
