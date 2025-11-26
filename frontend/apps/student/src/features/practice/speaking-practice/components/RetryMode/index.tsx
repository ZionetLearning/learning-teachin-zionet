import { useState, useEffect, useRef } from "react";
import { useTranslation } from "react-i18next";
import { Button, CircularProgress } from "@mui/material";
import * as sdk from "microsoft-cognitiveservices-speech-sdk";
import {
  useAvatarSpeech,
  useGameSubmission,
  useRetryNavigation,
  useTrackAchievement,
} from "@student/hooks";
import { useAzureSpeechToken } from "@student/api";
import { comparePhrases } from "../../utils";
import { toast } from "react-toastify";
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

const Feedback = {
  Perfect: "Perfect!",
  TryAgain: "Try again, that was not accurate.",
  RecognitionError: "Speech recognition error.",
  None: "",
} as const;

type FeedbackType = (typeof Feedback)[keyof typeof Feedback];

export const RetryMode = ({ retryData }: RetryModeProps) => {
  const { t } = useTranslation();
  const classes = useStyles();

  const [feedback, setFeedback] = useState<FeedbackType>(Feedback.None);
  const [isCorrect, setIsCorrect] = useState<boolean | null>(null);
  const [isRecording, setIsRecording] = useState(false);

  const correctSentence = retryData.correctAnswer.join(" ");

  const { submitAttempt } = useGameSubmission();
  const { navigateToMistakes } = useRetryNavigation();
  const { track } = useTrackAchievement("PracticeMistakes");

  const recognizerRef = useRef<sdk.SpeechRecognizer | null>(null);
  const audioConfigRef = useRef<sdk.AudioConfig | null>(null);
  const speechConfigRef = useRef<sdk.SpeechConfig | null>(null);

  const { data: azureSpeechToken, isLoading: tokenLoading } =
    useAzureSpeechToken();

  useEffect(
    function loadAzureSpeechToken() {
      if (!azureSpeechToken?.token || !azureSpeechToken?.region) return;
      if (!speechConfigRef.current) {
        const cfg = sdk.SpeechConfig.fromAuthorizationToken(
          azureSpeechToken.token,
          azureSpeechToken.region,
        );
        cfg.speechSynthesisVoiceName = "he-IL-HilaNeural";
        cfg.speechRecognitionLanguage = "he-IL";
        speechConfigRef.current = cfg;
        return;
      }
      try {
        if (
          (
            speechConfigRef.current as sdk.SpeechConfig & {
              setAuthorizationToken?: (token: string) => void;
            }
          ).setAuthorizationToken
        ) {
          (
            speechConfigRef.current as sdk.SpeechConfig & {
              setAuthorizationToken: (token: string) => void;
            }
          ).setAuthorizationToken(azureSpeechToken.token);
        } else {
          (
            speechConfigRef.current as sdk.SpeechConfig & {
              authorizationToken: string;
            }
          ).authorizationToken = azureSpeechToken.token;
        }
      } catch {
        const cfg = sdk.SpeechConfig.fromAuthorizationToken(
          azureSpeechToken.token,
          azureSpeechToken.region,
        );
        cfg.speechSynthesisVoiceName = "he-IL-HilaNeural";
        cfg.speechRecognitionLanguage = "he-IL";
        speechConfigRef.current = cfg;
      }
    },
    [azureSpeechToken?.token, azureSpeechToken?.region],
  );

  const {
    speak,
    stop: stopSpeech,
    isPlaying,
    error,
  } = useAvatarSpeech({
    volume: 1,
  });

  const stopRecognition = () => {
    if (recognizerRef.current) {
      recognizerRef.current.close();
      recognizerRef.current = null;
    }
    if (audioConfigRef.current) {
      audioConfigRef.current.close();
      audioConfigRef.current = null;
    }
    setIsRecording(false);
  };

  const handleRecord = () => {
    const config = speechConfigRef.current;
    if (!config) {
      setFeedback(Feedback.None);
      return;
    }
    if (isRecording) {
      stopRecognition();
      return;
    }
    if (isPlaying) {
      stopSpeech();
    }
    setFeedback(Feedback.None);

    const audioConfig = sdk.AudioConfig.fromDefaultMicrophoneInput();
    const recognizer = new sdk.SpeechRecognizer(config, audioConfig);
    audioConfigRef.current = audioConfig;
    recognizerRef.current = recognizer;
    setIsRecording(true);

    recognizer.recognizeOnceAsync(
      async (result) => {
        const userText = result.text ?? "";
        const correct = userText
          ? comparePhrases(correctSentence, userText)
          : false;

        try {
          const res = await submitAttempt(retryData.exerciseId, [userText]);

          const isServerCorrect = res.status === "Success";
          setIsCorrect(isServerCorrect);
          setFeedback(isServerCorrect ? Feedback.Perfect : Feedback.TryAgain);

          if (isServerCorrect) {
            track(1);
            toast.success(
              `${Feedback.Perfect} - ${res.accuracy.toFixed(1)}% ${t("pages.speakingPractice.accuracy")}`,
            );
          } else {
            toast.error(
              `${Feedback.TryAgain} - ${res.accuracy.toFixed(1)}% ${t("pages.speakingPractice.accuracy")}`,
            );
          }
        } catch (error) {
          console.error("Failed to submit speaking practice attempt:", error);
          setIsCorrect(correct);
          setFeedback(correct ? Feedback.Perfect : Feedback.TryAgain);
        }

        stopRecognition();
      },
      (err) => {
        console.error("Recognition error:", err);
        setIsCorrect(false);
        setFeedback(Feedback.RecognitionError);
        stopRecognition();
      },
    );
  };

  const handlePlay = () => {
    if (isPlaying) {
      stopSpeech();
      return;
    }
    if (isRecording) stopRecognition();
    setFeedback(Feedback.None);
    speak(correctSentence);
  };

  if (tokenLoading) {
    return (
      <div className={classes.loader}>
        <CircularProgress />
      </div>
    );
  }

  return (
    <div className={classes.container} data-testid="speaking-practice-page">
      <div className={classes.main} data-testid="speaking-main">
        <h2 className={classes.phrase} data-testid="speaking-phrase">
          {correctSentence}
        </h2>

        <p
          className={`${classes.feedback} ${isCorrect ? "correct" : "incorrect"}`}
          data-testid="speaking-feedback"
        >
          {error ? "TTS error." : feedback}
        </p>
      </div>

      <div className={classes.controls} data-testid="speaking-controls">
        <button onClick={handlePlay} data-testid="speaking-play">
          {isPlaying
            ? t("pages.speakingPractice.stop")
            : t("pages.speakingPractice.play")}
        </button>
        <button
          onClick={handleRecord}
          data-testid="speaking-record"
          disabled={
            tokenLoading ||
            !azureSpeechToken?.token ||
            !azureSpeechToken?.region
          }
        >
          {isRecording
            ? t("pages.speakingPractice.stop")
            : t("pages.speakingPractice.record")}
        </button>
      </div>

      <div className={classes.backButtonWrapper}>
        <Button variant="outlined" onClick={navigateToMistakes}>
          {t("pages.practiceMistakes.title")}
        </Button>
      </div>
    </div>
  );
};
