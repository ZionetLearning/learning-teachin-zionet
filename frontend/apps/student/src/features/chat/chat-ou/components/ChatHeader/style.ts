import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  container: {
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    padding: "0px 20px",
    backgroundColor: "#ffffff",
    borderBottom: "1px solid #e1e5e9",
    minHeight: "65px",
    position: "relative",
    zIndex: 10,
  },

  titleSection: {
    display: "flex",
    flexDirection: "column",
    alignItems: "flex-start",
    flex: 1,
  },

  title: {
    margin: 0,
    fontSize: "18px",
    fontWeight: 600,
    color: "#1a1a1a",
    lineHeight: 1.2,
  },

  subtitle: {
    fontSize: "12px",
    color: "#666",
    marginTop: "2px",
    fontWeight: 400,
  },

  statusSection: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    gap: "4px",
    flex: 1,
    maxWidth: "200px",
  },

  statusIndicator: {
    display: "flex",
    alignItems: "center",
    gap: "6px",
    padding: "4px 8px",
    borderRadius: "12px",
    backgroundColor: "#f8f9fa",
    transition: "all 0.2s ease",
  },

  typing: {
    backgroundColor: "#e3f2fd",
    animation: "$pulse 2s infinite",
  },

  statusIcon: {
    fontSize: "12px",
    lineHeight: 1,
  },

  statusText: {
    fontSize: "12px",
    fontWeight: 500,
    color: "#333",
  },

  typingAnimation: {
    display: "flex",
    alignItems: "center",
    height: "16px",
  },

  typingDots: {
    display: "flex",
    gap: "2px",

    "& span": {
      width: "4px",
      height: "4px",
      borderRadius: "50%",
      backgroundColor: "#007bff",
      animation: "$typingBounce 1.4s infinite ease-in-out",

      "&:nth-child(1)": {
        animationDelay: "0s",
      },
      "&:nth-child(2)": {
        animationDelay: "0.2s",
      },
      "&:nth-child(3)": {
        animationDelay: "0.4s",
      },
    },
  },

  actions: {
    display: "flex",
    alignItems: "center",
    gap: "8px",
    flex: 0,
  },

  actionButton: {
    background: "none",
    border: "none",
    padding: "8px",
    borderRadius: "8px",
    cursor: "pointer",
    fontSize: "16px",
    color: "#666",
    transition: "all 0.2s ease",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    width: "32px",
    height: "32px",

    "&:hover": {
      backgroundColor: "#f0f0f0",
      color: "#333",
    },

    "&:active": {
      transform: "scale(0.95)",
    },
  },

  "@keyframes pulse": {
    "0%": {
      opacity: 1,
    },
    "50%": {
      opacity: 0.7,
    },
    "100%": {
      opacity: 1,
    },
  },

  "@keyframes typingBounce": {
    "0%, 60%, 100%": {
      transform: "translateY(0)",
      opacity: 0.5,
    },
    "30%": {
      transform: "translateY(-6px)",
      opacity: 1,
    },
  },

  // Responsive design
  "@media (max-width: 768px)": {
    container: {
      padding: "12px 16px",
      minHeight: "60px",
    },

    title: {
      fontSize: "16px",
    },

    subtitle: {
      fontSize: "11px",
    },

    statusSection: {
      maxWidth: "150px",
    },

    statusText: {
      fontSize: "11px",
    },

    actionButton: {
      width: "28px",
      height: "28px",
      fontSize: "14px",
    },
  },

  "@media (max-width: 480px)": {
    container: {
      padding: "10px 12px",
    },

    titleSection: {
      flex: 2,
    },

    statusSection: {
      flex: 1,
      maxWidth: "120px",
    },

    actions: {
      gap: "4px",
    },

    actionButton: {
      width: "24px",
      height: "24px",
      fontSize: "12px",
    },
  },
});
