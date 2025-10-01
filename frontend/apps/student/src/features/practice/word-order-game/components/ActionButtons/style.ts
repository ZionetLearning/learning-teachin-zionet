import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  actionButtons: {
    display: "flex",
    flexDirection: "row",
    gap: "20px",
    width: "max-content",
    alignSelf: "center",
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
