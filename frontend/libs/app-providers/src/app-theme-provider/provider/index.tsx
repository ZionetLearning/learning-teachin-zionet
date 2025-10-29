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
    MuiSelect: {
      styleOverrides: {
        // make inner area transparent so the root bg shows uniformly
        select: {
          backgroundColor: "transparent",
        },
        icon: ({ theme }) => ({
          color: theme.vars.palette.text.secondary,
        }),
      },
      defaultProps: {
        MenuProps: {
          PaperProps: {
            sx: {
              bgcolor: "background.paper",
              color: "text.primary",
              border: "1px solid",
              borderColor: "divider",
            },
          },
        },
      },
    },
    MuiOutlinedInput: {
      styleOverrides: {
        root: ({ theme }) => ({
          backgroundColor: theme.vars.palette.background.paper,
          borderRadius: 8,
          // outline colors
          "& .MuiOutlinedInput-notchedOutline": {
            borderColor: theme.vars.palette.divider,
          },
          "&:hover .MuiOutlinedInput-notchedOutline": {
            borderColor: theme.vars.palette.divider,
          },
          "&.Mui-focused .MuiOutlinedInput-notchedOutline": {
            borderColor: theme.vars.palette.primary.main,
          },
        }),
        input: ({ theme }) => ({
          color: theme.vars.palette.text.primary,
        }),
      },
    },
    MuiMenu: {
      styleOverrides: {
        paper: ({ theme }) => ({
          backgroundColor: theme.vars.palette.background.paper,
          color: theme.vars.palette.text.primary,
          border: `1px solid ${theme.vars.palette.divider}`,
        }),
      },
    },
  },
});

export const AppThemeProvider = ({ children }: PropsWithChildren) => {
  return <ThemeProvider theme={theme}>{children}</ThemeProvider>;
};
