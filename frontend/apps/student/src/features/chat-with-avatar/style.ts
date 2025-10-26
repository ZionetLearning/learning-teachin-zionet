import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  chatWrapper: {
    maxWidth: "80%",
    height: "90vh",
    marginTop: "3%",
    margin: "0 auto 30px auto",
    border: "1px solid #ccc",
    borderRadius: "10px",
    display: "flex",
    flexDirection: "row",
    position: "relative",
    overflow: "hidden",
    "[dir='rtl'] &": {
      direction: "rtl",
    },
  },

  mainContent: {
    flex: 1,
    display: "flex",
    flexDirection: "column",
    transition:
      "margin-left 0.3s cubic-bezier(0.4, 0, 0.2, 1), margin-right 0.3s cubic-bezier(0.4, 0, 0.2, 1)",
    backgroundColor: "#fafafa",
    height: "100%",
    overflow: "hidden",
    minHeight: 0,
    position: "relative",
  },

  mainContentShifted: {
    "@media (min-width: 769px)": {
      marginLeft: "320px",
      "[dir='rtl'] &": {
        marginLeft: 0,
        marginRight: "320px",
      },
    },
  },

  muteButton: {
    position: "absolute !important",
    top: "23px",
    right: "16px",
    zIndex: 1000,
    backgroundColor: "rgba(255, 255, 255, 0.9) !important",
    boxShadow: "0 2px 8px rgba(0, 0, 0, 0.15) !important",
    "&:hover": {
      backgroundColor: "rgba(255, 255, 255, 1) !important",
    },
    // RTL positioning
    "[dir='rtl'] &": {
      right: "auto",
      left: "16px",
    },
  },

  muteButtonMuted: {
    backgroundColor: "rgba(244, 67, 54, 0.9) !important", // Red for muted
    color: "white !important",
    "&:hover": {
      backgroundColor: "rgba(244, 67, 54, 1) !important",
    },
  },

  muteButtonUnmuted: {
    backgroundColor: "rgba(76, 175, 80, 0.9) !important", // Green for unmuted
    color: "white !important",
    "&:hover": {
      backgroundColor: "rgba(76, 175, 80, 1) !important",
    },
  },

  wrapper: {
    position: "relative",
    width: "220px",
    height: "220px",
    margin: "20px auto",
    flexShrink: 0,
  },

  chatElementsWrapper: {
    flex: 1,
    display: "flex",
    flexDirection: "column",
    minHeight: 0,
    overflow: "hidden",
  },
  button: {
    marginTop: "10px",
    background: "#59BEDFFF",
    color: "white",
    padding: [10, 20],
    border: "none",
    borderRadius: 15,
    cursor: "pointer",
    "&:hover": {
      background: "#1B81A6FF",
    },
  },
  buttonRed: {
    marginTop: "10px",
    background: "red",
    color: "white",
    padding: [10, 20],
    border: "none",
    borderRadius: 15,
    cursor: "pointer",
  },
  lipsImage: {
    position: "absolute",
    top: "42%",
    left: "40%",
    width: "20%",
    height: "20%",
    pointerEvents: "none",
  },
  avatar: {
    width: "100%",
    height: "100%",
    borderRadius: "50%",
    objectFit: "cover",
  },
  toolCallBadge: {
    position: "absolute",
    top: 8,
    right: 8,
    padding: "4px 10px",
    borderRadius: 999,
    background:
      "linear-gradient(180deg, rgba(255,255,255,0.72), rgba(255,255,255,0.58))",
    border: "1px solid rgba(17,187,255,0.35)",
    backdropFilter: "saturate(1.2) blur(6px)",
    fontSize: 12,
    fontWeight: 700,
    color: "#0c3b5d",
    boxShadow: "0 2px 8px rgba(16,24,40,.06), 0 1px 2px rgba(16,24,40,.04)",
    pointerEvents: "none",
    whiteSpace: "nowrap",
    transition: "opacity 180ms ease",
  },
});
