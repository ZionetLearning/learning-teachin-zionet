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
    onChangeLevel
}: FeedbackDisplayProps) => {
    const classes = useStyles();

    const getAccuracyClass = (accuracy: number) => {
        if (accuracy >= 80) return classes.accuracyHigh;
        if (accuracy >= 60) return classes.accuracyMedium;
        return classes.accuracyLow;
    };

    return (
        <div className={classes.feedbackSection}>
            <div className={classes.feedbackHeader}>
                <h4 className={classes.feedbackTitle}>Your Results</h4>
                <div className={`${classes.accuracyBadge} ${getAccuracyClass(feedbackResult.accuracy)}`}>
                    {feedbackResult.accuracy}% Accuracy
                </div>
            </div>

            <div className={classes.textComparison}>
                <div>
                    <div className={classes.comparisonLabel}>What you typed:</div>
                    <div className={classes.comparisonText}>
                        {feedbackResult.userInput || '(empty)'}
                    </div>
                </div>

                <div>
                    <div className={classes.comparisonLabel}>Expected text:</div>
                    <div className={classes.expectedText}>
                        {feedbackResult.characterComparison.map((char, index) => (
                            <span
                                key={index}
                                className={char.isCorrect ? classes.characterCorrect : classes.characterIncorrect}
                            >
                                {char.char}
                            </span>
                        ))}
                    </div>
                </div>
            </div>

            <div className={classes.exerciseControls}>
                <button
                    className={classes.controlButton}
                    onClick={onTryAgain}
                >
                    🔄 Try Again
                </button>
                <button
                    className={`${classes.controlButton} ${classes.primaryControlButton}`}
                    onClick={onNextExercise}
                >
                    ➡️ Next Exercise
                </button>
                <button
                    className={classes.controlButton}
                    onClick={onChangeLevel}
                >
                    📊 Change Level
                </button>
            </div>
        </div>
    );
};
