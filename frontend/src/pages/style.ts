import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  homePageWrapper: {
    display: "flex",
    flexDirection: "column",
    gap: "16px",
    maxWidth: "1200px",
    margin: "0 auto",
  },
  columnsWrapper: {
    display: "flex",
    gap: "5%",
    flexWrap: "wrap",
    justifyContent: "center",
  },
  column: {
    display: "flex",
    flexDirection: "column",
    gap: "20px",
    minWidth: 200,
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
    marginTop: "5%",
  },
  fullScreenAvatarDaPage: {
    position: "absolute",
    top: 0,
    left: 0,
    width: "100%",
    height: "100%",
    margin: 0,
    padding: 0,
    overflow: "hidden",
  },
});
