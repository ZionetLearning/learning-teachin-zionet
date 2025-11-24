import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    container: {
      padding: "16px",
      maxWidth: "1200px",
      margin: "0 auto",
      display: "flex",
      flexDirection: "column",
      overflow: "hidden",
      "@media (max-width: 600px)": {
        height: "calc(100vh - 25px)",
        padding: "12px",
      },
    },

    loadingContainer: {
      display: "flex",
      flexDirection: "column",
      alignItems: "center",
      justifyContent: "center",
      minHeight: "200px",
      gap: "16px",
    },

    errorContainer: {
      padding: "20px",
      textAlign: "center",
    },

    emptyState: {
      textAlign: "center",
      padding: "60px 20px",
    },

    emptyIcon: {
      fontSize: "48px",
      marginBottom: "16px",
    },

    header: {
      marginBottom: "24px",
    },

    title: {
      marginBottom: "8px",
    },

    subtitle: {
      marginBottom: "16px",
    },

    contentArea: {
      flex: 1,
      overflow: "hidden",
      display: "flex",
      flexDirection: "column",
    },

    scrollableContainer: {
      flex: 1,
      overflow: "auto",
      paddingInlineEnd: "4px",
      paddingBottom: "16px",

      "&::-webkit-scrollbar": {
        width: "8px",
      },
      "&::-webkit-scrollbar-track": {
        backgroundColor: color.paper,
        borderRadius: "4px",
      },
      "&::-webkit-scrollbar-thumb": {
        backgroundColor: color.divider,
        borderRadius: "4px",
        "&:hover": {
          backgroundColor: color.textMuted,
        },
      },
    },

    mobileCardList: {
      display: "none",
      "@media (max-width: 600px)": {
        display: "block",
        flex: 1,
        overflow: "auto",
        paddingInlineEnd: "4px",
        paddingBottom: "24px",

        "&::-webkit-scrollbar": {
          width: "6px",
        },
        "&::-webkit-scrollbar-track": {
          backgroundColor: color.paper,
          borderRadius: "3px",
        },
        "&::-webkit-scrollbar-thumb": {
          backgroundColor: color.divider,
          borderRadius: "3px",
          "&:hover": {
            backgroundColor: color.textMuted,
          },
        },
      },
    },
  })();
};
