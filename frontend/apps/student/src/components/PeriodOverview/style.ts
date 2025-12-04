import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    container: {
      width: "100%",
      boxSizing: "border-box",
      background: color.paper,
      border: `1px solid ${color.divider}`,
      borderRadius: 12,
      padding: 14,
      height: "100%",
      display: "flex",
      flexDirection: "column",
    },
    title: {
      fontSize: 16,
      fontWeight: 600,
      marginBottom: 8,
      color: color.text,
    },
    statsGrid: {
      display: "grid",
      gridTemplateColumns: "repeat(2, 1fr)",
      gap: 8,
      "@media (max-width: 480px)": {
        gridTemplateColumns: "1fr",
      },
    },
    statCard: {
      height: "100%",
      borderRadius: 12,
      background: color.bg,
      border: `1px solid ${color.divider}`,
      boxShadow: "0 2px 6px rgba(0, 0, 0, 0.08)",
      transition: "transform 0.2s, box-shadow 0.2s",
      "&:hover": {
        transform: "translateY(-2px)",
        boxShadow: "0 4px 12px rgba(0, 0, 0, 0.12)",
      },
    },
    statCardContent: {
      display: "flex",
      flexDirection: "column",
      alignItems: "center",
      padding: 6,
    },
    statIconContainer: {
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      width: 34,
      height: 34,
      borderRadius: 8,
      marginBottom: 6,
      "& svg": {
        fontSize: 18,
      },
    },
    statTextContainer: {
      textAlign: "center",
    },
    statValue: {
      fontSize: 22,
      fontWeight: 700,
      marginBottom: 2,
      color: color.text,
    },
    statLabel: {
      fontSize: 11,
      color: color.textMuted,
    },
  })();
};

