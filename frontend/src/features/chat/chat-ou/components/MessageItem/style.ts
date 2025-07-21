import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  container: {
    display: "flex",
    flexDirection: "column",
    marginBottom: "16px",
    padding: "12px 16px",
    borderRadius: "12px",
    maxWidth: "80%",
    wordWrap: "break-word",
    transition: "all 0.2s ease",
    "&:hover": {
      transform: "translateY(-1px)",
      boxShadow: "0 2px 8px rgba(0, 0, 0, 0.1)",
    },
  },

  userMessage: {
    alignSelf: "flex-end",
    backgroundColor: "#007bff",
    color: "white",
    marginLeft: "20%", // Add left margin to push user messages more to the right
    borderBottomRightRadius: "4px", // Make the bottom right corner less rounded for user messages
    "& $senderName": {
      color: "rgba(255, 255, 255, 0.9)",
    },
    "& $timestamp": {
      color: "rgba(255, 255, 255, 0.7)",
    },
    "& $contextLabel": {
      color: "rgba(255, 255, 255, 0.8)",
    },
    "& $contextItem": {
      color: "rgba(255, 255, 255, 0.7)",
    },
  },

  aiMessage: {
    alignSelf: "flex-start",
    backgroundColor: "#f8f9fa",
    color: "#333",
    border: "1px solid #e9ecef",
    marginRight: "20%", // Add right margin to keep AI messages more to the left
    borderBottomLeftRadius: "4px", // Make the bottom left corner less rounded for AI messages
  },

  messageHeader: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: "8px",
    fontSize: "12px",
  },

  senderInfo: {
    display: "flex",
    alignItems: "center",
    gap: "8px",
  },

  avatar: {
    width: "20px",
    height: "20px",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    fontSize: "16px",
    lineHeight: 1,
  },

  senderName: {
    fontWeight: "600",
    color: "#666",
  },

  timestamp: {
    color: "#999",
    fontSize: "11px",
  },

  messageContent: {
    lineHeight: "1.5",
    // Ensure inner components have proper spacing
    "& > *": {
      width: "100%",
    },
  },

  contextInfo: {
    marginTop: "8px",
    padding: "8px",
    borderRadius: "6px",
    backgroundColor: "rgba(0, 0, 0, 0.05)",
    fontSize: "12px",
    display: "flex",
    flexDirection: "column",
    gap: "4px",
  },

  contextLabel: {
    fontWeight: "600",
    color: "#666",
  },

  contextItem: {
    color: "#777",
    fontStyle: "italic",
  },

  fallback: {
    color: "#999",
    fontStyle: "italic",
    padding: "8px",
    textAlign: "center",
  },

  "@media (max-width: 768px)": {
    container: {
      maxWidth: "90%",
      padding: "10px 12px",
    },

    userMessage: {
      marginLeft: "10%", // Reduce left margin on mobile
    },

    aiMessage: {
      marginRight: "10%", // Reduce right margin on mobile
    },

    messageHeader: {
      fontSize: "11px",
    },

    timestamp: {
      fontSize: "10px",
    },
  },
});
