import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  container: {
    marginTop: 16,
    padding: 16,
    backgroundColor: "#fff3e0",
    border: "1px solid #ffcc80",
    borderRadius: 8,
    textAlign: "center",
  },
  label: {
    marginBottom: 8,
  },
  wrongAnswer: {
    fontWeight: 500,
    color: "#e65100",
    fontStyle: "italic",
  },
});
