import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    creationContainer: {
      display: "flex",
      flexDirection: "column",
      gap: "1.25rem",
      background: color.paper,
      borderRadius: "12px",
      boxShadow: "0 4px 16px rgba(0,0,0,0.08)",
      border: `1px solid ${color.divider}`,
      padding: "1.5rem 2rem",
      height: "fit-content",
      "@media (min-width: 700px)": {
        minWidth: "350px",
        maxWidth: "400px",
      },
    },
    sectionTitle: {
      fontSize: "1.5rem",
      fontWeight: 600,
      color: color.text,
      marginBottom: "1rem",
      borderBottom: `2px solid rgba(var(${color.primaryMainChannel}) / 1)`,
      paddingBottom: "0.5rem",
    },
    label: {
      display: "flex",
      flexDirection: "column",
      gap: "0.5rem",
      fontSize: "0.9rem",
      fontWeight: 500,
      color: color.text,
      "& input, & textarea": {
        padding: "0.75rem",
        border: `1px solid ${color.divider}`,
        borderRadius: "8px",
        fontSize: "1rem",
        backgroundColor: color.paper,
        color: color.text,
        transition: "all 0.3s ease",
        "&:focus": {
          outline: "none",
          borderColor: color.primary,
          boxShadow: `0 0 0 3px rgba(var(${color.primaryMainChannel}) / 0.1)`,
        },
        "&:disabled": {
          backgroundColor: color.bg,
          color: color.textMuted,
          cursor: "not-allowed",
        },
      },
      "& textarea": {
        resize: "vertical",
        minHeight: "100px",
        fontFamily: "inherit",
      },
    },
    error: {
      color: "red",
      fontSize: "0.8rem",
      minHeight: "1rem",
      display: "flex",
      alignItems: "center",
    },
    statusLabel: {
      fontSize: "0.9rem",
      fontWeight: 500,
      color: color.textMuted,
      marginRight: "0.5rem",
    },
    statusValue: {
      fontSize: "0.9rem",
      fontWeight: 600,
      textTransform: "capitalize",
    },
    statusConnected: {
      color: "#28a745",
    },
    statusDisconnected: {
      color: "#dc3545",
    },
    buttonGroup: {
      display: "flex",
      gap: "0.75rem",
      marginTop: "1rem",
    },
    submitButton: {
      flex: 1,
      padding: "0.875rem 1.5rem",
      backgroundColor: color.primary,
      color: color.primaryContrast,
      border: "none",
      borderRadius: "8px",
      fontSize: "1rem",
      fontWeight: 600,
      cursor: "pointer",
      transition: "all 0.3s ease",
      "&:hover:not(:disabled)": {
        backgroundColor: `rgba(var(${color.primaryMainChannel}) / 0.9)`,
        transform: "translateY(-2px)",
        boxShadow: `0 4px 12px rgba(var(${color.primaryMainChannel}) / 0.3)`,
      },
      "&:disabled": {
        backgroundColor: "rgba(var(${color.primaryMainChannel}) / 0.35)",
        cursor: "not-allowed",
        transform: "none",
        boxShadow: "none",
      },
    },
    cancelButton: {
      flex: 1,
      padding: "0.875rem 1.5rem",
      backgroundColor: "rgba(var(${color.primaryMainChannel}) / 0.15)",
      color: color.text,
      border: "none",
      borderRadius: "8px",
      fontSize: "1rem",
      fontWeight: 600,
      cursor: "pointer",
      transition: "all 0.3s ease",
      "&:hover": {
        backgroundColor: "rgba(var(${color.primaryMainChannel}) / 0.25)",
        transform: "translateY(-2px)",
        boxShadow: "0 4px 12px rgba(0,0,0,0.1)",
      },
    },
    createNewButton: {
      flex: 1,
      padding: "0.875rem 1.5rem",
      backgroundColor: "#27ae60",
      color: "white",
      border: "none",
      borderRadius: "8px",
      fontSize: "1rem",
      fontWeight: 600,
      cursor: "pointer",
      transition: "all 0.3s ease",
      "&:hover": {
        backgroundColor: "#229954",
        transform: "translateY(-2px)",
        boxShadow: "0 4px 12px rgba(39,174,96,0.3)",
      },
    },
  })();
};
