import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  inputContainer: {
    display: "flex",
    position: "absolute",
    bottom: 80,
    left: "50%",
    transform: "translateX(-50%)",
    background: "rgba(255,255,255,0.9)",
    padding: "8px",
    borderRadius: "4px",
    gap: "8px",
  },
  input: {
    width: 200,
    padding: [8, 12],
    fontSize: 14,
    border: "1px solid #ccc",
    borderRadius: 4,
    outline: "none",
  },
  inputButton: {
    padding: [8, 12],
    fontSize: 14,
    background: "#646cff",
    color: "white",
    border: "none",
    borderRadius: 4,
    cursor: "pointer",
    transition: "background 150ms ease",
    "&:hover": {
      background: "#535bf2",
    },
  },
});
