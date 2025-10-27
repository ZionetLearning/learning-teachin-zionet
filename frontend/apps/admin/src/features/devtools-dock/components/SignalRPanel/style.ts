import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  grid: {
    display: "grid",
    gridTemplateColumns: "140px 1fr",
    gap: 8,
    alignItems: "center",
    fontSize: 12,
  },
  pill: {
    display: "inline-flex",
    alignItems: "center",
    gap: 6,
    padding: "2px 8px",
    borderRadius: 999,
    border: "1px solid rgba(255,255,255,.22)",
    background: "rgba(255,255,255,.06)",
    fontWeight: 600,
  },
  dot: {
    width: 8,
    height: 8,
    borderRadius: "50%",
    background: "#ef4444",
    "&.ok": { background: "#10b981" },
    "&.warn": { background: "#f59e0b" },
  },
  log: {
    marginTop: 12,
    fontFamily: "ui-monospace, SFMono-Regular, Menlo, monospace",
    fontSize: 12,
    whiteSpace: "pre-wrap",
    wordBreak: "break-word",
    background: "rgba(255,255,255,.04)",
    border: "1px solid rgba(255,255,255,.1)",
    borderRadius: 8,
    padding: 10,
    maxHeight: 220,
    overflow: "auto",
  },
});
