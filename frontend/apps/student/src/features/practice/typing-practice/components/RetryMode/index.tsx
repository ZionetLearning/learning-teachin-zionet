import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Button, CircularProgress } from "@mui/material";
import {
  useGameSubmission,
  useRetryAudio,
  useRetryNavigation,
  useTrackAchievement,
} from "@student/hooks";
import { ACHIEVEMENT_INCREMENT } from "@student/constants";
import { compareTexts } from "../../utils";
import { FeedbackDisplay } from "../FeedbackDisplay";
import { AudioControls } from "../AudioControls";
import type { FeedbackResult } from "../../types";
import { useStyles } from "./style";

interface RetryData {
  exerciseId: string;
  correctAnswer: string[];
  mistakes: Array<{
    wrongAnswer: string[];
    accuracy: number;
  }>;
  difficulty: number;
}

interface RetryModeProps {
  retryData: RetryData;
}

export const RetryMode = ({ retryData }: RetryModeProps) => {
  const { t } = useTranslation();
  const classes = useStyles();

  const [phase, setPhase] = useState<
    "ready" | "playing" | "typing" | "feedback"
  >("ready");
  const [userInput, setUserInput] = useState("");
  const [feedbackResult, setFeedbackResult] = useState<FeedbackResult | null>(
    null,
  );
  const [isSubmitting, setIsSubmitting] = useState(false);

  const correctSentence = retryData.correctAnswer.join(" ");

  const { submitAttempt } = useGameSubmission();
  const { navigateToMistakes } = useRetryNavigation();
  const { track } = useTrackAchievement("PracticeMistakes");
  const { handlePlayAudio, handleReplayAudio, isPlaying, audioError } =
    useRetryAudio({
      sentence: correctSentence,
      onAudioStart: () => setPhase("playing"),
      onAudioEnd: () => setPhase("typing"),
    });

  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setUserInput(event.target.value);
  };

  const handleSubmitAnswer = async () => {
    if (!userInput.trim()) return;

    setIsSubmitting(true);
    const localFeedback = compareTexts(userInput, correctSentence);

    try {
      const res = await submitAttempt(retryData.exerciseId, [userInput]);

      const updatedFeedback = {
        ...localFeedback,
        accuracy: res.accuracy,
      };

      setFeedbackResult(updatedFeedback);
      setPhase("feedback");

      if (res.status === "Success") {
        track(ACHIEVEMENT_INCREMENT);
      }
    } catch (error) {
      console.error("Failed to submit typing practice attempt:", error);
      setFeedbackResult(localFeedback);
      setPhase("feedback");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleTryAgain = () => {
    setPhase("typing");
    setUserInput("");
    setFeedbackResult(null);
  };

  const audioState = {
    isPlaying,
    hasPlayed: phase !== "ready",
    error: audioError
      ? audioError instanceof Error
        ? audioError.message
        : "TTS error"
      : null,
  };

  return (
    <div className={classes.pageWrapper}>
      <div className={classes.container}>
        <div className={classes.content}>
          <div
            className={classes.exerciseArea}
            data-testid="typing-exercise-area"
          >
            <div className={classes.audioSection}>
              <AudioControls
                phase={phase}
                audioState={audioState}
                onPlayAudio={handlePlayAudio}
                onReplayAudio={handleReplayAudio}
              />

              {phase === "typing" && (
                <div
                  className={classes.typingInput}
                  data-testid="typing-input-wrapper"
                >
                  <input
                    type="text"
                    className={classes.typingInputField}
                    value={userInput}
                    onChange={handleInputChange}
                    onKeyDown={(e) => e.key === "Enter" && handleSubmitAnswer()}
                    placeholder={t("pages.typingPractice.typeHereWhatYouHeard")}
                    autoFocus
                    data-testid="typing-input"
                  />
                  <button
                    className={classes.typingSubmitButton}
                    onClick={handleSubmitAnswer}
                    disabled={!userInput.trim() || isSubmitting}
                    data-testid="typing-submit"
                  >
                    {t("pages.typingPractice.submitAnswer")}
                  </button>
                </div>
              )}

              {phase === "feedback" && feedbackResult && (
                <FeedbackDisplay
                  feedbackResult={feedbackResult}
                  onTryAgain={handleTryAgain}
                  onNextExercise={navigateToMistakes}
                />
              )}
            </div>
          </div>

          <div className={classes.backButtonWrapper}>
            <Button variant="outlined" onClick={navigateToMistakes}>
              {t("pages.practiceMistakes.title")}
            </Button>
          </div>
        </div>

        {isSubmitting && (
          <div className={classes.loadingOverlay}>
            <CircularProgress />
          </div>
        )}
      </div>
    </div>
  );
};
