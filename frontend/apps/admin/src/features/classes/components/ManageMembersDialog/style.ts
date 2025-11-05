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
      borderBottom: `1px solid ${color.divider}`,
    },
    content: {
      height: 420,
      display: "flex",
      flexDirection: "column",
      overflow: "hidden",
      background: color.paper,
      color: color.text,
      "@media (max-width: 600px)": {
        height: "min(90vh, 620px)",
      },
    },
    panels: {
      flex: 1,
      overflow: "hidden",
      display: "flex",
      gap: 16,
      marginTop: 16,
      "@media (max-width: 900px)": {
        flexDirection: "column",
      },
    },
    dividerV: {
      alignSelf: "stretch",
      borderColor: color.divider,
    },
    actions: {
      borderTop: `1px solid ${color.divider}`,
      padding: "8px 12px",
    },
  })();
};
