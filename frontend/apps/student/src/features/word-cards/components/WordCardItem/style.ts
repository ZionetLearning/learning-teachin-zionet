import { createUseStyles } from "react-jss";

export const PURPLE = "#7c4dff";
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
  },
  primaryBtn: {
    background: PURPLE,
    color: "#fff",
    fontWeight: 800,
    borderRadius: 12,
    padding: "8px 16px",
    boxShadow: "0 8px 18px rgba(124,77,255,0.28), 0 3px 8px rgba(0,0,0,0.12)",
    "&:hover": {
      background: PURPLE_DARK,
      boxShadow:
        "0 10px 22px rgba(124,77,255,0.34), 0 4px 10px rgba(0,0,0,0.14)",
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

  /* Cards grid */
  grid: {
    display: "grid",
    gridTemplateColumns: "repeat(auto-fill, minmax(260px, 1fr))",
    gap: 12,
    padding: 10,
  },
  card: {
    background: "#fff",
    borderRadius: 14,
    border: `1px solid rgba(124,77,255,0.18)`,
    padding: 14,
    boxShadow: "0 4px 16px rgba(0,0,0,0.06)",
    display: "flex",
    flexDirection: "column",
    justifyContent: "space-between",
    minHeight: 130,
  },
  cardTop: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "flex-start",
    gap: 12,
    marginBottom: 6,
  },
  wordGroup: {
    maxWidth: "80%",
  },
  hebrew: {
    direction: "rtl",
    fontWeight: 800,
    fontSize: 20,
    lineHeight: 1.25,
    color: "#23124a",
  },
  english: {
    marginTop: 2,
    color: "rgba(0,0,0,0.70)",
    fontWeight: 600,
  },
  cardFoot: {
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    marginTop: 8,
  },
  learnBtn: {
    padding: 6,
  },
  learnIconActive: {
    color: "#30b566",
  },
  learnIconIdle: {
    color: "rgba(0,0,0,0.28)",
  },

  /* Dialog (Add Card) */
  dialogPaper: {
    background: "#ffffff",
    borderRadius: 16,
    border: `1px solid rgba(124,77,255,0.18)`,
    boxShadow:
      "0 14px 34px rgba(27, 18, 66, 0.18), 0 3px 10px rgba(0,0,0,0.08)",
    overflow: "hidden",
  },
  dialogTitle: {
    fontWeight: 800,
    letterSpacing: "0.2px",
    padding: "16px 56px 8px 24px",
    color: "#1f1142",
  },
  closeButton: {
    position: "absolute",
    right: 8,
    top: 8,
    color: PURPLE,
    "&:hover": { background: "rgba(124,77,255,0.10)" },
  },
  dialogBodyGradient: {
    background:
      "linear-gradient(180deg, rgba(124,77,255,0.10) 0%, rgba(124,77,255,0.03) 100%)",
    padding: 16,
    borderRadius: 16,
  },
  textField: {
    "& .MuiOutlinedInput-root": {
      background: "#ffffff",
      borderRadius: 12,
      "& fieldset": {
        borderColor: "rgba(124,77,255,0.32)",
      },
      "&:hover fieldset": {
        borderColor: PURPLE,
      },
      "&.Mui-focused fieldset": {
        borderColor: PURPLE,
        borderWidth: 2,
      },
      "&.Mui-focused": {
        boxShadow: "0 0 0 2px rgba(124,77,255,0.16)",
      },
    },
    "& .MuiInputLabel-root": {
      color: "rgba(0,0,0,0.62)",
      fontWeight: 600,
    },
    "& .MuiInputLabel-root.Mui-focused": {
      color: PURPLE,
    },
  },
  actions: {
    padding: "0 24px 16px 24px",
    display: "flex",
    gap: 12,
    justifyContent: "flex-end",
  },
  cancelButton: {
    color: PURPLE,
    fontWeight: 700,
    padding: "8px 12px",
    borderRadius: 10,
    "&:hover": {
      background: "rgba(124,77,255,0.10)",
    },
  },
  saveButton: {
    background: PURPLE,
    color: "#fff",
    fontWeight: 800,
    borderRadius: 12,
    padding: "8px 16px",
    boxShadow: "0 8px 18px rgba(124,77,255,0.28), 0 3px 8px rgba(0,0,0,0.12)",
    "&:hover": {
      background: PURPLE_DARK,
      boxShadow:
        "0 10px 22px rgba(124,77,255,0.34), 0 4px 10px rgba(0,0,0,0.14)",
    },
    "&.Mui-disabled": {
      background: "rgba(124,77,255,0.35)",
      color: "rgba(255,255,255,0.92)",
      boxShadow: "none",
    },
  },
});
