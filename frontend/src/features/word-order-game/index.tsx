import { useStyles } from "./style";
import { Header, Description, Game } from "./components";
export const WordOrderGame = () => {
  const classes = useStyles();
  return (
    <div className={classes.container}>
      <Header />
      <Description />
      <Game />
    </div>
  );
};
