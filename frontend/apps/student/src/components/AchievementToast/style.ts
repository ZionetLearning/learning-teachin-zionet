import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    container: {
      display: "flex",
      alignItems: "center",
      gap: 16,
      padding: 8,
    },
    icon: {
      fontSize: 48,
      color: color.primary,
      filter: `drop-shadow(0 2px 8px rgba(var(${color.primaryMainChannel}) / 0.3))`,
    },
    title: {
      fontSize: 16,
      fontWeight: 700,
      color: color.text,
      marginBottom: 4,
    },
    name: {
      fontSize: 14,
      fontWeight: 600,
      color: color.primary,
      marginBottom: 2,
    },
    description: {
      fontSize: 12,
      color: color.textMuted,
    },
  })();
};
