import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    root: {
      display: "flex",
      gap: "2rem",
      alignItems: "stretch",
      padding: "1.5rem",
      background: `linear-gradient(135deg, rgba(var(${color.primaryMainChannel}) / 0.04) 0%, rgba(var(${color.primaryMainChannel}) / 0.08) 100%)`,
      height: "calc(100vh - 3rem)",
      overflowY: "auto",
      "@media (max-width: 768px)": {
        flexDirection: "column",
        padding: "1rem",
        gap: "1.5rem",
        height: "auto",
        minHeight: "100vh",
        overflowY: "visible",
      },
      "@media (max-width: 480px)": {
        padding: "0.75rem",
        gap: "1rem",
      },
    },
  })();
};
