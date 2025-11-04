import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  const headerHover = color.primaryMainChannel
    ? `rgba(var(${color.primaryMainChannel}) / 0.06)`
    : color.hover;

  return createUseStyles({
    container: {
      padding: "16px",
      maxWidth: "1200px",
      margin: "0 auto",
      display: "flex",
      flexDirection: "column",
      overflow: "hidden",
      color: color.text,
      "@media (max-width: 600px)": {
        height: "calc(100vh - 25px)",
        padding: "12px",
      },
    },

    toolbar: {
      paddingInline: 0,
      display: "flex",
      justifyContent: "space-between",
      marginBottom: "16px",
    },

    title: { margin: 0 },

    tableContainer: {
      backgroundColor: color.paper,
      borderRadius: 8,
      overflow: "hidden",
      overflowX: "auto",
      border: `1px solid ${color.divider}`,
    },

    tableHead: {
      "& th": {
        fontWeight: 600,
        backgroundColor: headerHover,
        color: color.text,
        borderBottom: `1px solid ${color.divider}`,
      },
    },

    tableBody: {
      "& td": { borderBottom: `1px solid ${color.divider}` },
    },

    row: {
      "&:hover": { backgroundColor: headerHover },
    },

    actionsCell: {
      textAlign: "right",
      whiteSpace: "nowrap",
    },

    iconButton: {
      marginInlineStart: 4,
      "&:hover": { color: color.primary },
      "&:focus-visible": {
        outline: `2px solid ${color.primary}`,
        outlineOffset: 2,
        borderRadius: 8,
      },
    },

    loadingCell: {
      padding: "16px",
      textAlign: "center",
      color: color.textMuted,
    },
    errorCell: {
      padding: "16px",
      textAlign: "center",
      color: color.error, 
    },
    emptyCell: {
      padding: "16px",
      textAlign: "center",
      color: color.textMuted,
    },

    scrollable: {
      flex: 1,
      overflow: "auto",
      paddingInlineEnd: "4px",
      paddingBottom: "16px",
      "&::-webkit-scrollbar": { width: "8px" },
      "&::-webkit-scrollbar-track": {
        backgroundColor: color.paper,
        borderRadius: "4px",
      },
      "&::-webkit-scrollbar-thumb": {
        backgroundColor: color.divider,
        borderRadius: "4px",
        "&:hover": { backgroundColor: color.textMuted },
      },
    },
  })();
};
