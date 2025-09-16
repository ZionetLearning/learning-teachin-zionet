import { useStyles } from "./style";
interface ChosenWordsAreaProps {
  chosenWords: string[];
  handleUnchooseWord: (index: number, word: string) => void;
}

export const ChosenWordsArea = ({
  chosenWords,
  handleUnchooseWord,
}: ChosenWordsAreaProps) => {
  const classes = useStyles();

  return (
    <div className={classes.chosenWordsArea} dir="rtl">
      <div className={classes.dashLine} />
      <div className={classes.dashLineWithWords} data-testid="wog-chosen">
        {chosenWords.map((w, i) => (
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
  );
};
