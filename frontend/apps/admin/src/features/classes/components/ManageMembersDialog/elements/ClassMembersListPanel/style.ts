import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  const selectedBg = color.primaryMainChannel
    ? `rgba(var(${color.primaryMainChannel}) / 0.08)`
    : color.selected;

  return createUseStyles({
    container: {
      flex: 1,
      minWidth: 320,
      color: color.text,
      display: "flex",
      flexDirection: "column",
    },

    title: {
      marginBottom: 8,
      fontWeight: 600,
      color: color.text,
    },

    toolbar: {
      display: "flex",
      flexDirection: "row",
      gap: 8,
      marginBottom: 8,
    },

    listContainer: {
      padding: 10,
      borderRadius: 8,
      border: `1px solid ${color.divider}`,
      backgroundColor: color.paper,
      maxHeight: 360,
      overflowY: "auto",
      overflowX: "hidden",
      whiteSpace: "normal",
      "&::-webkit-scrollbar-track": {
        backgroundColor: color.paper,
        borderRadius: 4,
      },
      "&::-webkit-scrollbar-thumb": {
        backgroundColor: color.divider,
        borderRadius: 4,
        "&:hover": {
          backgroundColor: color.textMuted,
        },
      },
    },

    listItemButton: {
      paddingRight: 32,
      "&.selected": {
        backgroundColor: selectedBg,
      },
    },

    listItemIcon: {
      minWidth: 40,
    },

    emptyState: {
      padding: 16,
      textAlign: "center",
      color: color.textMuted,
      fontSize: 14,
    },
  })();
};
