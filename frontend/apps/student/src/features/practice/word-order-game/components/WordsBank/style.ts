import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  wordsBank: {
    display: "flex",
    flexDirection: "row",
    gap: "5px",
    alignSelf: "center",
  },
  bankWord: {
    padding: "8px 14px",
    borderRadius: 12,
    border: "1px solid #d8e2e8",
    background: "#fefefe",
    color: "black",
    boxShadow: "0 1px 2px rgba(0,0,0,0.06)",
    fontSize: 18,
    cursor: "pointer",
  },
});
