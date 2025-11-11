import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    container: {
      display: "flex",
      flexDirection: "column",
      alignItems: "center",
      gap: 16,
      marginBottom: 24,
    },
    avatarWrapper: {
      position: "relative",
      width: 120,
      height: 120,
    },
    avatar: {
      width: "100%",
      height: "100%",
      fontSize: "2rem",
      backgroundColor: color.primary,
      color: color.paper,
    },
    loadingOverlay: {
      position: "absolute",
      top: 0,
      left: 0,
      right: 0,
      bottom: 0,
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      backgroundColor: "rgba(0, 0, 0, 0.5)",
      borderRadius: "50%",
    },
    buttonGroup: {
      display: "flex",
      gap: 8,
    },
    iconButton: {
      backgroundColor: color.paper,
      border: `1px solid ${color.divider}`,
      "&:hover": {
        backgroundColor: color.divider,
      },
    },
  })();
};
