import { useThemeColors } from "@app-providers";
import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  layout: {
    display: "flex",
    height: "100vh",
  },
  content: (color: ReturnType<typeof useThemeColors>) => ({
    flexGrow: 1,
    position: "relative",
    overflowY: "auto",
    overflowX: "hidden",
    backgroundColor: color.paper,
    color: color.text,
  }),
});
