import { createUseStyles } from "react-jss";

const PURPLE = "#7c4dff";
const PURPLE_DARK = "#5f35ff";

export const useStyles = createUseStyles({
  actionsBar: {
    display: "flex",
    gap: 12,
    width: "max-content",
    alignSelf: "center",
    padding: 8,
    borderRadius: 16,
    backdropFilter: "saturate(1.2) blur(6px)",
    background:
      "linear-gradient(180deg, rgba(255,255,255,0.8), rgba(255,255,255,0.6))",
    border: "1px solid rgba(13, 59, 80, 0.08)",
    boxShadow: "0 2px 8px rgba(16,24,40,.06), 0 1px 2px rgba(16,24,40,.04)",
    "@media (max-width: 520px)": {
      flexWrap: "wrap",
      width: "100%",
      justifyContent: "center",
      gap: 8,
    },
  },
  btnBase: {
    display: "inline-flex",
    alignItems: "center",
    gap: 8,
    borderRadius: 12,
    padding: "10px 18px",
    fontSize: 16,
    lineHeight: 1.1,
    fontWeight: 600,
    cursor: "pointer",
    transition:
      "background-color .15s ease, box-shadow .15s ease, transform .05s ease",
    border: "1px solid transparent",
    userSelect: "none",
    "&:focus-visible": {
      outline: "none",
      boxShadow:
        "0 0 0 3px rgba(124, 77, 255, 0.25), 0 1px 2px rgba(16,24,40,.06)",
      transform: "translateY(0)",
    },
    "&:active": {
      transform: "translateY(1px)",
    },
    "&[disabled]": {
      opacity: 0.6,
      pointerEvents: "none",
    },
  },
  btnCheck: {
    composes: "$btnBase",
    background: "linear-gradient(180deg, #ede7ff, #e6e0ff)",
    border: `1px solid ${PURPLE}`,
    color: PURPLE_DARK,
    "&:hover": {
      background: "linear-gradient(180deg, #e2d7ff, #d8cbff)",
      boxShadow: "0 2px 6px rgba(124,77,255,0.25)",
    },
    "&:active": {
      background: "#d1bfff",
    },
  },
  btnReset: {
    composes: "$btnBase",
    background: "#f7f5fb",
    border: "1px solid #e0ddee",
    color: "#4a445a",
    "&:hover": {
      background: "#efecf7",
    },
    "&:active": {
      background: "#e5e1f0",
    },
  },
  btnNext: {
    composes: "$btnBase",
    background: `linear-gradient(180deg, ${PURPLE}, ${PURPLE_DARK})`,
    border: `1px solid ${PURPLE_DARK}`,
    color: "#fff",
    boxShadow: "0 2px 8px rgba(124,77,255,0.35)",
    "&:hover": {
      background: `linear-gradient(180deg, ${PURPLE_DARK}, ${PURPLE_DARK})`,
      boxShadow: "0 3px 10px rgba(124,77,255,0.45)",
    },
    "&:active": {
      transform: "translateY(1px)",
      boxShadow: "0 1px 4px rgba(124,77,255,0.3)",
    },
  },
  btnIcon: {
    fontSize: 20,
    flex: "0 0 auto",
    color: "inherit",
  },
});
