// import { useState, useEffect, useCallback } from "react";
// import { useLocation } from "react-router-dom";
// import { useTranslation } from "react-i18next";
// import { useAvatarSpeech, useHebrewSentence } from "@student/hooks";
// import { ChosenWordsArea, WordsBank, ActionButtons, Speaker } from "../";
// import {
//   GameConfig,
//   GameConfigModal,
//   GameOverModal,
//   GameSettings,
//   GameSetupPanel,
// } from "@ui-components";
// import { getDifficultyLabel } from "@student/features";
// import { useAuth } from "@app-providers";
// import { useSubmitGameAttempt } from "@student/api";
// import { useStyles } from "./style";
// import { toast } from "react-toastify";
// import { DifficultyLevel } from "@student/types";
// //import { DifficultyLevel } from "@student/features/practice/typing-practice/types";

// interface RetryState {
//   retryMode: boolean;
//   nikud: boolean;
//   difficulty: DifficultyLevel;
//   gameType: string;
//   sentence?: string;
// }

// export const Game = () => {
//   const { user } = useAuth();
//   const { t, i18n } = useTranslation();
//   const location = useLocation();
//   const classes = useStyles();

//   const studentId = user?.userId ?? "";
//   const { mutateAsync: submitAttempt } = useSubmitGameAttempt();

//   const [chosen, setChosen] = useState<string[]>([]);
//   const [shuffledSentence, setShuffledSentence] = useState<string[]>([]);
//   const [configModalOpen, setConfigModalOpen] = useState(false);
//   const [gameOverModalOpen, setGameOverModalOpen] = useState(false);
//   const [gameConfig, setGameConfig] = useState<GameConfig | null>(null);
//   const [gameStarted, setGameStarted] = useState(false);
//   const [correctSentencesCount, setCorrectSentencesCount] = useState<number>(0);
//   const isHebrew = i18n.language === "he";

//   const {
//     attemptId,
//     sentence,
//     words,
//     loading,
//     error,
//     fetchSentence,
//     initOnce,
//     resetGame,
//     sentenceCount,
//     currentSentenceIndex,
//   } = useHebrewSentence(gameConfig || undefined);

//   const { speak, stop, isLoading: speechLoading } = useAvatarSpeech({});

// useEffect(() => {
//   const state = location.state as RetryState | undefined;
//   if (state?.retryMode && state?.difficulty !== undefined) {
//     const retryConfig: GameConfig = {
//       difficulty: state.difficulty,
//       count: 1,
//       nikud: state.nikud,
//     };
//     setGameConfig(retryConfig);
//     setGameStarted(true);

//     window.history.replaceState({}, document.title);
//   }
// }, [location.state]);

//   // Show config modal on first load
//   useEffect(() => {
//     if (!gameStarted && !gameConfig) {
//       setConfigModalOpen(true);
//     }
//   }, [gameStarted, gameConfig]);

//   // Initialize game when config is set
//   useEffect(() => {
//     if (gameConfig && !gameStarted) {
//       initOnce();
//       setGameStarted(true);
//     }
//   }, [gameConfig, gameStarted, initOnce]);

//   // Handle new sentences
//   useEffect(() => {
//     const handleNewSentence = () => {
//       if (!sentence || words.length === 0) {
//         return;
//       }
//       setChosen([]);
//       setShuffledSentence(shuffleDistinct(words));
//     };
//     handleNewSentence();
//   }, [sentence, words]);

//   const handleConfigConfirm = (config: GameConfig) => {
//     setGameConfig(config);
//     setConfigModalOpen(false);
//     // Reset game state when config changes
//     setChosen([]);
//     setShuffledSentence([]);
//     setCorrectSentencesCount(0);
//     setGameStarted(false);
//     resetGame();
//   };

//   const handleConfigChange = () => {
//     setConfigModalOpen(true);
//   };

//   const handlePlay = () => {
//     if (!sentence || sentence.trim() === "" || speechLoading) {
//       return;
//     }
//     speak(sentence);
//   };

