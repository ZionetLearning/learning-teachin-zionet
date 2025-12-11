import { createUseStyles } from "react-jss";

export const useStyles = () => {
  return createUseStyles({
    container: {
      display: "flex",
      justifyContent: "center",
      alignItems: "center",
      minHeight: 300,
      textAlign: "center",
    },
  })();
};
