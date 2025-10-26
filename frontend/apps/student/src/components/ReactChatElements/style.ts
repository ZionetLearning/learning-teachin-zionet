import { createUseStyles } from "react-jss";

export const PURPLE = "#7c4dff";

export const useStyles = createUseStyles({
  chatContainer: {
    display: "flex",
    flexDirection: "column",
    height: "100%",
    background:
      "linear-gradient(180deg, #fafbff 0%, #f7f5ff 60%, #f5f3ff 100%)",
  },

  // Scroll area
  messagesList: {
    flex: 1,
    overflowY: "auto",
    overflowX: "hidden",
    padding: 16,
    display: "flex",
    flexDirection: "column",
    scrollBehavior: "smooth",
    "[dir='rtl'] &": { direction: "rtl" },
  },
  messageBox: {
    "& .rce-mbox": {
      background: "transparent !important",
      border: "none !important",
      boxShadow: "none !important",
    },
    "& .rce-mbox-text": {
      whiteSpace: "pre-wrap",
      lineHeight: 1.5,
      fontSize: 15,
    },

    "& svg": { display: "none !important" },

    // Titles alignment for RTL/LTR
    "& .rce-mbox-right .rce-mbox-title": {
      textAlign: "right",
      justifyContent: "flex-end",
      "[dir='rtl'] &": { textAlign: "left", justifyContent: "flex-start" },
    },
    "& .rce-mbox-left .rce-mbox-title": {
      "[dir='rtl'] &": { textAlign: "right", justifyContent: "flex-end" },
    },
  },

  bubbleRight: {
    "& .rce-container-mbox-right": {
      flexDirection: "row-reverse",
      "[dir='rtl'] &": { flexDirection: "row" },
    },
    "& .rce-mbox-body": {
      background: `linear-gradient(180deg, ${PURPLE} 0%, #6b3aff 100%)`,
      color: "#fff",
      borderRadius: 12,
      padding: "6px 10px",
      boxShadow: "0 3px 8px rgba(124,77,255,0.22)",
      border: "1px solid rgba(255,255,255,0.15)",
      fontSize: 12,
      lineHeight: 1.4,
      textAlign: "start",
      direction: "inherit",
      "& p, & div, & span": {
        textAlign: "start !important",
      },
    },
    "& .rce-mbox-title": {
      color: "rgba(255,255,255,0.8)",
      fontWeight: 500,
      marginBottom: 2,
      fontSize: 12,
    },
    "& .rce-mbox-time": {
      color: "rgba(255, 255, 255, 0.75) !important",
      fontSize: 11,
      marginTop: 2,
    },
  },

  bubbleLeft: {
    "& .rce-container-mbox-left": {
      "[dir='rtl'] &": { flexDirection: "row-reverse" },
    },
    "& .rce-mbox-body": {
      background: "linear-gradient(180deg, #ffffff, #f9f8ff)",
      color: "#1f2937",
      borderRadius: 12,
      padding: "6px 10px",
      border: "1px solid rgba(124,77,255,0.12)",
      boxShadow: "0 2px 6px rgba(16,24,40,.04)",
      fontSize: 12,
      lineHeight: 1.4,
      textAlign: "start",
      direction: "inherit",
      "& p, & div, & span": {
        textAlign: "start !important",
      },
    },
    "& .rce-mbox-title": {
      color: "#5b5f6a",
      fontWeight: 500,
      marginBottom: 2,
      fontSize: 12,
    },
  },

  inputContainer: {
    flexShrink: 0,
    padding: 10,
    background:
      "linear-gradient(180deg, rgba(255,255,255,0.8), rgba(245,243,255,0.9))",
    backdropFilter: "saturate(1.1) blur(6px)",
    borderTop: "1px solid rgba(124,77,255,0.15)",
    "[dir='rtl'] &": { direction: "rtl" },
  },

  input: {
    width: "100% !important",
    border: "1px solid rgba(124,77,255,0.25)",
    borderRadius: 24,
    padding: "4px 6px",
    background: "rgba(255,255,255,0.9)",
    boxShadow: "inset 0 1px 2px rgba(16,24,40,.06)",
    "& input": {
      color: "#111",
      padding: "10px 12px",
      fontSize: 15,
      outline: "none",
      "[dir='rtl'] &": { textAlign: "right" },
      "@media (prefers-color-scheme: dark)": {
        color: "black",
        background: "rgba(255,255,255,0.9)",
      },
    },
  },

  sendButton: {
    marginLeft: 8,
    background: `linear-gradient(180deg, ${PURPLE} 0%, #6b3aff 100%)`,
    color: "#fff",
    borderRadius: 999,
    padding: "10px 16px",
    fontSize: 12,
    fontWeight: 700,
    border: "1px solid rgba(255,255,255,0.2)",
    boxShadow: "0 6px 14px rgba(124,77,255,0.35)",
    cursor: "pointer",
    transition: "transform .06s ease, box-shadow .12s ease, opacity .12s ease",
    "&:hover": { transform: "translateY(-1px)" },
    "&:active": { transform: "translateY(0)" },
    "&:disabled": {
      opacity: 0.6,
      cursor: "not-allowed",
      boxShadow: "none",
    },
    "[dir='rtl'] &": { marginLeft: 0, marginRight: 8 },
  },
});
