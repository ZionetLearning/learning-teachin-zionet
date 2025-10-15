import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  paper: {
    p: 32,
    textAlign: "center",
  },
  container: {
    p: 16,
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    cursor: "pointer",
    "&:hover": { bgcolor: "action.hover" },
  },
  innerContainer: {
    dispplay: "flex",
    alignItems: "center",
    gap: 16
  },
  tableContainer: {
    borderTop: 8,
    borderColor: "divider",
    //maxHeight: "300px",
  }
});
