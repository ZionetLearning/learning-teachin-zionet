import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  container: {
    width: "100%",
    cursor: "pointer",
    transition: "all 0.2s ease",

    "&:hover": {
      transform: "translateY(-1px)",
    },
  },

  content: {
    display: "flex",
    alignItems: "center",
    padding: "8px 0",
    transition: "all 0.2s ease",

    "&:hover": {
      opacity: 0.8,
    },
  },

  icon: {
    fontSize: "24px",
    marginRight: "12px",
    minWidth: "32px",
    textAlign: "center",
  },

  textContent: {
    flex: 1,
    minWidth: 0,
  },

  title: {
    fontSize: "16px",
    fontWeight: "600",
    color: "#1f2937",
    marginBottom: "4px",
    fontFamily:
      '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',

    overflow: "hidden",
    textOverflow: "ellipsis",
    whiteSpace: "nowrap",
  },

  description: {
    fontSize: "14px",
    color: "#6b7280",
    lineHeight: "1.4",

    display: "-webkit-box",
    WebkitLineClamp: 2,
    WebkitBoxOrient: "vertical",
    overflow: "hidden",
  },

  arrow: {
    fontSize: "18px",
    color: "#9ca3af",
    marginLeft: "12px",
    transition: "all 0.2s ease",

    "$container:hover &": {
      color: "#3b82f6",
      transform: "translateX(2px)",
    },
  },
});
