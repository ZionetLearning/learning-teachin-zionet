import { useTheme } from "@mui/material/styles";

export const useThemeColors = () => {
  const theme = useTheme();

  const vars = theme.vars ?? {
    palette: {
      background: { default: "#fff", paper: "#fff" },
      text: { primary: "#000", secondary: "#555" },
      primary: { main: "#7c4dff", dark: "#6a40e6", contrastText: "#fff" },
      action: { hover: "rgba(0,0,0,0.04)", selected: "rgba(0,0,0,0.08)" },
      divider: "rgba(0,0,0,0.12)",
    },
  };

  const primaryMainChannel = "--mui-palette-primary-mainChannel";

  return {
    bg: vars.palette.background.default,
    paper: vars.palette.background.paper,
    text: vars.palette.text.primary,
    textMuted: vars.palette.text.secondary,
    primary: vars.palette.primary.main,
    primaryDark: vars.palette.primary.dark,
    primaryContrast: vars.palette.primary.contrastText,
    hover: vars.palette.action.hover,
    selected: vars.palette.action.selected,
    divider: vars.palette.divider,
    primaryMainChannel,
  };
};
