import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    listContainer: {
      background: color.paper,
      border: `1px solid ${color.divider}`,
      boxShadow: "0 2px 4px rgba(0,0,0,0.04), 0 4px 12px rgba(0,0,0,0.03)",
      borderTopLeftRadius: 0,
      borderBottomLeftRadius: 0,
      borderRadius: 12,
      padding: "1.25rem 1.15rem 1.5rem",
      position: "relative",
      transition: "box-shadow .25s ease, transform .25s ease",
      display: "flex",
      flexDirection: "column",
      minHeight: 0,
      "@supports (height: 100dvh)": {
        height: "calc(100dvh - 46px)",
      },
    },
    sectionTitle: {
      margin: "0 0 0.85rem",
      fontSize: 18,
      fontWeight: 600,
      letterSpacing: 0.3,
      color: color.text,
      textAlign: "center",
    },
    paginationBar: {
      padding: "4px 12px",
      background: color.paper,
    },
  })();
};
