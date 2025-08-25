import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  speaker: {
    width: 80,
    height: 80,
    borderRadius: 16,
    backgroundColor: "white",
    border: "1px solid rgb(200, 200, 200)",
    boxShadow: "0 1px 2px rgba(0,0,0,0.06)",
    display: "inline-flex",
    alignItems: "center",
    justifyContent: "center",
    cursor: "pointer",
    fontSize: 34,
    lineHeight: 1,
    transition: "background .15s, transform .05s",
    "&:hover": { background: "#f8fbfd" },
    "&:active": { transform: "translateY(1px)" },
    "&:focus-visible": {
      outline: "2px solid #66afe9",
      outlineOffset: 2,
    },
    "& span": {
      lineHeight: 1,
      display: "block",
    },
  },
});
