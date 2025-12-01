import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

interface StyleProps {
  isUnlocked: boolean;
}

export const useStyles = (props: StyleProps) => {
  const color = useThemeColors();

  return createUseStyles({
    container: {
      display: "flex",
      flexDirection: "column",
      alignItems: "center",
      padding: 16,
      borderRadius: 12,
      background: props.isUnlocked
        ? `linear-gradient(135deg, rgba(var(${color.primaryMainChannel}) / 0.12) 0%, rgba(var(${color.primaryMainChannel}) / 0.06) 100%)`
        : `rgba(var(${color.primaryMainChannel}) / 0.04)`,
      border: `1px solid ${
        props.isUnlocked
          ? `rgba(var(${color.primaryMainChannel}) / 0.3)`
          : `rgba(var(${color.primaryMainChannel}) / 0.15)`
      }`,
      transition: "all 0.3s ease",
      cursor: "pointer",
      "&:hover": {
        transform: "translateY(-2px)",
        boxShadow: props.isUnlocked
          ? `0 8px 16px rgba(var(${color.primaryMainChannel}) / 0.2)`
          : `0 4px 8px rgba(0, 0, 0, 0.1)`,
      },
    },
    iconContainer: {
      position: "relative",
      marginBottom: 12,
    },
    icon: {
      fontSize: 48,
      color: props.isUnlocked ? color.primary : color.textMuted,
      opacity: props.isUnlocked ? 1 : 0.4,
      filter: props.isUnlocked
        ? `drop-shadow(0 2px 8px rgba(var(${color.primaryMainChannel}) / 0.3))`
        : "none",
    },
    lockIcon: {
      position: "absolute",
      top: "50%",
      left: "50%",
      transform: "translate(-50%, -50%)",
      fontSize: 24,
      color: color.textMuted,
    },
    name: {
      fontSize: 14,
      fontWeight: 600,
      color: props.isUnlocked ? color.text : color.textMuted,
      textAlign: "center",
      marginBottom: 4,
    },
    date: {
      fontSize: 11,
      color: color.textMuted,
      textAlign: "center",
    },
  })();
};
