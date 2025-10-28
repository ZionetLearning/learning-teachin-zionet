import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  container: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    justifyContent: "flex-start",
    height: "100%",
    minHeight: "100vh",
    background:
      "linear-gradient(180deg, rgba(255,255,255,1) 0%, rgba(249,247,255,0.6) 100%)",
    padding: "0px 20px",
    boxSizing: "border-box",
  },

  headerSection: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    textAlign: "center",
    background:
      "linear-gradient(180deg, rgba(124,77,255,0.12) 0%, rgba(124,77,255,0.04) 100%)",
    borderRadius: 20,
    padding: "32px 40px",
    boxShadow: "0 4px 12px rgba(124,77,255,0.08)",
    marginBottom: 40,
    width: "100%",
  },
});
