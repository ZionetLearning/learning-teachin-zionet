import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  container: {
    maxWidth: "1200px",
    mx: "auto",
    p: 24,
    height: "calc(100vh - 100px)", // Limit to viewport height
    overflow: "auto",
  },
  loadingContainer: {
    display: "flex",
    justifyContent: "center",
    py: 48,
  },
  buttonsContainer: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    mb: 24
  },
  innerButtonsContainer: {
    display: "flex",
    gap: 8,
  },
  formControl: {
    minWidth: 120,
  },
  navigationButtons: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    mt: 24,
  },
});
