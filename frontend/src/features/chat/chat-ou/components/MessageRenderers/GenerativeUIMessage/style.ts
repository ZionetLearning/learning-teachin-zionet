import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  container: {
    width: "100%",
    // Remove redundant styling - MessageItem handles the container
  },

  fallbackText: {
    fontSize: "14px",
    lineHeight: "1.6",
    color: "#374151",
    marginBottom: "8px",
    fontFamily:
      '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
  },

  componentInfo: {
    fontSize: "12px",
    color: "#9ca3af",
    fontStyle: "italic",
    padding: "4px 8px",
    backgroundColor: "#f3f4f6",
    borderRadius: "6px",
    display: "inline-block",
  },

  // Styles for when component is successfully rendered
  componentWrapper: {
    width: "100%",
    "& > *": {
      width: "100%",
    },
  },
});
