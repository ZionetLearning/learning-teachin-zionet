import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
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
  header: {
    position: "fixed",
    top: 0,
    left: 0,
    right: 0,
    height: 60,
    backgroundColor: "#f5f5f5",
    display: "flex",
    alignItems: "center",
    justifyContent: "flex-start",
    padding: "0 16px",
    boxShadow: "0 2px 4px rgba(0, 0, 0, 0.1)",
    zIndex: 1000,
  },
  main: {
    marginTop: 60,
  },
  logoutButton: {
    extend: "button",
    marginLeft: "auto",
  },
});
