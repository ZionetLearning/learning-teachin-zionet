import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    container: {
      padding: 24,
      borderRadius: 16,
      background: color.paper,
      border: `1px solid rgba(var(${color.primaryMainChannel}) / 0.18)`,
      boxShadow: `0 4px 12px rgba(0, 0, 0, 0.08)`,
    },
    title: {
      fontSize: 24,
      fontWeight: 700,
      color: color.text,
      marginBottom: 8,
    },
    subtitle: {
      fontSize: 14,
      color: color.textMuted,
      marginBottom: 24,
    },
    grid: {
      display: "grid",
      gridTemplateColumns: "repeat(auto-fill, minmax(150px, 1fr))",
      gap: 16,
      marginTop: 16,
      "@media (max-width: 600px)": {
        gridTemplateColumns: "repeat(2, 1fr)",
      },
    },
  })();
};
