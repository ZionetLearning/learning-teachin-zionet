import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    chatWrapper: {
      maxWidth: "80%",
      height: "90vh",
      marginTop: "3%",
      margin: "0 auto 30px auto",

      // frame
      border: `1px solid ${color.divider}`,
      borderRadius: 10,

      display: "flex",
      flexDirection: "row",
      position: "relative",
      overflow: "hidden",

      // surface background should match card/dialog surface in theme
      backgroundColor: color.paper,
      color: color.text,

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

      // the inner area behind messages
      // we'll use background.default here so in dark mode it's darker
      backgroundColor: color.bg,
      color: color.text,

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

      // floating chip-style button
      backgroundColor: color.paper + " !important",
      color: color.text + " !important",
      boxShadow:
        "0 2px 8px rgba(0,0,0,0.15), 0 1px 2px rgba(0,0,0,0.08) !important",
      border: `1px solid ${color.divider} !important`,

      "&:hover": {
        backgroundColor: color.paper + " !important",
        boxShadow:
          "0 3px 10px rgba(0,0,0,0.22), 0 2px 4px rgba(0,0,0,0.12) !important",
      },

      // RTL swap
      "[dir='rtl'] &": {
        right: "auto",
        left: "16px",
      },
    },

    muteButtonMuted: {
      backgroundColor: "rgba(244, 67, 54, 0.9) !important",
      color: "#fff !important",
      border: "1px solid rgba(0,0,0,0.2) !important",
      "&:hover": {
        backgroundColor: "rgba(244, 67, 54, 1) !important",
      },
    },

    muteButtonUnmuted: {
      backgroundColor: "rgba(76, 175, 80, 0.9) !important",
      color: "#fff !important",
      border: "1px solid rgba(0,0,0,0.2) !important",
      "&:hover": {
        backgroundColor: "rgba(76, 175, 80, 1) !important",
      },
    },

    wrapper: {
      position: "relative",
      width: 220,
      height: 220,
      margin: "20px auto",
      flexShrink: 0,
    },

    chatElementsWrapper: {
      flex: 1,
      display: "flex",
      flexDirection: "column",
      minHeight: 0,
      overflow: "hidden",
      color: color.text,
    },

    button: {
      marginTop: 10,
      background: "#59BEDFFF",
      color: "#fff",
      padding: [10, 20],
      border: "none",
      borderRadius: 15,
      cursor: "pointer",
      "&:hover": {
        background: "#1B81A6FF",
      },
    },

    buttonRed: {
      marginTop: 10,
      background: "red",
      color: "#fff",
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
      // we do NOT touch sizing or position, just optional shadow matching theme bg
      boxShadow:
        "0 10px 24px rgba(0,0,0,0.25), 0 2px 6px rgba(0,0,0,0.2)",
      border: `1px solid ${color.divider}`,
      backgroundColor: color.paper,
    },

    toolCallBadge: {
      position: "absolute",
      top: 8,
      right: 8,
      padding: "4px 10px",
      borderRadius: 999,

      background: `linear-gradient(
        180deg,
        rgba(var(${color.primaryMainChannel}) / 0.10) 0%,
        rgba(var(${color.primaryMainChannel}) / 0.06) 100%
      )`,

      border: `1px solid rgba(var(${color.primaryMainChannel}) / 0.4)`,
      backdropFilter: "saturate(1.2) blur(6px)",

      fontSize: 12,
      fontWeight: 700,
      color: color.text,

      boxShadow:
        "0 2px 8px rgba(0,0,0,0.4), 0 1px 2px rgba(0,0,0,0.3)",

      pointerEvents: "none",
      whiteSpace: "nowrap",
      transition: "opacity 180ms ease",
    },
  })();
};
