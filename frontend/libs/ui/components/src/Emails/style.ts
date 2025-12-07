import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    rootWrapper: {
      height: "100dvh",
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      width: "100%",
    },
    root: {
      display: "grid",
      gridTemplateColumns: "320px 1fr",
      gap: 16,
      "@media (max-width: 900px)": {
        gridTemplateColumns: "1fr",
      },
    },

    // LEFT: classes list
    sidebar: {
      overflow: "hidden",
      borderRadius: 14,
    },
    sidebarHeader: {
      position: "sticky",
      top: 0,
      zIndex: 1,
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between",
      padding: "14px 14px",
      background: `linear-gradient(
        180deg,
        rgba(var(${color.primaryMainChannel}) / 0.10) 0%,
        rgba(var(${color.primaryMainChannel}) / 0.04) 100%
      )`,
      borderBottom: `1px solid ${color.divider}`,
    },
    sidebarTitle: {
      color: color.primary,
      fontWeight: 700,
      letterSpacing: 0.2,
    },
    list: {
      padding: 6,
    },
    listItem: {
      borderRadius: 12,
      margin: "6px 6px",
      paddingInline: 10,
      "& .MuiListItemText-primary": {
        fontWeight: 600,
      },
      "& .MuiListItemText-secondary": {
        color: color.textMuted,
      },
      "&.Mui-selected": {
        background: `rgba(var(${color.primaryMainChannel}) / 0.12)`,
        boxShadow: `inset 0 0 0 1px rgba(var(${color.primaryMainChannel}) / 0.28)`,
      },
      "&:hover": {
        background: `rgba(var(${color.primaryMainChannel}) / 0.08)`,
      },
    },

    // RIGHT: details
    panel: {
      borderRadius: 14,
      padding: 16,
      width: 500,
    },
    headerRow: {
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between",
      gap: 12,
      marginBottom: 8,
      "&[dir='rtl']": { flexDirection: "row-reverse" },
    },
    className: {
      fontSize: 22,
      fontWeight: 800,
      color: color.text,
    },
    subtle: {
      color: color.textMuted,
      fontWeight: 600,
    },

    sectionGrid: {
      display: "grid",
      gridTemplateColumns: "repeat(2, minmax(0, 1fr))",
      gap: 16,
      "@media (max-width: 900px)": {
        gridTemplateColumns: "1fr",
      },
    },

    sectionCard: {
      borderRadius: 14,
      padding: 12,
      border: `1px solid ${color.divider}`,
      background: `linear-gradient(
        180deg,
        rgba(255,255,255,0.02),
        rgba(0,0,0,0.02)
      )`,
    },
    sectionHeader: {
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between",
      marginBottom: 6,
      "&[dir='rtl']": { flexDirection: "row-reverse" },
    },
    sectionTitle: {
      display: "flex",
      alignItems: "center",
      gap: 8,
      fontWeight: 700,
      color: color.text,
    },
    countChip: {
      borderRadius: 10,
      fontSize: 12,
      padding: "2px 8px",
      background: `rgba(var(${color.primaryMainChannel}) / 0.12)`,
      color: color.primary,
      border: `1px solid rgba(var(${color.primaryMainChannel}) / 0.3)`,
    },

    memberList: {
      paddingTop: 4,
      "& .MuiListItem-root": {
        borderRadius: 10,
        marginBlock: 4,
      },
    },
    memberAvatar: {
      width: 32,
      height: 32,
      fontWeight: 700,
      marginInlineEnd: 10,
      "&[dir='rtl']": { marginInlineEnd: 0, marginInlineStart: 10 },
      background: `rgba(var(${color.primaryMainChannel}) / 0.16)`,
      color: color.primary,
    },
    memberName: {
      fontWeight: 600,
    },
    emptyNote: {
      marginTop: 6,
      color: color.textMuted,
      fontStyle: "italic",
    },

    centerState: {
      minHeight: 240,
      display: "flex",
      flexDirection: "column",
      gap: 12,
      alignItems: "center",
      justifyContent: "center",
      textAlign: "center",
      color: color.textMuted,
    },
    divider: {
      margin: "12px 0",
      borderColor: color.divider,
    },
    updatingLine: {
      marginTop: 6,
      height: 3,
      borderRadius: 999,
      background: `linear-gradient(90deg,
        rgba(var(${color.primaryMainChannel}) / 0.2),
        rgba(var(${color.primaryMainChannel}) / 0.6),
        rgba(var(${color.primaryMainChannel}) / 0.2)
      )`,
      animation: "$shine 1.2s linear infinite",
    },
    "@keyframes shine": {
      "0%": { filter: "brightness(0.9)" },
      "50%": { filter: "brightness(1.3)" },
      "100%": { filter: "brightness(0.9)" },
    },
  })();
};
