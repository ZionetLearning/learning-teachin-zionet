import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  root: {
    display: "flex",
    gap: "2rem",
    alignItems: "stretch",
    padding: "1.5rem",
    background: "linear-gradient(135deg,#f7f9fc 0%, #eef2f7 100%)",
    height: "calc(100vh - 3rem)",
  },
});
