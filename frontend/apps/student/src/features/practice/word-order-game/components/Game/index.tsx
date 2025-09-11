import { useState, useEffect, useMemo } from "react";
import { useTranslation } from "react-i18next";
import { useStyles } from "./style";
import { Speaker } from "../Speaker";
import { useHebrewSentence, DifficultyLevel, SentenceMode } from "../../hooks";
import { useAvatarSpeech } from "@student/hooks";

export const Game = () => {
  const { t } = useTranslation();
  const classes = useStyles();
  const [chosen, setChosen] = useState<string[]>([]);
  const [shuffledSentence, setShuffledSentence] = useState<string[]>([]);

  // Define sentence configuration inside the component
  const sentenceConfig = {
    difficulty: 1 as DifficultyLevel, // 0=easy, 1=medium, 2=hard
    nikud: true, // Hebrew diacritics
    count: 3, // Number of sentences to fetch per request
    mode: "regular" as SentenceMode, // "regular" or "split"
  };

  // Enhanced hook with split sentence support
  const {
    sentence,
    words,
    loading,
    error,
    fetchSentence,
    initOnce,
    mode: currentMode,
  } = useHebrewSentence(sentenceConfig);

  const { speak, stop } = useAvatarSpeech({});

  useEffect(() => {
    console.log("Game - calling initOnce");
    initOnce();
  }, [initOnce]);

  useEffect(() => {
    const handleNewSentence = () => {
      if (!sentence) return;

      setChosen([]);

      if (currentMode === "split" && words.length > 0) {
        // Use pre-split words from API
        setShuffledSentence(shuffleDistinct(words));
      } else {
        // Split sentence manually (for regular mode)
        setShuffledSentence(
          shuffleDistinct(sentence.replace(/\./g, "").split(" ")),
        );
      }
    };
    handleNewSentence();
  }, [sentence, words, currentMode]);

  const handlePlay = () => {
    if (!sentence) return;
    speak(sentence);
  };

  const handleNextClick = async () => {
    stop();
    const s = await fetchSentence();
    if (!s) return;

    setChosen([]);

    if (currentMode === "split" && words.length > 0) {
      setShuffledSentence(shuffleDistinct(words));
    } else {
      setShuffledSentence(shuffleDistinct(s.replace(/\./g, "").split(" ")));
    }
  };

  const handleReset = () => {
    if (!sentence) return;

    setChosen([]);

    if (currentMode === "split" && words.length > 0) {
      setShuffledSentence(shuffleDistinct(words));
    } else {
      const clean = sentence.replace(/\./g, "");
      setShuffledSentence(shuffleDistinct(clean.split(" ")));
    }
  };

  const handleChooseWord = (word: string) => {
    setShuffledSentence((prev) => prev.filter((w) => w !== word));
    setChosen((prev) => [...prev, word]);
  };

  const handleUnchooseWord = (index: number, word: string) => {
    setChosen((prev) => prev.filter((_, i) => i !== index));
    setShuffledSentence((prev) => [word, ...prev]);
  };

  const isCorrect = useMemo(() => {
    if (currentMode === "split" && words.length > 0) {
      // For split mode, compare with the original word array
      return chosen.join(" ") === words.join(" ");
    } else {
      // For regular mode, compare with sentence (remove periods)
      return chosen.join(" ") === sentence.replace(/\./g, "");
    }
  }, [chosen, sentence, words, currentMode]);

  const handleCheck = () => {
    alert(isCorrect ? "Correct!" : "Try again!");
    return isCorrect;
  };

  const shuffleDistinct = (words: string[]) => {
    if (words.length < 2) return [...words];

    const original = words.join(" ");
    for (let i = 0; i < 5; i++) {
      const arr = [...words];
      for (let j = arr.length - 1; j > 0; j--) {
        const k = Math.floor(Math.random() * (j + 1));
        [arr[j], arr[k]] = [arr[k], arr[j]];
      }
      if (arr.join(" ") !== original) return arr;
    }
    return [...words].sort(() => Math.random() - 0.5);
  };

  return (
    <div className={classes.gameContainer}>
      <div className={classes.gameLogic}>
        <div className={classes.speakersContainer}>
          <Speaker onClick={() => handlePlay()} />
        </div>

        <div className={classes.answerArea} dir="rtl">
          <div className={classes.dashLine} />
          <div className={classes.dashLineWithWords} data-testid="wog-chosen">
            {chosen.map((w, i) => (
              <button
                key={`c-${w}-${i}`}
                className={classes.chosenWord}
                onClick={() => handleUnchooseWord(i, w)}
              >
                {w}
              </button>
            ))}
          </div>
        </div>

        <div className={classes.wordsBank} dir="rtl" data-testid="wog-bank">
          {loading && <div>{t("pages.wordOrderGame.loading")}</div>}
          {error && <div style={{ color: "red" }}>{error}</div>}
          {!loading &&
            !error &&
            shuffledSentence.map((w, i) => (
              <button
                key={`b-${w}-${i}`}
                className={classes.bankWord}
                onClick={() => handleChooseWord(w)}
              >
                {w}
              </button>
            ))}
        </div>
      </div>

      <div className={classes.sideButtons}>
        <button data-testid="wog-reset" onClick={handleReset}>
          {t("pages.wordOrderGame.reset")}
        </button>
        <button data-testid="wog-check" onClick={handleCheck}>
          {t("pages.wordOrderGame.check")}
        </button>
        <button
          data-testid="wog-next"
          disabled={loading}
          onClick={handleNextClick}
        >
          {t("pages.wordOrderGame.next")}
        </button>
      </div>
    </div>
  );
};
