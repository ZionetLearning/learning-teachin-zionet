import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  root: {
    borderRadius: 999,
    fontWeight: 500,
    background: "#eef2f8",
    border: "1px solid #d6deea",
    color: "#23437a",
    boxShadow: "0 1px 0 rgba(16,24,40,0.04)",
    transition: "background .2s, border-color .2s, box-shadow .2s",
    direction: "ltr",
    "& .MuiChip-deleteIcon": {
      color: "#23437a",
      opacity: 0.7,
      transition: "opacity 0.2s, transform 0.1s",
      "&:hover": { opacity: 1 },
      "&:active": { transform: "scale(0.9)" },
    },
    "&:hover": {
      background: "#e6edf7",
      borderColor: "#cad6ea",
    },
    "&:focus-visible": {
      outline: "none",
      boxShadow: "0 0 0 2px rgba(35,67,122,0.18)",
    },
  },
});