//   const handleNextClick = useCallback(async () => {
//     stop();
//     const res = await submitAttempt({
//       attemptId,
//       studentId,
//       givenAnswer: chosen,
//     });

//     const isServerCorrect = res.status === "Success";
//     if (isServerCorrect) {
//       setCorrectSentencesCount(correctSentencesCount + 1);
//     }
//     const result = await fetchSentence();
//     // Check if we've completed all sentences
//     if (!result || !result.sentence) {
//       setGameOverModalOpen(true);
//       return;
//     }
//     setChosen([]);
//     if (result.words && result.words.length > 0) {
//       setShuffledSentence(shuffleDistinct(result.words));
//     }
//   }, [
//     stop,
//     submitAttempt,
//     attemptId,
//     studentId,
//     chosen,
//     fetchSentence,
//     correctSentencesCount,
//   ]);

//   const handleGameOverPlayAgain = () => {
//     setGameOverModalOpen(false);
//     resetGame();
//     // Reset component state
//     setChosen([]);
//     setShuffledSentence([]);
//     setGameStarted(false);
//     setCorrectSentencesCount(0);
//     // Restart the game with same config
//     setTimeout(() => {
//       setGameStarted(true);
//       initOnce();
//     }, 100);
//   };

//   const handleGameOverChangeSettings = () => {
//     setGameOverModalOpen(false);
//     setConfigModalOpen(true);
//   };

//   const handleReset = () => {
//     if (!sentence) return;
//     setChosen([]);

//     if (words.length > 0) {
//       setShuffledSentence(shuffleDistinct(words));
//     }
//   };

//   const handleChooseWord = (word: string) => {
//     setShuffledSentence((prev) => {
//       const index = prev.indexOf(word);
//       if (index > -1) {
//         const newArray = [...prev];
//         newArray.splice(index, 1); // Remove only the first occurrence
//         return newArray;
//       }
//       return prev;
//     });
//     setChosen((prev) => [...prev, word]);
//   };

//   const handleUnchooseWord = (index: number, word: string) => {
//     setChosen((prev) => prev.filter((_, i) => i !== index));
//     setShuffledSentence((prev) => [word, ...prev]);
//   };

//   const handleCheck = useCallback(async () => {
//     const res = await submitAttempt({
//       attemptId,
//       studentId,
//       givenAnswer: chosen,
//     });

//     const isServerCorrect = res.status === "Success";

//     if (isServerCorrect) {
//       toast.success(t("pages.wordOrderGame.correct"));
//     } else {
//       toast.error(t("pages.wordOrderGame.incorrect"));
//     }

//     return isServerCorrect;
//   }, [submitAttempt, attemptId, studentId, chosen, t]);

//   const shuffleDistinct = (words: string[]) => {
//     if (words.length < 2) return [...words];

//     const shuffled = [...words];

//     for (let i = shuffled.length - 1; i > 0; i--) {
//       const j = Math.floor(Math.random() * (i + 1));
//       [shuffled[i], shuffled[j]] = [shuffled[j], shuffled[i]];
//     }

//     if (shuffled.join(" ") === words.join(" ") && words.length > 1) {
//       [shuffled[0], shuffled[1]] = [shuffled[1], shuffled[0]];
//     }

//     return shuffled;
//   };

//   // Show welcome screen if game hasn't started yet
//   if (!gameStarted || !gameConfig) {
//     return (
//       <GameSetupPanel
//         configModalOpen={configModalOpen}
//         setConfigModalOpen={setConfigModalOpen}
//         handleConfigConfirm={handleConfigConfirm}
//         getDifficultyLabel={getDifficultyLabel}
//       />
//     );
//   }

//   return (
//     <>
//       <div className={classes.gameContainer}>
//         <GameSettings
//           gameConfig={gameConfig}
//           currentSentenceIndex={currentSentenceIndex}
//           sentenceCount={sentenceCount}
//           isHebrew={isHebrew}
//           handleConfigChange={handleConfigChange}
//           getDifficultyLabel={getDifficultyLabel}
//         />

