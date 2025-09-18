import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  gameHeader: {
    position: "absolute",
    insetBlockEnd: 16,
    insetInlineEnd: 16,
    zIndex: 2,
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    gap: 12,
    paddingBlock: 10,
    paddingInline: 12,
    borderRadius: 12,
    background: "rgba(255,255,255,0.72)",
    boxShadow: "0 2px 8px rgba(0,0,0,0.12), 0 12px 28px -18px rgba(0,0,0,0.35)",
    backdropFilter: "saturate(120%) blur(6px)",
    border: "1px solid rgba(255,255,255,0.14)",
    maxWidth: "min(92vw, 560px)",
    "@media (prefers-color-scheme: light)": {
      background: "rgba(245,247,250,0.9)",
      border: "1px solid rgba(0,0,0,0.06)",
      boxShadow:
        "0 1px 3px rgba(0,0,0,0.08), 0 10px 24px -18px rgba(0,0,0,0.25)",
    },
    "@media (prefers-color-scheme: dark)": {
      background: "rgba(255,255,255,0.72)",
      border: "1px solid rgba(255,255,255,0.10)",
    },
  },
  gameHeaderInfo: {
    display: "flex",
    alignItems: "center",
    gap: 8,
    flexWrap: "wrap",
    "& .MuiTypography-root": {
      display: "inline-block",
      paddingInline: 10,
      paddingBlock: 4,
      borderRadius: 999,
      background: "rgba(0,0,0,0.06)",
      color: "rgba(0,0,0,0.75)",
      fontSize: 12,
      lineHeight: 1.1,
    },
    "@media (prefers-color-scheme: dark)": {
      "& .MuiTypography-root": {
        background: "rgba(255,255,255,0.14)",
        color: "rgba(0, 0, 0, 0.8)",
      },
    },
  },
  settingsButtonEnglish: {
    minWidth: 100,
    whiteSpace: "nowrap",
    borderRadius: 999,
    textTransform: "none",
    fontWeight: 600,
    paddingInline: 14,
    display: "inline-flex",
    alignItems: "center",
    justifyContent: "center",
    gap: 8,
    "& .MuiButton-startIcon, & .MuiButton-endIcon": {
      margin: 0,
    },
  },
  settingsButtonHebrew: {
    minWidth: 100,
    whiteSpace: "nowrap",
    borderRadius: 999,
    textTransform: "none",
    fontWeight: 600,
    paddingInline: 14,

    display: "inline-flex",
    alignItems: "center",
    justifyContent: "center",
    gap: 8,
    "& .MuiButton-startIcon, & .MuiButton-endIcon": {
      margin: 0,
    },
  },
});
