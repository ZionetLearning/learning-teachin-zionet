import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  const headerBg = color.primaryMainChannel
    ? `linear-gradient(
        180deg,
        rgba(var(${color.primaryMainChannel}) / 0.10) 0%,
        rgba(var(${color.primaryMainChannel}) / 0.03) 100%
      )`
    : color.paper;

  return createUseStyles({
    title: {
      margin: 0,
      color: color.text,
      background: headerBg,
      padding: "12px 16px",
      fontSize: 18,
      borderBottom: `1px solid ${color.divider}`,
      "@media (max-width: 600px)": {
        fontSize: 16,
        padding: "10px 14px",
      },
    },

    content: {
      height: 460,
      display: "flex",
      flexDirection: "column",
      overflow: "hidden",
      background: color.paper,
      color: color.text,
      "@media (max-width: 900px)": {
        height: "min(85vh, 620px)",
      },
      "@media (max-width: 600px)": {
        height: "min(88vh, 620px)",
        padding: "4px",
      },
    },

    panels: {
      flex: 1,
      minHeight: 0,
      display: "flex",
      gap: 16,
      marginTop: 16,
      overflowY: "auto",
      overflowX: "hidden",
      WebkitOverflowScrolling: "touch",
      overscrollBehavior: "contain",
      "@media (max-width: 900px)": {
        flexDirection: "column",
        gap: 12,
      },
      "@media (max-width: 600px)": {
        marginTop: 12,
        gap: 10,
      },
    },

    dividerV: {
      alignSelf: "stretch",
      borderColor: color.divider,
      "@media (max-width: 900px)": {
        display: "none",
      },
    },

    actions: {
      borderTop: `1px solid ${color.divider}`,
      padding: "8px 12px",
      display: "flex",
      justifyContent: "flex-end",
      "@media (max-width: 600px)": {
        padding: "8px 10px",
      },
    },
  })();
};
