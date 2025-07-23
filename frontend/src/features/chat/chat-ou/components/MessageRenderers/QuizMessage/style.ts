import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  container: {
    width: "100%",
  },

  question: {
    fontSize: "18px",
    fontWeight: "600",
    color: "#1f2937",
    marginBottom: "16px",
    lineHeight: "1.5",
  },

  options: {
    display: "flex",
    flexDirection: "column",
    gap: "8px",
    marginBottom: "16px",
  },

  option: {
    display: "flex",
    alignItems: "center",
    padding: "12px",
    backgroundColor: "#f8fafc",
    border: "1px solid #e2e8f0",
    borderRadius: "8px",
    cursor: "pointer",
    transition: "all 0.2s ease",

    "&:hover": {
      backgroundColor: "#f1f5f9",
      borderColor: "#cbd5e1",
    },
  },

  optionDefault: {},

  optionCorrect: {
    backgroundColor: "#dcfce7",
    borderColor: "#16a34a",

    "&:hover": {
      backgroundColor: "#dcfce7",
      borderColor: "#16a34a",
    },
  },

  optionIncorrect: {
    backgroundColor: "#fee2e2",
    borderColor: "#dc2626",

    "&:hover": {
      backgroundColor: "#fee2e2",
      borderColor: "#dc2626",
    },
  },

  optionMissed: {
    backgroundColor: "#fef3c7",
    borderColor: "#d97706",

    "&:hover": {
      backgroundColor: "#fef3c7",
      borderColor: "#d97706",
    },
  },

  optionIndicator: {
    marginRight: "12px",
    minWidth: "20px",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
  },

  checkbox: {
    width: "18px",
    height: "18px",
    border: "2px solid #d1d5db",
    borderRadius: "4px",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    fontSize: "12px",
    color: "#ffffff",
    transition: "all 0.2s ease",
  },

  checkboxChecked: {
    backgroundColor: "#3b82f6",
    borderColor: "#3b82f6",
  },

  radio: {
    width: "18px",
    height: "18px",
    border: "2px solid #d1d5db",
    borderRadius: "50%",
    position: "relative",
    transition: "all 0.2s ease",

    "&::after": {
      content: '""',
      position: "absolute",
      top: "50%",
      left: "50%",
      transform: "translate(-50%, -50%)",
      width: "8px",
      height: "8px",
      borderRadius: "50%",
      backgroundColor: "transparent",
      transition: "all 0.2s ease",
    },
  },

  radioSelected: {
    borderColor: "#3b82f6",

    "&::after": {
      backgroundColor: "#3b82f6",
    },
  },

  optionText: {
    flex: 1,
    fontSize: "14px",
    color: "#374151",
    lineHeight: "1.5",
  },

  correctIndicator: {
    marginLeft: "8px",
    color: "#16a34a",
    fontSize: "16px",
    fontWeight: "bold",
  },

  actions: {
    display: "flex",
    justifyContent: "flex-end",
    marginTop: "16px",
  },

  button: {
    padding: "8px 16px",
    borderRadius: "6px",
    border: "none",
    fontSize: "14px",
    fontWeight: "500",
    cursor: "pointer",
    transition: "all 0.2s ease",
    fontFamily:
      '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',

    "&:disabled": {
      opacity: 0.5,
      cursor: "not-allowed",
    },
  },

  submitButton: {
    backgroundColor: "#3b82f6",
    color: "#ffffff",

    "&:hover:not(:disabled)": {
      backgroundColor: "#2563eb",
    },
  },

  resetButton: {
    backgroundColor: "#6b7280",
    color: "#ffffff",

    "&:hover": {
      backgroundColor: "#4b5563",
    },
  },

  results: {
    marginTop: "16px",
    padding: "16px",
    backgroundColor: "#f8fafc",
    borderRadius: "8px",
    border: "1px solid #e2e8f0",
  },

  score: {
    fontSize: "16px",
    fontWeight: "600",
    color: "#1f2937",
    marginBottom: "8px",
  },

  explanation: {
    fontSize: "14px",
    color: "#4b5563",
    lineHeight: "1.5",
    marginBottom: "12px",

    "& strong": {
      color: "#1f2937",
    },
  },
});
