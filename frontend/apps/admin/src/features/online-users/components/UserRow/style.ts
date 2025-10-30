import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    tableRow: {
      "&:hover": {
        backgroundColor: `rgba(var(${color.primaryMainChannel}) / 0.05)`,
      },
      "& .MuiTableCell-root": {
        padding: "12px 16px",
        borderBottom: `1px solid ${color.divider}`,
        color: color.text,
      },
    },
    userInfo: {
      display: "flex",
      alignItems: "center",
    },
    avatar: {
      width: "32px",
      height: "32px",
      fontSize: "14px",
      marginInlineEnd: "12px",
    },
    userName: {
      fontWeight: "500",
      fontSize: "14px",
      color: color.text,
    },
    roleChip: {
      height: "24px",
      fontSize: "12px",
      textTransform: "capitalize",
      color: color.primaryContrast,
      fontWeight: "500",
      backgroundColor: color.primary,
      "& .MuiChip-label": {
        paddingInline: "8px",
      },
    },
    onlineChip: {
      height: "24px",
      fontSize: "12px",
      "& .MuiChip-label": {
        paddingInline: "8px",
      },
    },
    connectionCount: {
      fontSize: "14px",
      color: color.textMuted,
    },
    mobileCard: {
      marginBottom: "12px",
      borderRadius: "8px",
      border: `1px solid ${color.divider}`,
      backgroundColor: color.paper,
      "&:last-child": {
        marginBottom: "16px",
      },
    },
    mobileCardContent: {
      padding: "16px !important",
      "&:last-child": {
        paddingBottom: "16px !important",
      },
    },
    mobileUserHeader: {
      display: "flex",
      alignItems: "center",
      marginBottom: "12px",
    },
    mobileUserInfo: {
      flex: 1,
      marginInlineStart: "12px",
    },
    mobileUserName: {
      fontWeight: "500",
      fontSize: "16px",
      marginBottom: "4px",
      color: color.text,
    },
    mobileConnectionInfo: {
      display: "flex",
      justifyContent: "space-between",
      alignItems: "center",
      marginTop: "12px",
      paddingTop: "12px",
      borderTop: `1px solid ${color.divider}`,
    },
  })();
};
