import { createUseStyles } from "react-jss";

const useStyles = createUseStyles({
  sidebar: {
    flex: "0 0 30%",
    display: "flex",
    flexDirection: "column",
    background: "#fafafa",
  },
  fullScreen: {
    flex: 1,
    display: "flex",
    flexDirection: "column",
    background: "#f5f5f5",
  },
  messagesContainer: {
    flex: 1,
    overflowY: "auto",
    padding: 8,
    display: "flex",
    flexDirection: "column",
    gap: 4,
    background: "#f5f5f5",
  },
  toggleButton: {
    position: "absolute",
    left: 8,
    padding: "6px 12px",
    border: "none",
    background: "#007bff",
    color: "#fff",
    borderRadius: 4,
    cursor: "pointer",
  },
});

export default useStyles;
