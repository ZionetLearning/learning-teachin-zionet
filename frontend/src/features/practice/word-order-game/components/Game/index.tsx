import { useState, useEffect, useMemo } from "react";
import { useStyles } from "./style";
import { Speaker } from "../Speaker";
import { playSentenceCached, clearSpeechCache } from "../../services";
import { useHebrewSentence } from "../../hooks";

export const Game = () => {
  const classes = useStyles();
  const [chosen, setChosen] = useState<string[]>([]);
  const [shuffledSentence, setShuffledSentence] = useState<string[]>([]);
  const { sentence, loading, error, fetchSentence, initOnce } =
    useHebrewSentence();

  useEffect(() => {
    initOnce();
  }, [initOnce]);

  useEffect(() => {
    const handleNewSentence = () => {
      if (!sentence) return;
      setChosen([]);
      setShuffledSentence(
        shuffleDistinct(sentence.replace(/\./g, "").split(" ")),
      );
    };
    handleNewSentence();
  }, [sentence]);

  const handlePlay = (mode?: "normal" | "slow") => {
    if (!sentence) return;
    // play the sentence
    playSentenceCached(sentence, undefined, mode === "slow" ? -40 : 0);
  };

  const handleNextClick = async () => {
    clearSpeechCache();
    const s = await fetchSentence();
    if (!s) return;
    setChosen([]);
    setShuffledSentence(shuffleDistinct(s.replace(/\./g, "").split(" ")));
  };

  const handleReset = () => {
    if (!sentence) return;
    const clean = sentence.replace(/\./g, "");
    setChosen([]);
    setShuffledSentence(shuffleDistinct(clean.split(" ")));
  };

  const handleChooseWord = (word: string) => {
    setShuffledSentence((prev) => prev.filter((w) => w !== word));
    setChosen((prev) => [...prev, word]);
  };

  const handleUnchooseWord = (index: number, word: string) => {
    setChosen((prev) => prev.filter((_, i) => i !== index));
    setShuffledSentence((prev) => [word, ...prev]);
  };

  const isCorrect = useMemo(
    () => chosen.join(" ") === sentence.replace(/\./g, ""),
    [chosen, sentence],
  );

  const handleCheck = () => {
    alert(isCorrect ? "Correct!" : "Try again!");
    return isCorrect;
  };

  const shuffleDistinct = (words: string[]) => {
    if (words.length < 2) return [...words];

    // try a few times to get an order different from the original
    const original = words.join(" ");
    for (let i = 0; i < 5; i++) {
      const arr = [...words];
      for (let j = arr.length - 1; j > 0; j--) {
        const k = Math.floor(Math.random() * (j + 1));
        [arr[j], arr[k]] = [arr[k], arr[j]];
      }
      if (arr.join(" ") !== original) return arr;
    }
    // if we couldn’t get a different order, just return the shuffled result
    return [...words].sort(() => Math.random() - 0.5);
  };

  return (
    <div className={classes.gameContainer}>
      <div className={classes.gameLogic}>
        <div className={classes.speakersContainer}>
          <Speaker onClick={() => handlePlay()} />
          <Speaker mode="slow" onClick={() => handlePlay("slow")} />
        </div>

        <div className={classes.answerArea} dir="rtl">
          <div className={classes.dashLine} />
          <div className={classes.dashLineWithWords}>
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

        <div className={classes.wordsBank} dir="rtl">
          {loading && <div>Loading…</div>}
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
        <button onClick={handleReset}>Reset</button>
        <button onClick={handleCheck}>Check</button>
        <button onClick={handleNextClick}>Next</button>
      </div>
    </div>
  );
};
