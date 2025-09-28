import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  chatWrapper: {
    maxWidth: "900px",
    height: "570px",
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
});
