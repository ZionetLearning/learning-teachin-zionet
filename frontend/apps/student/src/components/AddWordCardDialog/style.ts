import { createUseStyles } from "react-jss";

export const PURPLE = "#7c4dff";
const PURPLE_DARK = "#5f35ff";

export const BG_GRADIENT =
  "linear-gradient(180deg, rgba(124,77,255,0.10) 0%, rgba(124,77,255,0.03) 100%)";

export const useStyles = createUseStyles({
  dialogPaper: {
    background: "white",
    borderRadius: 16,
    border: `1px solid rgba(124,77,255,0.18)`,
    boxShadow:
      "0 14px 34px rgba(27, 18, 66, 0.18), 0 3px 10px rgba(0,0,0,0.08)",
    overflow: "hidden",
  },
  dialogBodyGradient: {
    background: BG_GRADIENT,
    padding: 16,
    borderRadius: 16,
  },
  dialogTitle: {
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
  wordPanel: {
    marginBottom: 16,
    padding: 5,
    borderRadius: 12,
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    justifyContent: "center",
    textAlign: "center",
    background: "rgba(124,77,255,0.06)",
    border: `1px solid rgba(124,77,255,0.25)`,
    fontFamily: "'Alef', sans-serif",
  },
  wordMeta: {
    display: "flex",
    flexDirection: "column",
  },
  wordLabel: {
    fontSize: 12,
    fontWeight: 600,
    color: "rgba(0,0,0,0.62)",
    marginBottom: 4,
  },
  hebrewWord: {
    fontSize: 20,
    lineHeight: 1.25,
    color: "#23124a",
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
