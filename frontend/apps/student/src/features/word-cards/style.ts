import { createUseStyles } from "react-jss";

const PURPLE = "#7c4dff";
const PURPLE_DARK = "#5f35ff";

export const useStyles = createUseStyles({
  headerWrapper: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    textAlign: "center",
    background:
      "linear-gradient(180deg, rgba(124,77,255,0.10) 0%, rgba(124,77,255,0.03) 100%)",
    padding: "24px 16px 8px",
    marginBottom: 16,
  },
  title: {
    color: "#7c4dff",
    fontSize: 26,
    fontWeight: 700,
    letterSpacing: 0.2,
    marginBottom: 8,
  },
  description: {
    color: "#7c4dff",
    fontSize: 16,
    opacity: 0.9,
    maxWidth: 900,
  },
  helperNote: {
    marginTop: 8,
    color: "rgba(0,0,0,0.62)",
  },
  headerActions: {
    marginTop: 12,
    display: "flex",
    gap: 12,
    alignItems: "center",
    "&[dir='rtl']": {
      flexDirection: "row-reverse",
    },
  },
  addCardBtn: {
    background: PURPLE,
    color: "#fff",
    borderRadius: 12,
    padding: "8px 16px",
    boxShadow: "0 8px 18px rgba(124,77,255,0.28), 0 3px 8px rgba(0,0,0,0.12)",
    "&:hover": {
      background: PURPLE_DARK,
      boxShadow:
        "0 10px 22px rgba(124,77,255,0.34), 0 4px 10px rgba(0,0,0,0.14)",
    },
    "& .MuiButton-startIcon": {
      marginInlineEnd: 8,
    },
    "& .MuiButton-endIcon": {
      marginInlineStart: 8,
    },
    "&[dir='rtl'] .MuiButton-startIcon": {
      marginLeft: 8, // startIcon appears on the left in RTL
      marginRight: 0,
    },
    "&[dir='rtl'] .MuiButton-endIcon": {
      marginRight: 8, // endIcon appears on the right in RTL
      marginLeft: 0,
    },
  },
  body: {
    marginTop: 8,
  },
  centerState: {
    minHeight: 220,
    display: "flex",
    flexDirection: "column",
    gap: 12,
    alignItems: "center",
    justifyContent: "center",
  },
  grid: {
    display: "grid",
    gridTemplateColumns: "repeat(auto-fill, minmax(260px, 1fr))",
    gap: "15px",
    padding: "2%",
  },
});
