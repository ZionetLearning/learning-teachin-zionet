import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    sectionShell: {
      width: "100%",
      padding: 24,
      borderRadius: 16,
      background: color.paper,
      border: `1px solid rgba(var(${color.primaryMainChannel}) / 0.18)`,
      boxShadow: "0 4px 12px rgba(0, 0, 0, 0.08)",
      boxSizing: "border-box",
    },
    centerContent: {
      minHeight: 200,
      display: "flex",
      flexDirection: "column",
      alignItems: "center",
      justifyContent: "center",
      textAlign: "center",
      gap: 12,
    },
  })();
};