//         <div className={classes.gameLogic}>
//           <div className={classes.speakersContainer}>
//             <Speaker
//               onClick={() => handlePlay()}
//               disabled={!sentence || sentence.trim() === "" || speechLoading}
//             />
//             <ActionButtons
//               loading={loading}
//               handleNextClick={handleNextClick}
//               handleCheck={handleCheck}
//               handleReset={handleReset}
//             />
//           </div>

//           <ChosenWordsArea
//             chosenWords={chosen}
//             handleUnchooseWord={handleUnchooseWord}
//           />

//           <WordsBank
//             loading={loading}
//             error={error}
//             shuffledSentence={shuffledSentence}
//             handleChooseWord={handleChooseWord}
//           />
//         </div>
//       </div>
//       {/* Configuration Modal */}
//       <GameConfigModal
//         open={configModalOpen}
//         onClose={() => setConfigModalOpen(false)}
//         onConfirm={handleConfigConfirm}
//         getDifficultyLevelLabel={getDifficultyLabel}
//         initialConfig={gameConfig || undefined}
//       />

//       {/* Game Over Modal */}
//       <GameOverModal
//         open={gameOverModalOpen}
//         onPlayAgain={handleGameOverPlayAgain}
//         onChangeSettings={handleGameOverChangeSettings}
//         correctSentences={correctSentencesCount}
//         totalSentences={sentenceCount}
//       />
//     </>
//   );
// };

import { useState, useEffect, useCallback, useRef } from "react";
import { useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAvatarSpeech, useHebrewSentence } from "@student/hooks";
import { ChosenWordsArea, WordsBank, ActionButtons, Speaker } from "../";
import {
  GameConfig,
  GameConfigModal,
  GameOverModal,
  GameSettings,
  GameSetupPanel,
} from "@ui-components";
import { getDifficultyLabel } from "@student/features";
import { useAuth } from "@app-providers";
import { useSubmitGameAttempt } from "@student/api";
import { useStyles } from "./style";
import { toast } from "react-toastify";
import { DifficultyLevel } from "@student/types";

interface RetryState {
  retryMode: boolean;
  nikud: boolean;
  difficulty: DifficultyLevel;
  gameType: string;
  sentence?: string;
  attemptId?: string;
}

