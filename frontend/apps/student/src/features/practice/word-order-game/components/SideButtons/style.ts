import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  sideButtons: {
    display: "flex",
    flexDirection: "column",
    gap: "20px",
    paddingLeft: "100px",
    paddingRight: "95px",
    "& > button": {
      background: "#eef6fb",
      border: "1px solid #cfe3ef",
      color: "#0d3b50",
      borderRadius: 12,
      padding: "10px 18px",
      fontSize: 16,
      cursor: "pointer",
      transition: "background .15s, transform .05s",
    },
    "& > button:hover": {
      background: "#dff0f9",
    },
    "& > button:active": {
      transform: "translateY(1px)",
    },
  },
});
