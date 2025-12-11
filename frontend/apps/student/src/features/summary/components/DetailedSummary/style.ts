import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    container: {
      width: "100%",
      display: "flex",
      flexDirection: "column",
    },
    tabs: {
      borderBottom: `1px solid ${color.divider}`,
      "& .MuiTabs-indicator": {
        height: 3,
      },
    },
    tab: {
      textTransform: "none",
      fontSize: "1rem",
      fontWeight: 500,
      minHeight: 64,
      gap: 8,
    },
    content: {
      padding: "24px 0",
      minHeight: 400,
    },
  })();
};
