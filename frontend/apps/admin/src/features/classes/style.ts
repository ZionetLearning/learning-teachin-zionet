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
      "@media (max-width: 900px)": {
        padding: "14px",
      },
      "@media (max-width: 600px)": {
        height: "calc(100vh - 25px)",
        padding: "12px",
      },
    },

    toolbar: {
      paddingInline: 0,
      display: "flex",
      justifyContent: "space-between",
      alignItems: "center",
      marginBottom: "16px",
      gap: 12,
      "@media (max-width: 600px)": {
        flexDirection: "column",
        alignItems: "stretch",
        gap: 8,
        marginBottom: 12,
      },
    },

    title: {
      margin: 0,
      "@media (max-width: 600px)": {
        fontSize: 18,
        lineHeight: 1.25,
      },
    },

    tableContainer: {
      backgroundColor: color.paper,
      borderRadius: 8,
      overflow: "hidden",
      border: `1px solid ${color.divider}`,
      overflowX: "auto",
      "@media (max-width: 600px)": {
        borderRadius: 10,
      },
    },

    tableHead: {
      "& th": {
        position: "sticky",
        top: 0,
        zIndex: 1,
        fontWeight: 600,
        backgroundColor: headerHover,
        color: color.text,
        borderBottom: `1px solid ${color.divider}`,
        "@media (max-width: 600px)": {
          fontSize: 13,
          paddingBlock: 10,
        },
      },
    },

    tableBody: {
      "& td": {
        borderBottom: `1px solid ${color.divider}`,
        "@media (max-width: 600px)": {
          fontSize: 13,
          paddingBlock: 10,
        },
      },
    },

    row: {
      "&:hover": { backgroundColor: headerHover },
    },

    actionsCell: {
      textAlign: "right",
      whiteSpace: "nowrap",
      "@media (max-width: 600px)": {
        whiteSpace: "normal",
      },
    },

    iconButton: {
      marginInlineStart: 4,
      "&:hover": { color: color.primary },
      "&:focus-visible": {
        outline: `2px solid ${color.primary}`,
        outlineOffset: 2,
        borderRadius: 8,
      },
      "@media (max-width: 600px)": {
        "& .MuiSvgIcon-root": { fontSize: 22 },
      },
    },

    loadingCell: {
      padding: "16px",
      textAlign: "center",
      color: color.textMuted,
      "@media (max-width: 600px)": { padding: "12px" },
    },

    errorCell: {
      padding: "16px",
      textAlign: "center",
      color: color.error,
      "@media (max-width: 600px)": { padding: "12px" },
    },

    emptyCell: {
      padding: "16px",
      textAlign: "center",
      color: color.textMuted,
      "@media (max-width: 600px)": { padding: "12px" },
    },

    scrollable: {
      flex: 1,
      overflow: "auto",
      paddingInlineEnd: "4px",
      paddingBottom: "16px",
      "&::-webkit-scrollbar": { width: "8px", height: "8px" },
      "&::-webkit-scrollbar-track": {
        backgroundColor: color.paper,
        borderRadius: "4px",
      },
      "&::-webkit-scrollbar-thumb": {
        backgroundColor: color.divider,
        borderRadius: "4px",
        "&:hover": { backgroundColor: color.textMuted },
      },
      "@media (max-width: 600px)": {
        paddingBottom: "12px",
      },
    },
  })();
};
