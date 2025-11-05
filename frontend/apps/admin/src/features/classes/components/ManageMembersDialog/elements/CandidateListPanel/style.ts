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
    },

    filtersRow: {
      display: "flex",
      alignItems: "center",
      gap: 8,
      marginBottom: 8,
    },
    roleSelect: {
      minWidth: 160,
    },

    toolbarRow: {
      display: "flex",
      gap: 8,
      marginBottom: 8,
    },

    list: {
      borderRadius: 8,
      border: `1px solid ${color.divider}`,
      background: color.paper,
      maxHeight: 360,
      overflow: "auto",
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
    },
    emptyText: {
      color: color.textMuted,
    },
  })();
};
