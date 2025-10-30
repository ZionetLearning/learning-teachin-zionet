import { useThemeColors } from "@app-providers";
import { createUseStyles } from "react-jss";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    layout: {
      display: "flex",
      height: "100vh",
    },
    content: {
      flexGrow: 1,
      position: "relative",
      overflowY: "auto",
      overflowX: "hidden",
      backgroundColor: color.bg,
      color: color.text,
    },
  })();
};
