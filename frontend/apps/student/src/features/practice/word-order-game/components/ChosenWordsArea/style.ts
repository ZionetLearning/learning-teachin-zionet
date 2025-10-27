import { createUseStyles } from "react-jss";

const PURPLE = "#7c4dff";

export const useStyles = createUseStyles({
  chosenWordsArea: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    marginTop: 12,
    position: "relative",
  },
  dashLine: {
    width: 800,
    height: 0,
    borderBottom: "2px solid rgba(150, 180, 190, 0.5)",
  },
  dashLineWithWords: {
    position: "relative",
    width: 800,
    minHeight: 70,
    display: "flex",
    flexWrap: "wrap",
    gap: 10,
    justifyContent: "center",
    alignItems: "center",
    padding: "8px 0",
    transition: "all 0.2s ease-in-out",
    "&::after": {
      content: '""',
      position: "absolute",
      left: 0,
      right: 0,
      bottom: 6,
      borderBottom: "2px solid rgba(150, 180, 190, 0.4)",
      pointerEvents: "none",
    },
  },
  chosenWord: {
    padding: "8px 16px",
    borderRadius: 16,
    border: `1px solid ${PURPLE}`,
    background: "linear-gradient(145deg, #f6ecfa, #e8d6f3)",
    color: "#4A0E43",
    fontWeight: 600,
    fontSize: 18,
    cursor: "pointer",
    boxShadow: "0 2px 5px rgba(111, 29, 100, 0.15)",
    transition: "all 0.2s ease",
    marginBottom: "8px",
    "&:hover": {
      background: "linear-gradient(145deg, #eedcf6, #e0c8f1)",
      transform: "translateY(-2px)",
      boxShadow: "0 3px 8px rgba(111, 29, 100, 0.25)",
    },
    "&:active": {
      transform: "scale(0.97)",
      background: "linear-gradient(145deg, #e4c5f0, #d8b5ea)",
      boxShadow: "0 1px 3px rgba(111, 29, 100, 0.3)",
    },
  },
});
