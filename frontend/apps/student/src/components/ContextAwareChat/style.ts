import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

interface StyleProps {
  isRTL: boolean;
  hasSettings: boolean;
}

export const useStyles = (props: StyleProps) => {
  const color = useThemeColors();

  return createUseStyles({
    floatingButton: {
      position: "fixed",
      bottom: ({ hasSettings }: StyleProps) =>
        hasSettings ? "calc(16px + 60px + 16px)" : "24px",
      right: ({ isRTL }: StyleProps) => (isRTL ? "auto" : "24px"),
      left: ({ isRTL }: StyleProps) => (isRTL ? "24px" : "auto"),
      zIndex: 1000,
      backgroundColor: color.primary,
      boxShadow: `0 8px 18px rgba(var(${color.primaryMainChannel}) / 0.28), 0 3px 8px rgba(0,0,0,0.12)`,
      "&:hover": {
        backgroundColor: color.primaryDark,
        boxShadow: `0 10px 22px rgba(var(${color.primaryMainChannel}) / 0.34), 0 4px 10px rgba(0,0,0,0.14)`,
      },
    },
    drawer: {
      "& .MuiDrawer-paper": {
        width: "450px",
        maxWidth: "90vw",
        borderLeft: ({ isRTL }: StyleProps) =>
          isRTL ? "none" : `1px solid ${color.divider}`,
        borderRight: ({ isRTL }: StyleProps) =>
          isRTL ? `1px solid ${color.divider}` : "none",
        backgroundColor: color.bg,
        "@media (max-width: 768px)": {
          width: "100vw",
          maxWidth: "100vw",
        },
      },
    },
    chatContainer: {
      display: "flex",
      flexDirection: "column",
      height: "100%",
      backgroundColor: color.bg,
    },
    header: {
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between",
      padding: "16px 20px",
      borderBottom: `1px solid ${color.divider}`,
      background: `linear-gradient(180deg, rgba(var(${color.primaryMainChannel}) / 0.08) 0%, ${color.paper} 100%)`,
      boxShadow: `0 1px 3px rgba(0, 0, 0, 0.05)`,
      minHeight: "64px",
    },
    headerTitle: {
      fontWeight: 600,
      color: color.text,
      fontSize: "18px",
    },
    closeButton: {
      padding: 8,
      color: color.textMuted,
      "&:hover": {
        backgroundColor: color.hover,
        color: color.text,
      },
    },
    contextInfo: {
      padding: "12px 20px",
      backgroundColor: `rgba(var(${color.primaryMainChannel}) / 0.10)`,
      borderBottom: `1px solid rgba(var(${color.primaryMainChannel}) / 0.25)`,
      fontSize: "13px",
      color: color.primary,
      fontWeight: 500,
      direction: ({ isRTL }: StyleProps) => (isRTL ? "rtl" : "ltr"),
    },
    messagesContainer: {
      flex: 1,
      overflowY: "auto",
      overflowX: "hidden",
      padding: "20px",
      display: "flex",
      flexDirection: "column",
      gap: "16px",
      backgroundColor: `rgba(var(${color.primaryMainChannel}) / 0.03)`,
      minHeight: 0,
    },
    message: {
      maxWidth: "85%",
      padding: "12px 16px",
      borderRadius: 12,
      position: "relative",
      wordWrap: "break-word",
      boxShadow: "0 1px 2px rgba(0, 0, 0, 0.08)",
      direction: ({ isRTL }: StyleProps) => (isRTL ? "rtl" : "ltr"),
      lineHeight: 1.6,
      fontSize: "14px",
    },
    userMessage: {
      alignSelf: ({ isRTL }: StyleProps) => (isRTL ? "flex-start" : "flex-end"),
      backgroundColor: color.primary,
      color: color.primaryContrast,
      borderBottomRightRadius: ({ isRTL }: StyleProps) => (isRTL ? 12 : 4),
      borderBottomLeftRadius: ({ isRTL }: StyleProps) => (isRTL ? 4 : 12),
    },
    assistantMessage: {
      alignSelf: ({ isRTL }: StyleProps) => (isRTL ? "flex-end" : "flex-start"),
      backgroundColor: color.paper,
      color: color.text,
      borderBottomLeftRadius: ({ isRTL }: StyleProps) => (isRTL ? 12 : 4),
      borderBottomRightRadius: ({ isRTL }: StyleProps) => (isRTL ? 4 : 12),
      border: `1px solid ${color.divider}`,
    },
    inputContainer: {
      borderTop: `1px solid ${color.divider}`,
      padding: "16px 20px",
      backgroundColor: color.paper,
      boxShadow: `0 -2px 8px rgba(0, 0, 0, 0.05)`,
      display: "flex",
      gap: "8px",
      alignItems: "flex-end",
      direction: ({ isRTL }: StyleProps) => (isRTL ? "rtl" : "ltr"),
    },
    input: {
      flex: 1,
      "& .MuiOutlinedInput-root": {
        borderRadius: "12px",
        backgroundColor: `rgba(var(${color.primaryMainChannel}) / 0.05)`,
        "& fieldset": {
          borderColor: `rgba(var(${color.primaryMainChannel}) / 0.32)`,
        },
        "&:hover": {
          backgroundColor: color.paper,
          "& fieldset": {
            borderColor: color.primary,
          },
        },
        "&.Mui-focused": {
          backgroundColor: color.paper,
          boxShadow: `0 0 0 2px rgba(var(${color.primaryMainChannel}) / 0.16)`,
          "& .MuiOutlinedInput-notchedOutline": {
            borderColor: color.primary,
            borderWidth: "2px",
          },
        },
      },
      "& .MuiInputBase-input": {
        color: color.text,
      },
    },
    sendButton: {
      minWidth: "auto",
      padding: "8px",
      borderRadius: "12px",
      backgroundColor: color.primary,
      color: color.primaryContrast,
      boxShadow: `0 4px 12px rgba(var(${color.primaryMainChannel}) / 0.28)`,
      "&:hover": {
        backgroundColor: color.primaryDark,
        boxShadow: `0 6px 16px rgba(var(${color.primaryMainChannel}) / 0.34)`,
      },
      "&:disabled": {
        backgroundColor: `rgba(var(${color.primaryMainChannel}) / 0.35)`,
        color: color.primaryContrast,
        opacity: 0.6,
      },
    },
    emptyState: {
      display: "flex",
      flexDirection: "column",
      alignItems: "center",
      justifyContent: "center",
      height: "100%",
      padding: "40px 24px",
      textAlign: "center",
    },
    emptyStateIcon: {
      fontSize: 48,
      marginBottom: 16,
      opacity: 0.3,
      color: color.primary,
    },
    emptyStateText: {
      fontSize: "14px",
      color: color.textMuted,
    },
  })(props);
};
