import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  headerWrapper: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    justifyContent: "center",
  },
  tableWrapper: {
    width: "50%",
    display: "flex",
    alignItems: "center",
    flexDirection: "column",
    justifyContent: "center",
    margin: "auto",
  },
  title: {
    color: "#7c4dff",
    fontSize: "26px",
  },
  description: {
    color: "#7c4dff",
    fontSize: "18px",
  },
  tablePaginationWrapper: {
    position: "fixed",
    bottom: 0,
    left: 0,
    right: 0,
    borderTop: "1px solid #ddd",
  },
});
