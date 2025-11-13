import { useState, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";
import { Button, CircularProgress } from "@mui/material";
import { useAvatarSpeech } from "@student/hooks";
import { useSubmitGameAttempt } from "@student/api";
import { useAuth } from "@app-providers";
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
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const studentId = user?.userId ?? "";

  const [phase, setPhase] = useState<
    "ready" | "playing" | "typing" | "feedback"
  >("ready");
  const [userInput, setUserInput] = useState("");
  const [feedbackResult, setFeedbackResult] = useState<FeedbackResult | null>(
    null,
  );
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { mutateAsync: submitAttempt } = useSubmitGameAttempt();
  const correctSentence = retryData.correctAnswer.join(" ");

  const { speak, stop, isPlaying, error } = useAvatarSpeech({
    volume: 1,
    onAudioStart: () => {
      setPhase("playing");
    },
    onAudioEnd: () => {
      setPhase("typing");
    },
  });

  const handlePlayAudio = useCallback(() => {
    if (isPlaying) {
      stop();
      return;
    }
    speak(correctSentence);
  }, [isPlaying, stop, speak, correctSentence]);

  const handleReplayAudio = useCallback(() => {
    if (isPlaying) {
      stop();
      return;
    }
    speak(correctSentence);
  }, [isPlaying, stop, speak, correctSentence]);

  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setUserInput(event.target.value);
  };

  const handleSubmitAnswer = async () => {
    if (!userInput.trim()) return;

    setIsSubmitting(true);
    const localFeedback = compareTexts(userInput, correctSentence);

    try {
      const res = await submitAttempt({
        exerciseId: retryData.exerciseId,
        givenAnswer: [userInput],
      });

      const updatedFeedback = {
        ...localFeedback,
        accuracy: res.accuracy,
      };

      setFeedbackResult(updatedFeedback);
      setPhase("feedback");

      if (res.status === "Success") {
        queryClient.invalidateQueries({
          queryKey: ["gamesMistakes", { studentId }],
        });
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

  const handleBackToMistakes = () => {
    navigate("/practice-mistakes");
  };

  const audioState = {
    isPlaying,
    hasPlayed: phase !== "ready",
    error: error
      ? error instanceof Error
        ? error.message
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
                  onNextExercise={handleBackToMistakes}
                />
              )}
            </div>
          </div>

          <div className={classes.backButtonWrapper}>
            <Button variant="outlined" onClick={handleBackToMistakes}>
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
