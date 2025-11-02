/// <reference types="vite/client" />
import { useEffect, useMemo, useRef, useState } from "react";

import { CircularProgress } from "@mui/material";
import * as sdk from "microsoft-cognitiveservices-speech-sdk";
import { useTranslation } from "react-i18next";
import { comparePhrases } from "./utils";

import { useAzureSpeechToken, useGenerateSentences } from "@student/api";
import { useAvatarSpeech } from "@student/hooks";
import { DifficultyLevel } from "@student/types";
import {
  GameConfigModal,
  GameOverModal,
  GameSettings,
  GameSetupPanel,
} from "@ui-components";
import { ContextAwareChat, PageContext } from "@student/components";
import { getDifficultyLabel } from "../utils";
import { useStyles } from "./style";

const Feedback = {
  Perfect: "Perfect!",
  TryAgain: "Try again, that was not accurate.",
  RecognitionError: "Speech recognition error.",
  None: "",
} as const;

type FeedbackType = (typeof Feedback)[keyof typeof Feedback];

export const SpeakingPractice = () => {
  const classes = useStyles();
  const { t, i18n } = useTranslation();
  const isHebrew = i18n.language === "he" || i18n.language === "heb";
  const [currentIdx, setCurrentIdx] = useState(0);
  const [feedback, setFeedback] = useState<FeedbackType>(Feedback.None);
  const [isCorrect, setIsCorrect] = useState<boolean | null>(null);
  const [isRecording, setIsRecording] = useState(false);
  const [configModalOpen, setConfigModalOpen] = useState(true);
  const [gameOverOpen, setGameOverOpen] = useState(false);
  const [difficulty, setDifficulty] = useState<DifficultyLevel>(1);
  const [nikud, setNikud] = useState(true);
  const [count, setCount] = useState(3);
  const [sentences, setSentences] = useState<string[]>([]);
  const [attempted, setAttempted] = useState<Set<number>>(new Set());
  const [correctIdxs, setCorrectIdxs] = useState<Set<number>>(new Set());
  const [skipped, setSkipped] = useState<Set<number>>(new Set());
  const [isConfigured, setIsConfigured] = useState(false);

  const recognizerRef = useRef<sdk.SpeechRecognizer | null>(null);
  const audioConfigRef = useRef<sdk.AudioConfig | null>(null);
  const speechConfigRef = useRef<sdk.SpeechConfig | null>(null);
  //use useAzureSpeechToken here to get token and set it to speechConfig
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

  const generateMutation = useGenerateSentences();

  const requestSentences = (
    difficulty: DifficultyLevel,
    nikud: boolean,
    count: number,
  ) => {
    generateMutation.mutate(
      { difficulty, nikud, count },
      {
        onSuccess: (data) => {
          setSentences(data.map((item) => item.text));
          setCurrentIdx(0);
          setAttempted(new Set());
          setCorrectIdxs(new Set());
          setSkipped(new Set());
          setConfigModalOpen(false);
        },
        onError: (error) => {
          console.error("Error fetching sentences:", error);
          setSentences([]);
        },
      },
    );
  };

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

    setAttempted((prev) => {
      const next = new Set(prev);
      next.add(currentIdx);
      return next;
    });
    setSkipped((prev) => {
      const next = new Set(prev);
      next.delete(currentIdx);
      return next;
    });

    const audioConfig = sdk.AudioConfig.fromDefaultMicrophoneInput();
    const recognizer = new sdk.SpeechRecognizer(config, audioConfig);
    audioConfigRef.current = audioConfig;
    recognizerRef.current = recognizer;
    setIsRecording(true);

    recognizer.recognizeOnceAsync(
      (result) => {
        const userText = result.text ?? "";
        const correct = userText
          ? comparePhrases(sentences[currentIdx], userText)
          : false;
        setCorrectIdxs((prev) => {
          const next = new Set(prev);
          if (correct) next.add(currentIdx);
          else next.delete(currentIdx);
          return next;
        });
        setIsCorrect(correct);
        setFeedback(correct ? Feedback.Perfect : Feedback.TryAgain);
        stopRecognition();
        const total = sentences.length;
        const isLast = currentIdx === total - 1;

        const nextCorrect = new Set(correctIdxs);
        if (correct) nextCorrect.add(currentIdx);
        else nextCorrect.delete(currentIdx);

        const nextAttempted = new Set(attempted);
        nextAttempted.add(currentIdx);
        const nextSkipped = new Set(skipped);
        nextSkipped.delete(currentIdx);

        const allCorrect = nextCorrect.size === total;
        const noSkips = nextSkipped.size === 0 && nextAttempted.size === total;

        if (isLast && allCorrect && noSkips) {
          setGameOverOpen(true);
        }
      },
      (err) => {
        console.error("Recognition error:", err);
        setCorrectIdxs((prev) => {
          const next = new Set(prev);
          next.delete(currentIdx);
          return next;
        });
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
    speak(sentences[currentIdx]);
  };

  const goPrev = () => {
    stopSpeech();
    stopRecognition();
    setCurrentIdx((i) => Math.max(0, i - 1));
    setFeedback(Feedback.None);
    setIsCorrect(null);
  };

  const goNext = () => {
    stopSpeech();
    stopRecognition();

    setSkipped((prev) => {
      if (!attempted.has(currentIdx)) {
        const next = new Set(prev);
        next.add(currentIdx);
        return next;
      }
      return prev;
    });

    const isLast = currentIdx === Math.max(0, sentences.length - 1);

    if (isLast) {
      setGameOverOpen(true);
      setFeedback(Feedback.None);
      setIsCorrect(null);
      return;
    }

    setCurrentIdx((i) => i + 1);
    setFeedback(Feedback.None);
    setIsCorrect(null);
  };

  const handleConfigChange = () => setConfigModalOpen(true);

  const handleConfigConfirm = (config: {
    difficulty: DifficultyLevel;
    nikud: boolean;
    count: number;
  }) => {
    setDifficulty(config.difficulty);
    setNikud(config.nikud);
    setCount(config.count);
    setConfigModalOpen(false);
    setFeedback(Feedback.None);
    setIsConfigured(true);
    requestSentences(config.difficulty, config.nikud, config.count);
  };

  const handlePlayAgain = () => {
    setFeedback(Feedback.None);
    setIsCorrect(null);
    setGameOverOpen(false);
    setAttempted(new Set());
    setCorrectIdxs(new Set());
    setSkipped(new Set());
    requestSentences(difficulty, nikud, count);
  };

  const pageContext: PageContext = useMemo(
    () => ({
      pageName: "Speaking Practice",
      exerciseType: "speaking",
      currentExercise: currentIdx + 1,
      totalExercises: sentences.length,
      difficulty: difficulty.toString(),
      additionalContext: {
        isRecording,
        isPlaying,
        correctCount: correctIdxs.size,
        attemptedCount: attempted.size,
      },
    }),
    [
      currentIdx,
      sentences.length,
      difficulty,
      isRecording,
      isPlaying,
      correctIdxs.size,
      attempted.size,
    ],
  );

  if (!isConfigured) {
    return (
      <div className={classes.loader}>
        <GameSetupPanel
          configModalOpen={configModalOpen}
          setConfigModalOpen={setConfigModalOpen}
          handleConfigConfirm={handleConfigConfirm}
          getDifficultyLabel={getDifficultyLabel}
        />
      </div>
    );
  }

  if (!sentences.length && generateMutation.isPending) {
    return (
      <div className={classes.loader}>
        <CircularProgress />
      </div>
    );
  }

  return (
    <div className={classes.container} data-testid="speaking-practice-page">
      <div className={classes.nav} data-testid="speaking-nav">
        <button
          onClick={goPrev}
          data-testid="speaking-prev"
          disabled={sentences.length <= 1 || currentIdx === 0}
        >
          &laquo; {t("pages.speakingPractice.prev")}
        </button>
        <span data-testid="speaking-index">
          {sentences.length
            ? `${Math.min(currentIdx + 1, sentences.length)} / ${sentences.length}`
            : "—"}
        </span>
        <button
          onClick={goNext}
          data-testid="speaking-next"
          disabled={sentences.length === 0}
        >
          {t("pages.speakingPractice.next")} &raquo;
        </button>
      </div>

      <div className={classes.main} data-testid="speaking-main">
        <h2 className={classes.phrase} data-testid="speaking-phrase">
          {sentences.length
            ? sentences[currentIdx]
            : t("pages.speakingPractice.noData")}
        </h2>

        <p
          className={`${classes.feedback} ${isCorrect ? "correct" : "incorrect"}`}
          data-testid="speaking-feedback"
        >
          {generateMutation.isPending
            ? t("pages.wordOrderGame.loading")
            : error
              ? "TTS error."
              : feedback}
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
            !sentences.length ||
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
      {sentences.length > 0 && (
        <GameSettings
          gameConfig={{ difficulty, nikud, count }}
          currentSentenceIndex={currentIdx}
          sentenceCount={sentences.length}
          isHebrew={isHebrew}
          handleConfigChange={handleConfigChange}
          getDifficultyLabel={(lvl) => getDifficultyLabel(lvl, t)}
        />
      )}
      <GameConfigModal
        open={configModalOpen}
        onClose={() => setConfigModalOpen(false)}
        onConfirm={handleConfigConfirm}
        getDifficultyLevelLabel={(lvl) => getDifficultyLabel(lvl, t)}
        initialConfig={{ difficulty, nikud, count }}
      />
      <GameOverModal
        open={gameOverOpen}
        onPlayAgain={handlePlayAgain}
        onChangeSettings={() => setConfigModalOpen(true)}
        correctSentences={correctIdxs.size}
        totalSentences={sentences.length}
      />
      <ContextAwareChat pageContext={pageContext} hasSettings />
    </div>
  );
};
