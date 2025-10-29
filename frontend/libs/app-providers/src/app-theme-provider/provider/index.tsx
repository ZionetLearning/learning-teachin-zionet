import { PropsWithChildren } from "react";
import { createTheme, ThemeProvider } from "@mui/material/styles";

const PRIMARY_MAIN = "#7c4dff";
const PRIMARY_DARK = "#6a40e6";

const theme = createTheme({
  cssVariables: {
    colorSchemeSelector: "data",
  },
  colorSchemes: {
    light: {
      palette: {
        mode: "light",
        primary: {
          main: PRIMARY_MAIN,
          dark: PRIMARY_DARK,
          contrastText: "#ffffff",
        },
        background: {
          default: "#f9f9fb",
          paper: "#ffffff",
        },
      },
    },
    dark: {
      palette: {
        mode: "dark",
        primary: {
          main: PRIMARY_MAIN,
          dark: PRIMARY_DARK,
          contrastText: "#ffffff",
        },
        background: {
          default: "#1e1f22",
          paper: "#2a2b32",
        },
        text: {
          primary: "#e8eaed",
          secondary: "#a1a6ad",
        },
      },
    },
  },
  components: {
    // optional: ensure buttons and links pick up hover correctly
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 8,
          textTransform: "capitalize",
        },
        containedPrimary: {
          "&:hover": {
            backgroundColor: PRIMARY_DARK,
          },
        },
      },
    },
    MuiLink: {
      styleOverrides: {
        root: {
          color: PRIMARY_MAIN,
          "&:hover": {
            color: PRIMARY_DARK,
          },
        },
      },
    },
  },
});

export const AppThemeProvider = ({ children }: PropsWithChildren) => {
  return <ThemeProvider theme={theme}>{children}</ThemeProvider>;
};
