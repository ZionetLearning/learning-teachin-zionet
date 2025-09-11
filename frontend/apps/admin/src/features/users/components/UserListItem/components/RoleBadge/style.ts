import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  roleBadge: {
    display: "inline-flex",
    alignItems: "center",
    justifyContent: "center",
    padding: "2px 10px",
    borderRadius: 999,
    fontSize: 12,
    fontWeight: 600,
    lineHeight: 1,
    letterSpacing: 0.3,
    border: "1px solid transparent",
    minWidth: 64,
    textTransform: "capitalize",
    userSelect: "none",
  },
  roleStudent: {
    background: "rgba(99,102,241,0.10)",
    color: "#3730a3", // indigo-800
    borderColor: "rgba(99,102,241,0.35)",
  },
  roleTeacher: {
    background: "rgba(16,185,129,0.10)",
    color: "#065f46", // emerald-800
    borderColor: "rgba(16,185,129,0.35)",
  },
  roleAdmin: {
    background: "rgba(239,68,68,0.10)",
    color: "#7f1d1d", // red-900
    borderColor: "rgba(239,68,68,0.35)",
  },
});
