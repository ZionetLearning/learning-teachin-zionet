import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  container: {
    width: "100%",
  },

  content: {
    fontSize: "14px",
    lineHeight: "1.6",
    wordWrap: "break-word",
    whiteSpace: "pre-wrap",

    // Markdown styling
    "& strong": {
      fontWeight: "700",
    },

    "& em": {
      fontStyle: "italic",
    },

    "& code": {
      backgroundColor: "rgba(0, 0, 0, 0.1)",
      padding: "2px 4px",
      borderRadius: "3px",
      fontFamily: 'Monaco, Consolas, "Courier New", monospace',
      fontSize: "13px",
    },

    "& br": {
      lineHeight: "1.6",
    },
  },

  "@media (max-width: 768px)": {
    content: {
      fontSize: "13px",
      lineHeight: "1.5",
    },
  },
});
