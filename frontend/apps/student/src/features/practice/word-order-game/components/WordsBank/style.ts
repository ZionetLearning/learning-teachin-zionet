import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  wordsBank: {
    display: "flex",
    flexDirection: "row",
    gap: "5px",
    alignSelf: "center",
  },
  bankWord: {
    padding: "8px 16px",
    borderRadius: 16,
    border: "1px solid rgba(180, 200, 210, 0.6)",
    background: "linear-gradient(145deg, #ffffff, #edf3f5)",
    color: "#1f3a46",
    fontSize: 18,
    fontWeight: 500,
    boxShadow: "0 2px 5px rgba(0, 0, 0, 0.06)",
    cursor: "pointer",
    transition: "all 0.2s ease",
    "&:hover": {
      background: "linear-gradient(145deg, #eaf3f6, #dce9ec)",
      transform: "translateY(-2px)",
      boxShadow: "0 4px 8px rgba(0, 0, 0, 0.12)",
    },
    "&:active": {
      transform: "scale(0.97)",
      boxShadow: "0 2px 4px rgba(0, 0, 0, 0.15)",
    },
  },
});
