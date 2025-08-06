import { useTranslation } from "react-i18next";
import { useStyles } from "./style";
import type { FeedbackResult } from "../../types";

interface FeedbackDisplayProps {
  feedbackResult: FeedbackResult;
  onTryAgain: () => void;
  onNextExercise: () => void;
  onChangeLevel: () => void;
}

export const FeedbackDisplay = ({
  feedbackResult,
  onTryAgain,
  onNextExercise,
  onChangeLevel,
}: FeedbackDisplayProps) => {
  const { t } = useTranslation();
  const classes = useStyles();

  const getAccuracyClass = (accuracy: number) => {
    if (accuracy >= 80) return classes.accuracyHigh;
    if (accuracy >= 60) return classes.accuracyMedium;
    return classes.accuracyLow;
  };

  return (
    <div className={classes.feedbackSection}>
      <div className={classes.feedbackHeader}>
        <h4 className={classes.feedbackTitle}>{t('pages.typingPractice.yourResults')}</h4>
        <div
          className={`${classes.accuracyBadge} ${getAccuracyClass(feedbackResult.accuracy)}`}
        >
          {feedbackResult.accuracy}% {t('pages.typingPractice.accuracy')}
        </div>
      </div>

      <div className={classes.textComparison}>
        <div>
          <div className={classes.comparisonLabel}>{t('pages.typingPractice.whatYouTyped')}</div>
          <div className={classes.comparisonText}>
            {feedbackResult.userInput || t('pages.typingPractice.empty')}
          </div>
        </div>

        <div>
          <div className={classes.comparisonLabel}>{t('pages.typingPractice.expectedText')}</div>
          <div className={classes.expectedText}>
            {feedbackResult.characterComparison.map((char, index) => (
              <span
                key={index}
                className={
                  char.isCorrect
                    ? classes.characterCorrect
                    : classes.characterIncorrect
                }
              >
                {char.char}
              </span>
            ))}
          </div>
        </div>
      </div>

      <div className={classes.exerciseControls}>
        <button className={classes.controlButton} onClick={onTryAgain}>
          {t('pages.typingPractice.tryAgain')}
        </button>
        <button
          className={`${classes.controlButton} ${classes.primaryControlButton}`}
          onClick={onNextExercise}
        >
          {t('pages.typingPractice.nextExercise')}
        </button>
        <button className={classes.controlButton} onClick={onChangeLevel}>
          {t('pages.typingPractice.changeLevel')}
        </button>
      </div>
    </div>
  );
};