export const Game = () => {
  const { user } = useAuth();
  const { t, i18n } = useTranslation();
  const location = useLocation();
  const classes = useStyles();

  const studentId = user?.userId ?? "";
  const { mutateAsync: submitAttempt } = useSubmitGameAttempt();

  const [chosen, setChosen] = useState<string[]>([]);
  const [shuffledSentence, setShuffledSentence] = useState<string[]>([]);
  const [configModalOpen, setConfigModalOpen] = useState(false);
  const [gameOverModalOpen, setGameOverModalOpen] = useState(false);
  const [gameConfig, setGameConfig] = useState<GameConfig | null>(null);
  const [gameStarted, setGameStarted] = useState(false);
  const [correctSentencesCount, setCorrectSentencesCount] = useState<number>(0);
  const [retrySentence, setRetrySentence] = useState<string | null>(null);
  const [hasUsedRetrySentence, setHasUsedRetrySentence] = useState(false);
  const isRetryMode = useRef(false); // Track if we're in retry mode
  const [retryAttemptId, setRetryAttemptId] = useState<string | null>(null);
  const isHebrew = i18n.language === "he";

  const {
    attemptId,
    sentence,
    words,
    loading,
    error,
    fetchSentence,
    initOnce,
    resetGame,
    sentenceCount,
    currentSentenceIndex,
  } = useHebrewSentence(gameConfig || undefined);

  const { speak, stop, isLoading: speechLoading } = useAvatarSpeech({});

  // Check for retry mode on mount
  useEffect(() => {
    const state = location.state as RetryState | undefined;
    if (
      state?.retryMode &&
      state?.difficulty !== undefined &&
      state?.sentence
    ) {
      isRetryMode.current = true; // Mark as retry mode

      const retryConfig: GameConfig = {
        difficulty: state.difficulty,
        count: 1,
        nikud: state.nikud,
      };
      setRetryAttemptId(state.attemptId || null);
      setRetrySentence(state.sentence);
      setGameConfig(retryConfig);
      setGameStarted(true); // Set to true immediately to skip config modal

      // Clear location state
      window.history.replaceState({}, document.title);
    }
  }, []);

  // Show config modal on first load (but NOT in retry mode)
  useEffect(() => {
    if (!gameStarted && !gameConfig && !isRetryMode.current) {
      setConfigModalOpen(true);
    }
  }, [gameStarted, gameConfig]);

  // Initialize game when config is set (but SKIP in retry mode)
  useEffect(() => {
    if (gameConfig && !gameStarted && !isRetryMode.current) {
      initOnce();
      setGameStarted(true);
    }
  }, [gameConfig, gameStarted, initOnce]);

  // Handle retry sentence ONCE
  useEffect(() => {
    if (retrySentence && !hasUsedRetrySentence) {
      const retryWords = retrySentence.split(" ");
      setShuffledSentence(shuffleDistinct(retryWords));
      setHasUsedRetrySentence(true);
    }
  }, [retrySentence, hasUsedRetrySentence]);

  // Handle new sentences from the hook (only when NOT in retry mode)
  useEffect(() => {
    if (!sentence || words.length === 0 || isRetryMode.current) {
      return;
    }
    setChosen([]);
    setShuffledSentence(shuffleDistinct(words));
  }, [sentence, words]);

  const handleConfigConfirm = (config: GameConfig) => {
    isRetryMode.current = false; // Reset retry mode flag
    setGameConfig(config);
    setConfigModalOpen(false);
    setChosen([]);
    setShuffledSentence([]);
    setCorrectSentencesCount(0);
    setGameStarted(false);
    setRetrySentence(null);
    setHasUsedRetrySentence(false);
    resetGame();
  };

  const handleConfigChange = () => {
    setConfigModalOpen(true);
  };

  const handlePlay = () => {
    const textToSpeak = retrySentence || sentence;
    if (!textToSpeak || textToSpeak.trim() === "" || speechLoading) {
      return;
    }
    speak(textToSpeak);
  };

  const handleNextClick = useCallback(async () => {
    stop();
    const res = await submitAttempt({
      attemptId: retryAttemptId || attemptId,
      studentId,
      givenAnswer: chosen,
    });

    const isServerCorrect = res.status === "Success";
    if (isServerCorrect) {
      setCorrectSentencesCount(correctSentencesCount + 1);
    }

    // If this was a retry, we're done after one attempt
    if (isRetryMode.current) {
      if (isServerCorrect) {
        setCorrectSentencesCount(1);
      }
      // Wait for state to update, then show modal
      setTimeout(() => {
        setGameOverModalOpen(true);
      }, 100);
      return;
      return;
    }

    const result = await fetchSentence();
    if (!result || !result.sentence) {
      setGameOverModalOpen(true);
      return;
    }
    setChosen([]);
    if (result.words && result.words.length > 0) {
      setShuffledSentence(shuffleDistinct(result.words));
    }
  }, [
    stop,
    submitAttempt,
    attemptId,
    studentId,
    retryAttemptId,
    chosen,
    fetchSentence,
    correctSentencesCount,
  ]);

  const handleGameOverPlayAgain = () => {
    isRetryMode.current = false; // Reset retry mode
    setGameOverModalOpen(false);
    resetGame();
    setChosen([]);
    setShuffledSentence([]);
    setGameStarted(false);
    setCorrectSentencesCount(0);
    setRetrySentence(null);
    setHasUsedRetrySentence(false);
    setTimeout(() => {
      setGameStarted(true);
      initOnce();
    }, 100);
  };

  const handleGameOverChangeSettings = () => {
    isRetryMode.current = false; // Reset retry mode
    setGameOverModalOpen(false);
    setConfigModalOpen(true);
  };

  const handleReset = () => {
    const currentSentence = retrySentence || sentence;
    if (!currentSentence) return;
    setChosen([]);

    const currentWords = retrySentence ? retrySentence.split(" ") : words;
    if (currentWords.length > 0) {
      setShuffledSentence(shuffleDistinct(currentWords));
    }
  };

  const handleChooseWord = (word: string) => {
    setShuffledSentence((prev) => {
      const index = prev.indexOf(word);
      if (index > -1) {
        const newArray = [...prev];
        newArray.splice(index, 1);
        return newArray;
      }
      return prev;
    });
    setChosen((prev) => [...prev, word]);
  };

  const handleUnchooseWord = (index: number, word: string) => {
    setChosen((prev) => prev.filter((_, i) => i !== index));
    setShuffledSentence((prev) => [word, ...prev]);
  };

  const handleCheck = useCallback(async () => {
    const res = await submitAttempt({
      attemptId: retryAttemptId || attemptId,
      studentId,
      givenAnswer: chosen,
    });

    const isServerCorrect = res.status === "Success";

    if (isServerCorrect) {
      toast.success(t("pages.wordOrderGame.correct"));
    } else {
      toast.error(t("pages.wordOrderGame.incorrect"));
    }

    return isServerCorrect;
  }, [submitAttempt, attemptId, studentId, retryAttemptId, chosen, t]);

  const shuffleDistinct = (words: string[]) => {
    if (words.length < 2) return [...words];

    const shuffled = [...words];

    for (let i = shuffled.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [shuffled[i], shuffled[j]] = [shuffled[j], shuffled[i]];
    }

    if (shuffled.join(" ") === words.join(" ") && words.length > 1) {
      [shuffled[0], shuffled[1]] = [shuffled[1], shuffled[0]];
    }

    return shuffled;
  };

  if (!gameStarted || !gameConfig) {
    return (
      <GameSetupPanel
        configModalOpen={configModalOpen}
        setConfigModalOpen={setConfigModalOpen}
        handleConfigConfirm={handleConfigConfirm}
        getDifficultyLabel={getDifficultyLabel}
      />
    );
  }

  return (
    <>
      <div className={classes.gameContainer}>
        <GameSettings
          gameConfig={gameConfig}
          currentSentenceIndex={currentSentenceIndex}
          sentenceCount={sentenceCount}
          isHebrew={isHebrew}
          handleConfigChange={handleConfigChange}
          getDifficultyLabel={getDifficultyLabel}
        />

        <div className={classes.gameLogic}>
          <div className={classes.speakersContainer}>
            <Speaker
              onClick={() => handlePlay()}
              disabled={
                (!sentence && !retrySentence) ||
                (sentence?.trim() === "" && !retrySentence) ||
                speechLoading
              }
            />
            <ActionButtons
              loading={loading}
              handleNextClick={handleNextClick}
              handleCheck={handleCheck}
              handleReset={handleReset}
            />
          </div>

          <ChosenWordsArea
            chosenWords={chosen}
            handleUnchooseWord={handleUnchooseWord}
          />

          <WordsBank
            loading={loading}
            error={error}
            shuffledSentence={shuffledSentence}
            handleChooseWord={handleChooseWord}
          />
        </div>
      </div>

      <GameConfigModal
        open={configModalOpen}
        onClose={() => setConfigModalOpen(false)}
        onConfirm={handleConfigConfirm}
        getDifficultyLevelLabel={getDifficultyLabel}
        initialConfig={gameConfig || undefined}
      />

      <GameOverModal
        open={gameOverModalOpen}
        onPlayAgain={handleGameOverPlayAgain}
        onChangeSettings={handleGameOverChangeSettings}
        correctSentences={correctSentencesCount}
        totalSentences={ isRetryMode.current == true ? 1 : sentenceCount}
      />
    </>
  );
};
