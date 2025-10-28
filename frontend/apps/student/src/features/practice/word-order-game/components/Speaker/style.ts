import { createUseStyles } from "react-jss";

const PURPLE = "#7c4dff";

export const useStyles = createUseStyles({
  speakerWrap: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    gap: 8,
  },
  speakerBtn: {
    width: 88,
    height: 88,
    borderRadius: 16,
    background: "linear-gradient(145deg, #ffffff, #f6f3ff)",
    border: `1px solid rgba(124,77,255,0.35)`,
    boxShadow:
      "0 4px 14px rgba(124,77,255,0.12), inset 0 1px 0 rgba(255,255,255,0.8)",
    display: "inline-flex",
    alignItems: "center",
    justifyContent: "center",
    cursor: "pointer",
    color: PURPLE,
    transition: "transform .08s ease, box-shadow .2s ease, background .2s ease",
    "&:hover:not(:disabled), &:focus-visible:not(:disabled)": {
      background: "linear-gradient(145deg, #fbfaff, #efe9ff)",
      boxShadow:
        "0 6px 18px rgba(124,77,255,0.18), inset 0 1px 0 rgba(255,255,255,0.9)",
    },
    "&:active:not(:disabled)": {
      transform: "translateY(1px)",
    },
    "&:focus-visible": {
      outline: `2px solid ${PURPLE}`,
      outlineOffset: 2,
    },
    "&:disabled": {
      opacity: 0.6,
      cursor: "not-allowed",
      filter: "grayscale(10%)",
    },
    // animate waves on hover/focus/active
    "&:hover .wave, &:focus-visible .wave, &:active .wave": {
      animation: "$pulse 1.2s ease-out infinite",
    },
  },
  icon: {
    display: "block",
  },
  speakerLabel: {
    fontSize: 16,
    color: "#6f42c1",
    fontWeight: 600,
    letterSpacing: 0.2,
    textShadow: "0 1px 2px rgba(124,77,255,0.08)",
    userSelect: "none",
  },
  "@keyframes pulse": {
    "0%": { opacity: 0.2, transform: "translateX(0)" },
    "50%": { opacity: 0.7, transform: "translateX(0.5px)" },
    "100%": { opacity: 0.2, transform: "translateX(1px)" },
  },
});
