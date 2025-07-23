import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  homePageWrapper: {
    display: "flex",
    flexDirection: "column",
    gap: "16px",
    maxWidth: "600px",
    margin: "0 auto",
    marginTop: "50px",
  },
  button: {
    background: "#59BEDFFF",
    color: "white",
    padding: [10, 20],
    border: "none",
    borderRadius: 15,
    cursor: "pointer",
    "&:hover": {
      background: "#1B81A6FF",
    },
  },
  chatDaPageWrapper: {
    display: "flex",
    height: "calc(100vh - 4rem)",
  },
  chatDaChatWrapper: {
    flex: 1,
    display: "flex",
  },
});
