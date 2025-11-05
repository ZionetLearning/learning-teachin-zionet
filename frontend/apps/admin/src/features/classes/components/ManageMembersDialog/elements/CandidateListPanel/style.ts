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
      display: "flex",
      flexDirection: "column",
      "@media (max-width: 600px)": {
        minWidth: "100%",
      },
    },

    filtersRow: {
      display: "flex",
      alignItems: "center",
      gap: 8,
      marginBottom: 8,
      "@media (max-width: 600px)": {
        flexDirection: "column",
        alignItems: "stretch",
        gap: 6,
      },
    },

    roleSelect: {
      minWidth: 160,
      "@media (max-width: 600px)": {
        minWidth: "100%",
      },
    },

    toolbarRow: {
      display: "flex",
      gap: 8,
      marginBottom: 8,
      "@media (max-width: 600px)": {
        justifyContent: "space-between",
        flexWrap: "wrap",
      },
    },

    list: {
      borderRadius: 8,
      border: `1px solid ${color.divider}`,
      background: color.paper,
      maxHeight: 360,
      overflowY: "auto",
      overflowX: "hidden",
      "&::-webkit-scrollbar": { width: 8 },
      "&::-webkit-scrollbar-track": {
        backgroundColor: color.paper,
      },
      "&::-webkit-scrollbar-thumb": {
        backgroundColor: color.divider,
        borderRadius: 4,
        "&:hover": { backgroundColor: color.textMuted },
      },
      "@media (max-width: 600px)": {
        maxHeight: "min(60vh, 400px)",
      },
    },

    listItem: {},

    listItemDisabled: {
      opacity: 0.55,
      cursor: "not-allowed",
    },

    listItemButton: {
      paddingRight: 64,
      "&:hover": {
        backgroundColor: selectedBg,
      },
    },

    listItemButtonSelected: {
      backgroundColor: selectedBg,
    },

    listItemIcon: {
      minWidth: 40,
    },

    emptyBox: {
      padding: 16,
      textAlign: "center",
    },

    emptyText: {
      color: color.textMuted,
    },
  })();
};
