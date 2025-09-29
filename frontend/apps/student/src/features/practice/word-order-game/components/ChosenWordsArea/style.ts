import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  chosenWordsArea: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    marginTop: 8,
  },
  dashLine: {
    width: 520,
    height: 0,
    borderBottom: "2px solid #9bb9c8",
  },
  dashLineWithWords: {
    position: "relative",
    width: 520,
    minHeight: 64,
    display: "flex",
    flexWrap: "wrap",
    gap: 8,
    justifyContent: "center",
    alignItems: "center",
    "&::after": {
      content: '""',
      position: "absolute",
      left: 0,
      right: 0,
      bottom: 6,
      borderBottom: "2px solid #9bb9c8",
      pointerEvents: "none",
    },
  },
  chosenWord: {
    padding: "8px 14px",
    borderRadius: 12,
    border: "1px solid #cfe7d8",
    background: "#e8f8ef",
    color: "black",
    boxShadow: "0 1px 2px rgba(0,0,0,0.06)",
    fontSize: 18,
    cursor: "pointer",
    marginBottom: "8px",
  },
});
