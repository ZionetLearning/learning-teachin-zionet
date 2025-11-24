import { createUseStyles } from "react-jss";
import { useColorScheme } from "@mui/material";

export const PURPLE = "#7c4dff";
type ModeLike = "light" | "dark" | "system" | undefined;

const useStylesInternal = createUseStyles({
  chatContainer: ({ mode }: { mode: ModeLike }) => ({
    display: "flex",
    flexDirection: "column",
    height: "100%",
    background:
      mode === "dark"
        ? "linear-gradient(180deg, #2a2b32 0%, #2a2b32 30%, #2a2b32 30%, #2b2835 60%, #2b2638 100%)"
        : "linear-gradient(180deg, #fafbff 0%, #f7f5ff 60%, #f5f3ff 100%)",
  }),

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

    "& .rce-mbox-right .rce-mbox-title": {
      textAlign: "right",
      justifyContent: "flex-end",
      "[dir='rtl'] &": {
        textAlign: "left",
        justifyContent: "flex-start",
      },
    },
    "& .rce-mbox-left .rce-mbox-title": {
      "[dir='rtl'] &": {
        textAlign: "right",
        justifyContent: "flex-end",
      },
    },
  },

  // USER bubble
  bubbleRight: {
    "& .rce-container-mbox-right": {
      flexDirection: "row-reverse",
      "[dir='rtl'] &": { flexDirection: "row" },
    },

    "& .rce-mbox-body": {
      background: `linear-gradient(180deg, ${PURPLE} 0%, #8D6CF0FF 100%)`,
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

    "& .rce-mbox-time": ({ mode }: { mode: "light" | "dark" | undefined }) => ({
      color: mode === "dark" ? "white" : "black",
      fontSize: 11,
      marginTop: 2,
    }),
  },

  // ASSISTANT bubble
  bubbleLeft: {
    "& .rce-container-mbox-right": {
      flexDirection: "row-reverse",
      "[dir='rtl'] &": { flexDirection: "row" },
    },

    "& .rce-mbox-body": {
      background: "white",
      color: "black",
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

    "& .rce-mbox-time": ({ mode }: { mode: "light" | "dark" | undefined }) => ({
      color: mode === "dark" ? "white" : "black",
      fontSize: 11,
      marginTop: 2,
    }),
  },

  inputContainer: ({ mode }: { mode: "light" | "dark" | undefined }) => ({
    flexShrink: 0,
    padding: 10,
    background:
      mode === "dark"
        ? "linear-gradient(180deg, rgba(34,34,40,0.9), rgba(28,28,32,0.9))"
        : "linear-gradient(180deg, rgba(255,255,255,0.8), rgba(245,243,255,0.9))",

    backdropFilter: "saturate(1.1) blur(6px)",
    borderTop:
      mode === "dark"
        ? "1px solid rgba(124,77,255,0.3)"
        : "1px solid rgba(124,77,255,0.15)",

    boxShadow:
      mode === "dark"
        ? "0 -12px 24px rgba(0,0,0,0.8)"
        : "0 -8px 16px rgba(0,0,0,0.08)",

    "[dir='rtl'] &": { direction: "rtl" },
  }),

  input: ({ mode }: { mode: "light" | "dark" | undefined }) => ({
    width: "100% !important",
    border:
      mode === "dark"
        ? "1px solid rgba(124,77,255,0.4)"
        : "1px solid rgba(124,77,255,0.25)",
    borderRadius: 24,
    padding: "4px 6px",
    background: mode === "dark" ? "rgba(0,0,0,0.7)" : "rgba(255,255,255,0.9)",
    boxShadow:
      mode === "dark"
        ? "0 0 12px rgba(124,77,255,0.25), inset 0 1px 2px rgba(0,0,0,0.9)"
        : "inset 0 1px 2px rgba(16,24,40,.06)",

    "& input": {
      color: mode === "dark" ? "#f8f8f8" : "#111",
      background: "transparent",
      border: "none",
      padding: "10px 12px",
      fontSize: 15,
      outline: "none",

      "[dir='rtl'] &": { textAlign: "right" },
    },
  }),

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

    "&:hover": {
      transform: "translateY(-1px)",
    },
    "&:active": {
      transform: "translateY(0)",
    },
    "&:disabled": {
      opacity: 0.6,
      cursor: "not-allowed",
      boxShadow: "none",
    },

    "[dir='rtl'] &": { marginLeft: 0, marginRight: 8 },
  },
});

export const useStylesWithMode = () => {
  const { mode } = useColorScheme();
  const normalizedMode: "light" | "dark" | undefined =
    mode === "system" ? undefined : mode;
  return useStylesInternal({ mode: normalizedMode });
};
