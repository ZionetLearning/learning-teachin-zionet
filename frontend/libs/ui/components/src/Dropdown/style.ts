import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    wrapper: {
      display: "flex",
      flexDirection: "column",
      fontSize: 14,
      color: color.text,
      fontWeight: 500,

      // Label
      "&& .MuiInputLabel-root": {
        fontSize: 14,
        color: color.textMuted,
      },

      // Root of the input (OutlinedInput under the Select)
      "&& .MuiOutlinedInput-root": {
        background: color.paper,
        borderRadius: 8,
        transition: "border-color .2s, box-shadow .2s, background .3s",

        "& .MuiOutlinedInput-notchedOutline": {
          borderColor: color.divider,
        },

        "&:hover .MuiOutlinedInput-notchedOutline": {
          borderColor: color.divider,
        },

        "&.Mui-focused": {
          background: color.paper,
          // purple glow ring, same vibe as your dialog text fields
          boxShadow: `0 0 0 3px rgba(var(${color.primaryMainChannel}) / 0.25)`,
        },
        "&.Mui-focused .MuiOutlinedInput-notchedOutline": {
          borderColor: color.primary,
        },

        "&.Mui-disabled": {
          background: color.bg,
          color: color.textMuted,
        },

        // The visible "value" area inside the select
        "& .MuiSelect-select": {
          marginTop: 4,
          padding: "0.55rem 0.7rem",
          marginRight: 24,
          fontSize: 14,
          color: color.text,
          backgroundColor: "transparent",
        },

        "& .MuiSelect-select.MuiPlaceholder": {
          color: color.textMuted,
        },
      },

      // Helper/error text at the bottom
      "&& .MuiFormHelperText-root": {
        fontSize: 12,
        lineHeight: 1.4,
        marginLeft: 0,
        marginRight: 0,
        color: color.textMuted,
      },

      // Error state colors
      "&& .Mui-error .MuiOutlinedInput-notchedOutline": {
        borderColor: "#f87171",
      },
      "&& .Mui-error.MuiFormHelperText-root": {
        color: "#f87171",
      },
    },
  })();
};
