import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    pageContainer: {
      padding: "14px 0",
      display: "flex",
      flexDirection: "column",
      gap: 20,
    },
    headerSection: {
      display: "grid",
      gridTemplateColumns: "repeat(auto-fit, minmax(450px, 1fr))",
      gap: 15,
      alignItems: "stretch",
      alignContent: "stretch",
      width: "100%",
      boxSizing: "border-box",
    },
    headerColumn: {
      width: "100%",
      display: "flex",
      flexDirection: "column",
      height: "auto",
      boxSizing: "border-box",
    },
    mainContent: {
      width: "100%",
      minHeight: 360,
      background: color.paper,
      border: `1px solid ${color.divider}`,
      borderRadius: 12,
      padding: "0 24px 24px 24px",
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      textAlign: "center",
      boxSizing: "border-box",
    },
  })();
};
