import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    card: {
      width: "100%",
      boxSizing: "border-box",
      background: color.paper,
      border: `1px solid ${color.divider}`,
      borderRadius: 12,
      padding: 10,
      display: "flex",
      flexDirection: "column",
      gap: 8,
    },
    dateRange: {
      color: color.primary,
      fontWeight: 500,
      marginBottom: 10,
      fontSize: 14,
    },
    contentLayout: {
      display: "grid",
      gridTemplateColumns: "minmax(110px, 160px) 1fr",
      gap: 14,
      alignItems: "start",
      "@media (max-width: 768px)": {
        gridTemplateColumns: "1fr",
      },
    },
    buttonGroup: {
      display: "flex",
      flexDirection: "column",
      gap: 6,
      minWidth: 0,
      "& button": {
        outline: "none !important",
      },
      "@media (max-width: 768px)": {
        flexDirection: "row",
        flexWrap: "wrap",
        gap: 6,
        "& > *": {
          flex: "1 1 calc(50% - 6px)",
        },
      },
    },
    calendarSection: {
      display: "flex",
      flexDirection: "column",
      background: color.bg,
      border: `1px solid ${color.divider}`,
      borderRadius: 12,
      padding: 4,
      boxShadow: "0 2px 6px rgba(0, 0, 0, 0.08)",
      minWidth: 0,
      overflow: "hidden",
    },
    calendarHeader: {
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between",
      marginBottom: 8,
    },
    monthYear: {
      fontWeight: 600,
      color: color.text,
      fontSize: 14,
    },
    calendar: {
      width: "100%",
    },
    weekDays: {
      display: "grid",
      gridTemplateColumns: "repeat(7, minmax(0, max-content))",
      justifyContent: "center",
      gap: 9,
      marginBottom: 4,
      "@media (max-width: 480px)": {
        gridTemplateColumns: "repeat(7, 1fr)",
        gap: 2,
      },
    },
    weekDay: {
      textAlign: "center",
      fontWeight: 600,
      color: color.textMuted,
      fontSize: 9,
      minWidth: 28,
    },
    week: {
      display: "grid",
      gridTemplateColumns: "repeat(7, minmax(0, max-content))",
      justifyContent: "center",
      gap: 4,
      marginBottom: 4,
      "@media (max-width: 480px)": {
        gridTemplateColumns: "repeat(7, 1fr)",
        gap: 2,
      },
    },
    day: {
      width: 34,
      height: 34,
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      borderRadius: 6,
      cursor: "pointer",
      fontSize: 10,
      color: color.text,
      transition: "all 0.2s",
      "&:hover:not($emptyDay)": {
        backgroundColor: color.hover,
      },
    },
    selectedDay: {
      backgroundColor: color.primary,
      color: "#ffffff",
      fontWeight: 600,
      "&:hover": {
        backgroundColor: color.primaryDark,
      },
    },
    today: {
      border: `1px solid ${color.primary}`,
      fontWeight: 600,
    },
    emptyDay: {
      cursor: "default",
      "&:hover": {
        backgroundColor: "transparent",
      },
    },
    futureDay: {
      opacity: 0.4,
      cursor: "not-allowed",
      "&:hover": {
        backgroundColor: "transparent",
      },
    },
  })();
};

