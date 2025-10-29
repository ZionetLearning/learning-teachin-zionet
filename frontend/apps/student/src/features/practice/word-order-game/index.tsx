import { useStyles } from "./style";
import { Header, Description, Game } from "./components";

interface RetryData {
  correctAnswer: string[];
  attemptId: string;
  wrongAnswers: string[][];
  difficulty: number;
}

interface WordOrderGameProps {
  retryData?: RetryData;
}

export const WordOrderGame = ({ retryData }: WordOrderGameProps) => {
  const classes = useStyles();

  return (
    <div className={classes.container}>
      <div className={classes.headerSection}>
        <Header />
        <Description />
      </div>
      <Game retryData={retryData} />
    </div>
  );
};
