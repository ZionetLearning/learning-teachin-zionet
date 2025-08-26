/// <reference types="vite/client" />
import { useRef, useState } from "react";
import * as sdk from "microsoft-cognitiveservices-speech-sdk";
import { useTranslation } from "react-i18next";
import { comparePhrases, phrases, phrasesWithNikud } from "./utils";

import { useStyles } from "./style";
import { useAvatarSpeech } from "@/hooks";

const Feedback = {
  Perfect: "Perfect!",
  TryAgain: "Try again, that was not accurate.",
  RecognitionError: "Speech recognition error.",
  None: "",
} as const;

type FeedbackType = (typeof Feedback)[keyof typeof Feedback];

export const SpeakingPractice = () => {
  const classes = useStyles();
  const { t } = useTranslation();
  const [showNikud, setShowNikud] = useState(false);
  const [currentIdx, setCurrentIdx] = useState(0);
  const [feedback, setFeedback] = useState<FeedbackType>(Feedback.None);
  const [isCorrect, setIsCorrect] = useState<boolean | null>(null);
  const [isRecording, setIsRecording] = useState(false);

  const recognizerRef = useRef<sdk.SpeechRecognizer | null>(null); // for speech recognition from microphone
  const audioConfigRef = useRef<sdk.AudioConfig | null>(null); // for audio input/output from microphone/speaker

  const speechConfig = sdk.SpeechConfig.fromSubscription(
    import.meta.env.VITE_AZURE_SPEECH_KEY!,
    import.meta.env.VITE_AZURE_REGION!,
  );

  speechConfig.speechSynthesisVoiceName = "he-IL-HilaNeural";
  speechConfig.speechRecognitionLanguage = "he-IL";

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
    if (isRecording) {
      stopRecognition();
      return;
    }
    if (isPlaying) {
      stopSpeech();
    }
    setFeedback(Feedback.None);

    const audioConfig = sdk.AudioConfig.fromDefaultMicrophoneInput();
    const recognizer = new sdk.SpeechRecognizer(speechConfig, audioConfig);
    audioConfigRef.current = audioConfig;
    recognizerRef.current = recognizer;
    setIsRecording(true);

    recognizer.recognizeOnceAsync(
      (result) => {
        const userText = result.text ?? "";
        const correct = comparePhrases(userText, phrases[currentIdx]);
        setIsCorrect(correct);
        setFeedback(correct ? Feedback.Perfect : Feedback.TryAgain);
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
    speak(phrases[currentIdx]);
  };

  const goPrev = () => {
    stopSpeech();
    stopRecognition();
    setCurrentIdx((i) => (i === 0 ? phrases.length - 1 : i - 1));
    setFeedback(Feedback.None);
    setIsCorrect(null);
  };

  const goNext = () => {
    stopSpeech();
    stopRecognition();
    setCurrentIdx((i) => (i + 1) % phrases.length);
    setFeedback(Feedback.None);
    setIsCorrect(null);
  };

  return (
    <div className={classes.container} data-testid="speaking-practice-page">
      <div className={classes.nav} data-testid="speaking-nav">
        <button onClick={goPrev} data-testid="speaking-prev">
          &laquo; {t("pages.speakingPractice.prev")}
        </button>
        <span data-testid="speaking-index">
          {currentIdx + 1} / {phrases.length}
        </span>
        <button onClick={goNext} data-testid="speaking-next">
          {t("pages.speakingPractice.next")} &raquo;
        </button>
      </div>

      <div className={classes.main} data-testid="speaking-main">
        <h2 className={classes.phrase} data-testid="speaking-phrase">
          {showNikud ? phrasesWithNikud[currentIdx] : phrases[currentIdx]}
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
        <button onClick={handleRecord} data-testid="speaking-record">
          {isRecording
            ? t("pages.speakingPractice.stop")
            : t("pages.speakingPractice.record")}
        </button>
        <button
          onClick={() => setShowNikud(!showNikud)}
          data-testid="speaking-nikud-toggle"
        >
          {showNikud
            ? t("pages.speakingPractice.hideNikud")
            : t("pages.speakingPractice.showNikud")}
        </button>
      </div>
    </div>
  );
};
