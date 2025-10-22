import { useStyles } from "./style";
import { Header, Description, Game } from "./components";

export const WordOrderGame = () => {
  const classes = useStyles();

  return (
    <div className={classes.container}>
      <div className={classes.headerSection}>
        <Header />
        <Description />
      </div>
      <Game />
    </div>
  );
};
