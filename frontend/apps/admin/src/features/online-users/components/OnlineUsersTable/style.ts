import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    desktopTable: {
      flex: 1,
      display: "flex",
      flexDirection: "column",
      overflow: "hidden",
      "@media (max-width: 600px)": {
        display: "none",
      },
    },
    tableContainer: {
      borderRadius: "8px",
      border: `1px solid ${color.divider}`,
      backgroundColor: color.paper,
      overflow: "auto",
      maxHeight: "calc(100vh - 200px)",
      "&::-webkit-scrollbar": {
        width: "8px",
      },
      "&::-webkit-scrollbar-track": {
        backgroundColor: `rgba(var(${color.primaryMainChannel}) / 0.04)`,
        borderRadius: "4px",
      },
      "&::-webkit-scrollbar-thumb": {
        backgroundColor: `rgba(var(${color.primaryMainChannel}) / 0.18)`,
        borderRadius: "4px",
        "&:hover": {
          backgroundColor: `rgba(var(${color.primaryMainChannel}) / 0.25)`,
        },
      },
    },
    tableHead: {
      backgroundColor: `rgba(var(${color.primaryMainChannel}) / 0.06)`,
      position: "sticky",
      top: 0,
      zIndex: 10,
      "& .MuiTableCell-head": {
        fontWeight: "600",
        fontSize: "14px",
        padding: "12px 16px",
        color: color.text,
        backgroundColor: `rgba(var(${color.primaryMainChannel}) / 0.06)`,
      },
    },
  })();
};
